using System;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public class Lighting
    {
        private const string BufferName = "Lighting";

        private readonly CommandBuffer buffer = new()
        {
            name = BufferName
        };

        private const int MaxDirLightCount = 4, MaxOtherLightCount = 64;

        private static readonly int
            DirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
            DirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
            DirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections"),
            DirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData"),
            OtherLightCountId = Shader.PropertyToID("_OtherLightCount"),
            OtherLightColorsId = Shader.PropertyToID("_OtherLightColors"),
            OtherLightPositionId = Shader.PropertyToID("_OtherLightPositions"),
            OtherLightDirectionsId = Shader.PropertyToID("_OtherLightDirections"),
            OtherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles");

        private static readonly Vector4[]
            DirLightColors = new Vector4[MaxDirLightCount],
            DirLightDirections = new Vector4[MaxDirLightCount],
            DirLightShadowData = new Vector4[MaxDirLightCount],
            OtherLightColors = new Vector4[MaxOtherLightCount],
            OtherLightPositions = new Vector4[MaxOtherLightCount],
            OtherLightDirections = new Vector4[MaxOtherLightCount],
            OtherLightSpotAngles = new Vector4[MaxOtherLightCount];

        private CullingResults cullingResults;

        private readonly Shadows shadows = new();
        
        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            this.cullingResults = cullingResults;
            buffer.BeginSample(BufferName);
            shadows.Setup(context, cullingResults, shadowSettings);
            SetupLights();
            shadows.Render();
            buffer.EndSample(BufferName);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        private void SetupLights()
        {
            var visibleLights = cullingResults.visibleLights;

            int directionalLightCount = 0, otherLightCount = 0;

            for (var i = 0; i < visibleLights.Length; i++)
            {
                var light = visibleLights[i];
                switch (light.lightType)
                {
                    case LightType.Spot:
                        SetupSpotlight(otherLightCount++, ref light);
                        break;
                    case LightType.Directional:
                        SetupDirectionalLight(directionalLightCount++, ref light);
                        break;
                    case LightType.Point:
                        SetupPointLight(otherLightCount++, ref light);
                        break;
                    case LightType.Area:
                        break;
                    case LightType.Disc:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            buffer.SetGlobalInt(DirLightCountId, directionalLightCount);
            if (directionalLightCount > 0)
            {
                buffer.SetGlobalVectorArray(DirLightColorsId, DirLightColors);
                buffer.SetGlobalVectorArray(DirLightDirectionsId, DirLightDirections);
                buffer.SetGlobalVectorArray(DirLightShadowDataId, DirLightShadowData);
            }
            
            buffer.SetGlobalInt(OtherLightCountId, otherLightCount);
            if (otherLightCount > 0)
            {
                buffer.SetGlobalVectorArray(OtherLightColorsId, OtherLightColors);
                buffer.SetGlobalVectorArray(OtherLightPositionId, OtherLightPositions);
                buffer.SetGlobalVectorArray(OtherLightDirectionsId, OtherLightDirections);
                buffer.SetGlobalVectorArray(OtherLightSpotAnglesId, OtherLightSpotAngles);
            }
        }

        private void SetupDirectionalLight(int index, ref VisibleLight light)
        {
            DirLightColors[index] = light.finalColor;
            DirLightDirections[index] = -light.localToWorldMatrix.GetColumn(2);
            DirLightShadowData[index] = shadows.ReserveDirectionalShadows(light.light, index);
        }

        private void SetupOtherLight (int index, ref VisibleLight visibleLight)
        {
            OtherLightColors[index] = visibleLight.finalColor;
            
            var position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1.0f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
            OtherLightPositions[index] = position;

            OtherLightSpotAngles[index] = new Vector4(0.0f, 1.0f);
        }
        
        private void SetupPointLight(int index, ref VisibleLight visibleLight)
        {
            SetupOtherLight(index, ref visibleLight);
        }

        private void SetupSpotlight(int index, ref VisibleLight visibleLight)
        {
            SetupOtherLight(index, ref visibleLight);
            OtherLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);

            var light = visibleLight.light;
            var innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
            var outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.spotAngle);
            var angleRangeInv = 1.0f / Mathf.Max(innerCos - outerCos, 0.001f);
            OtherLightSpotAngles[index] = new Vector4
            {
                x = angleRangeInv,
                y = -outerCos * angleRangeInv,
            };
        }
        
        public void Cleanup()
        {
            shadows.Cleanup();
        }

        private delegate void LightSetup(int index, ref VisibleLight light);
    }
}