using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public class Shadows
    {
        private const string BufferName = "Shadows";
        private const int MaxShadowedDirectionalLightCount = 4;
        private const int MaxCascades = 4;
        private int shadowedDirectionalLightCount;

        private readonly CommandBuffer buffer = new CommandBuffer
        {
            name = BufferName
        };

        private ScriptableRenderContext context;
        private CullingResults cullingResults;
        private ShadowSettings settings;

        private readonly ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];

        private static readonly int
            DirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
            DirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
            CascadeCountId = Shader.PropertyToID("_CascadeCount"),
            CascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
            CascadeDataId = Shader.PropertyToID("_CascadeData"),
            ShadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize"),
            ShadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");

        private static readonly Matrix4x4[]
            DirShadowMatrices = new Matrix4x4[MaxShadowedDirectionalLightCount * MaxCascades];

        private static readonly Vector4[] 
            CascadeCullingSpheres = new Vector4[MaxCascades],
            CascadeData = new Vector4[MaxCascades];

        private static readonly string[] DirectionalFilterKeywords = 
        {
            "_DIRECTIONAL_PCF3",
            "_DIRECTIONAL_PCF5",
            "_DIRECTIONAL_PCF7",
        };

        private static readonly string[] CascadeBlendKeywords = 
        {
            "_CASCADE_BLEND_SOFT",
            "_CASCADE_BLEND_DITHER",
        };

        private static readonly string[] ShadowMaskKeywords = 
        {
            "_SHADOW_MASK_ALWAYS",
            "_SHADOW_MASK_DISTANCE",
        };

        private bool useShadowMask;
        
        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
        {
            this.context = context;
            this.cullingResults = cullingResults;
            this.settings = settings;
            shadowedDirectionalLightCount = 0;
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
            if (lightBaking is { lightmapBakeType: LightmapBakeType.Mixed, mixedLightingMode: MixedLightingMode.Shadowmask }) 
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
                    VisibleLightIndex = visibleLightIndex,
                    SlopeScaleBias = light.shadowBias,
                    NearPlaneOffset =  light.shadowNearPlane,
                };

            return new Vector4()
            {
                x = light.shadowStrength,
                y = settings.directional.cascadeCount * shadowedDirectionalLightCount++,
                z = light.shadowNormalBias,
                w = maskChannel
            };
        }

        private struct ShadowedDirectionalLight
        {
            public int VisibleLightIndex;
            public float SlopeScaleBias;
            public float NearPlaneOffset;
        }

        public void Render()
        {
            if (shadowedDirectionalLightCount > 0) RenderDirectionalShadows();
            else buffer.GetTemporaryRT(DirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            
            buffer.BeginSample(BufferName);
            SetKeywords(ShadowMaskKeywords, useShadowMask ? QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask ? 0 : 1 : -1);
            buffer.EndSample(BufferName);
            ExecuteBuffer();
        }

        private void RenderDirectionalShadows()
        {
            var atlasSize = (int)settings.directional.atlasSize;
            buffer.GetTemporaryRT(DirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            buffer.SetRenderTarget(DirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            buffer.ClearRenderTarget(true, false, Color.clear);
            buffer.BeginSample(BufferName);
            ExecuteBuffer();

            var tiles = shadowedDirectionalLightCount * settings.directional.cascadeCount;
            var split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
            var tileSize = atlasSize / split;

            for (var i = 0; i < shadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadows(i, split, tileSize);
            }
            
            buffer.SetGlobalInt(CascadeCountId, settings.directional.cascadeCount);
            buffer.SetGlobalVectorArray(CascadeCullingSpheresId, CascadeCullingSpheres);
            buffer.SetGlobalVectorArray(CascadeDataId, CascadeData);

            buffer.SetGlobalMatrixArray(DirShadowMatricesId, DirShadowMatrices);
            var f = 1.0f - settings.directional.cascadeFade;
            buffer.SetGlobalVector(ShadowDistanceFadeId, new Vector4(1.0f / settings.maxDistance, 1.0f / settings.distanceFade, 1.0f / (1.0f - f * f)));

            SetKeywords(DirectionalFilterKeywords, (int)settings.directional.filter - 1);
            SetKeywords(CascadeBlendKeywords, (int)settings.directional.cascadeBlend - 1);
            buffer.SetGlobalVector(ShadowAtlasSizeId, new Vector4(atlasSize, 1.0f / atlasSize));
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
            var offset = new Vector2(index % split, index / split);
            buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
            return offset;
        }

        private void RenderDirectionalShadows(int index, int split, int tileSize)
        {
            var light = shadowedDirectionalLights[index];
            var shadowSettings = new ShadowDrawingSettings(cullingResults, light.VisibleLightIndex);

            var cascadeCount = settings.directional.cascadeCount;
            var tileOffset = index * cascadeCount;
            var ratios = settings.directional.CascadeRatios;

            var cullingFactor = Mathf.Max(0f, 0.8f - settings.directional.cascadeFade);

            for (var i = 0; i < cascadeCount; i++)
            {
                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.VisibleLightIndex, i, cascadeCount,
                    ratios, tileSize, light.NearPlaneOffset, out var viewMatrix, out var projMatrix, out var splitData);
                
                splitData.shadowCascadeBlendCullingFactor = cullingFactor;
                shadowSettings.splitData = splitData;

                if (index == 0)
                {
                    SetCascadeData(i, splitData.cullingSphere, tileSize);
                }
                
                var tileIndex = tileOffset + i;
                DirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projMatrix * viewMatrix, SetTileViewport(tileIndex, split, tileSize), split);
                buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
                
                buffer.SetGlobalDepthBias(0.0f, light.SlopeScaleBias);
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
            ExecuteBuffer();
        }

        private static Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
        {
            if (SystemInfo.usesReversedZBuffer)
            {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }

            var scale = 1f / split;
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
    }
}