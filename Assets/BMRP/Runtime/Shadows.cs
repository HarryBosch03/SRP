using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public class Shadows
    {
        private const string BufferName = "Shadows";

        private const int
            MaxShadowedDirectionalLightCount = 4,
            MaxShadowedOtherLightCount = 16,
            MaxCascades = 4;

        private int
            shadowedDirectionalLightCount,
            shadowedOtherLightCount;

        private Vector4 atlasSizes;

        private readonly CommandBuffer buffer = new CommandBuffer
        {
            name = BufferName
        };

        private ScriptableRenderContext context;
        private CullingResults cullingResults;
        private ShadowSettings settings;

        private readonly ShadowedDirectionalLight[] shadowedDirectionalLights =
            new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];

        private static readonly int
            DirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
            OtherShadowAtlasId = Shader.PropertyToID("_OtherShadowAtlas");

        private static readonly ShaderInt CascadeCount = new("_CascadeCount");
        private static readonly ShaderFloat ShadowPancaking = new("_ShadowPancaking");

        private static readonly ShaderVector
            ShadowAtlasSize = new("_ShadowAtlasSize"),
            ShadowDistanceFade = new("_ShadowDistanceFade");

        private static readonly ShaderVectorArray
            CascadeCullingSpheres = new("_CascadeCullingSpheres", MaxCascades),
            CascadeData = new("_CascadeData", MaxCascades),
            OtherShadowTiles = new("_OtherShadowTiles", MaxShadowedOtherLightCount);

        private static readonly ShaderMatrixArray
            DirShadowMatrices = new("_DirectionalShadowMatrices", MaxShadowedDirectionalLightCount * MaxCascades),
            OtherShadowMatrices = new("_OtherShadowMatrices", MaxShadowedOtherLightCount);

        private static readonly string[]
            DirectionalFilterKeywords =
            {
                "_DIRECTIONAL_PCF3",
                "_DIRECTIONAL_PCF5",
                "_DIRECTIONAL_PCF7",
            },
            CascadeBlendKeywords =
            {
                "_CASCADE_BLEND_SOFT",
                "_CASCADE_BLEND_DITHER",
            },
            ShadowMaskKeywords =
            {
                "_SHADOW_MASK_ALWAYS",
                "_SHADOW_MASK_DISTANCE",
            },
            OtherFilterKeywords =
            {
                "_OTHER_PCF3",
                "_OTHER_PCF5",
                "_OTHER_PCF7",
            };

        private ShadowedOtherLight[] shadowedOtherLights = new ShadowedOtherLight[MaxShadowedOtherLightCount];

        private bool useShadowMask;

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
        {
            this.context = context;
            this.cullingResults = cullingResults;
            this.settings = settings;

            shadowedDirectionalLightCount = 0;
            shadowedOtherLightCount = 0;

            useShadowMask = false;
        }

        private void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        public Vector4 ReserveDirectionalShadows(Light light, int visibleLightIndex)
        {
            if (shadowedDirectionalLightCount >= MaxShadowedDirectionalLightCount) return Vector4.zero;
            if (light.shadows == LightShadows.None) return Vector4.zero;
            if (light.shadowStrength <= 0.0f) return Vector4.zero;

            var maskChannel = -1;
            var lightBaking = light.bakingOutput;
            if (lightBaking is
                { lightmapBakeType: LightmapBakeType.Mixed, mixedLightingMode: MixedLightingMode.Shadowmask })
            {
                useShadowMask = true;
                maskChannel = lightBaking.occlusionMaskChannel;
            }

            if (!cullingResults.GetShadowCasterBounds(visibleLightIndex, out var bounds))
            {
                return new Vector4(-light.shadowStrength, 0.0f, 0.0f, maskChannel);
            }

            shadowedDirectionalLights[shadowedDirectionalLightCount] =
                new ShadowedDirectionalLight
                {
                    visibleLightIndex = visibleLightIndex,
                    slopeScaleBias = light.shadowBias,
                    nearPlaneOffset = light.shadowNearPlane,
                };

            return new Vector4()
            {
                x = light.shadowStrength,
                y = settings.directional.cascadeCount * shadowedDirectionalLightCount++,
                z = light.shadowNormalBias,
                w = maskChannel
            };
        }

        public Vector4 ReserveOtherShadows(Light light, int visibleLightIndex)
        {
            if (light.shadows == LightShadows.None) return Vector4.zero;
            if (light.shadowStrength <= 0.0f) return Vector4.zero;

            var maskChannel = -1.0f;
            var output = light.bakingOutput;

            if (output.lightmapBakeType != LightmapBakeType.Mixed &&
                output.mixedLightingMode != MixedLightingMode.Shadowmask)
            {
                useShadowMask = true;
                maskChannel = output.occlusionMaskChannel;
            }

            if (shadowedOtherLightCount >= MaxShadowedOtherLightCount ||
                !cullingResults.GetShadowCasterBounds(visibleLightIndex, out var b))
            {
                return new Vector4(-light.shadowStrength, 0.0f, 0.0f, maskChannel);
            }

            shadowedOtherLights[shadowedOtherLightCount] = new ShadowedOtherLight()
            {
                visibleLightIndex = visibleLightIndex,
                slopeScaleBias = light.shadowBias,
                normalBias = light.shadowNormalBias,
            };

            return new Vector4(light.shadowStrength, shadowedOtherLightCount++, 0.0f, maskChannel);
        }

        private struct ShadowedDirectionalLight
        {
            public int visibleLightIndex;
            public float slopeScaleBias;
            public float nearPlaneOffset;
        }

        public void Render()
        {
            if (shadowedDirectionalLightCount > 0) RenderDirectionalShadows();
            else buffer.GetTemporaryRT(DirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

            if (shadowedOtherLightCount > 0) RenderOtherShadows();
            else buffer.SetGlobalTexture(OtherShadowAtlasId, DirShadowAtlasId);

            buffer.BeginSample(BufferName);
            SetKeywords(ShadowMaskKeywords,
                useShadowMask ? QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask ? 0 : 1 : -1);

            CascadeCount.Send(shadowedDirectionalLightCount > 0 ? settings.directional.cascadeCount : 0);
            var f = 1.0f - settings.directional.cascadeFade;
            ShadowDistanceFade.Send(new Vector4(1.0f / settings.maxDistance, 1.0f / settings.distanceFade,
                1.0f / (1.0f - f * f)));

            ShadowAtlasSize.Send(atlasSizes);

            buffer.EndSample(BufferName);
            ExecuteBuffer();
        }

        private void RenderDirectionalShadows()
        {
            var atlasSize = (int)settings.directional.atlasSize;
            atlasSizes.x = atlasSize;
            atlasSizes.x = 1.0f / atlasSize;

            buffer.GetTemporaryRT(DirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear,
                RenderTextureFormat.Shadowmap);
            buffer.SetRenderTarget(DirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            buffer.ClearRenderTarget(true, false, Color.clear);

            ShadowPancaking.Send(1.0f);

            buffer.BeginSample(BufferName);
            ExecuteBuffer();

            var tiles = shadowedDirectionalLightCount * settings.directional.cascadeCount;
            var split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
            var tileSize = atlasSize / split;

            for (var i = 0; i < shadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadows(i, split, tileSize);
            }

            ShaderProperty.SendAll(CascadeCullingSpheres, CascadeData);
            DirShadowMatrices.Send();
            SetKeywords(DirectionalFilterKeywords, (int)settings.directional.filter - 1);
            SetKeywords(CascadeBlendKeywords, (int)settings.directional.cascadeBlend - 1);
            buffer.EndSample(BufferName);
            ExecuteBuffer();
        }

        private void RenderOtherShadows()
        {
            var atlasSize = (int)settings.other.atlasSize;
            atlasSizes.x = atlasSize;
            atlasSizes.w = 1.0f / atlasSize;

            buffer.GetTemporaryRT(OtherShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear,
                RenderTextureFormat.Shadowmap);
            buffer.SetRenderTarget(OtherShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            buffer.ClearRenderTarget(true, false, Color.clear);

            ShadowPancaking.Send(0.0f);

            buffer.BeginSample(BufferName);
            ExecuteBuffer();

            var tiles = shadowedOtherLightCount;
            var split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
            var tileSize = atlasSize / split;

            for (var i = 0; i < shadowedOtherLightCount; i++)
            {
                RenderSpotShadows(i, split, tileSize);
            }

            ShaderProperty.SendAll(OtherShadowMatrices, OtherShadowTiles);
            SetKeywords(OtherFilterKeywords, (int)settings.other.filter - 1);
            buffer.EndSample(BufferName);
            ExecuteBuffer();
        }
 
        private void SetKeywords(IReadOnlyList<string> keywords, int enabledIndex)
        {
            for (var i = 0; i < keywords.Count; i++)
            {
                if (i == enabledIndex)
                {
                    buffer.EnableShaderKeyword(keywords[i]);
                }
                else
                {
                    buffer.DisableShaderKeyword(keywords[i]);
                }
            }
        }

        private Vector2 SetTileViewport(int index, int split, float tileSize)
        {
            // ReSharper disable once PossibleLossOfFraction
            var offset = new Vector2(index % split, index / split);
            buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
            return offset;
        }

        private void RenderDirectionalShadows(int index, int split, int tileSize)
        {
            var light = shadowedDirectionalLights[index];
            var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);

            var cascadeCount = settings.directional.cascadeCount;
            var tileOffset = index * cascadeCount;
            var ratios = settings.directional.CascadeRatios;

            var cullingFactor = Mathf.Max(0f, 0.8f - settings.directional.cascadeFade);

            var tileScale = 1.0f / split;
            
            for (var i = 0; i < cascadeCount; i++)
            {
                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, i,
                    cascadeCount,
                    ratios, tileSize, light.nearPlaneOffset, out var viewMatrix, out var projMatrix, out var splitData);

                splitData.shadowCascadeBlendCullingFactor = cullingFactor;
                shadowSettings.splitData = splitData;

                if (index == 0)
                {
                    SetCascadeData(i, splitData.cullingSphere, tileSize);
                }

                var tileIndex = tileOffset + i;
                DirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projMatrix * viewMatrix,
                    SetTileViewport(tileIndex, split, tileSize), tileScale);
                buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);

                buffer.SetGlobalDepthBias(0.0f, light.slopeScaleBias);
                ExecuteBuffer();
                context.DrawShadows(ref shadowSettings);
                buffer.SetGlobalDepthBias(0.0f, 0.0f);
            }
        }

        private void SetCascadeData(int i, Vector4 cullingSphere, float tileSize)
        {
            var texelSize = 2.0f * cullingSphere.w / tileSize;
            var filterSize = texelSize * ((float)settings.directional.filter + 1f);

            cullingSphere.w -= filterSize;
            cullingSphere.w *= cullingSphere.w;
            CascadeCullingSpheres[i] = cullingSphere;
            CascadeData[i] = new Vector4()
            {
                x = 1.0f / cullingSphere.w,
                y = filterSize * 1.4142136f,
            };
        }

        public void Cleanup()
        {
            buffer.ReleaseTemporaryRT(DirShadowAtlasId);

            if (shadowedOtherLightCount > 0) buffer.ReleaseTemporaryRT(OtherShadowAtlasId);

            ExecuteBuffer();
        }

        private static Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, float scale)
        {
            if (SystemInfo.usesReversedZBuffer)
            {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }

            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);

            return m;
        }

        private void RenderSpotShadows(int index, int split, int tileSize)
        {
            var light = shadowedOtherLights[index];
            var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
            cullingResults.ComputeSpotShadowMatricesAndCullingPrimitives(light.visibleLightIndex, out var viewMatrix,
                out var projMatrix, out var splitData);

            shadowSettings.splitData = splitData;

            var texelSize = 2.0f / (tileSize * projMatrix.m00);
            var filterSize = texelSize * ((float)settings.other.filter + 1.0f);
            var bias = light.normalBias * filterSize * 1.4142136f;

            var offset = SetTileViewport(index, split, tileSize);
            var tileScale = 1.0f / split;
            SetOtherTileData(index, offset, tileScale, bias);
            OtherShadowMatrices[index] =
                ConvertToAtlasMatrix(projMatrix * viewMatrix, offset, tileScale);

            buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
            buffer.SetGlobalDepthBias(0.0f, light.slopeScaleBias);
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
            buffer.SetGlobalDepthBias(0.0f, 0.0f);
        }

        private void SetOtherTileData (int index, Vector2 offset, float scale, float bias)
        {
            var border = atlasSizes.w * 0.5f;
            Vector4 data;
            data.x = offset.x * scale + border;
            data.y = offset.y * scale + border;
            data.z = scale - border - border;
            data.w = bias;
            OtherShadowTiles[index] = data;
        }
        
        private struct ShadowedOtherLight
        {
            public int visibleLightIndex;
            public float slopeScaleBias;
            public float normalBias;
        }
    }
}