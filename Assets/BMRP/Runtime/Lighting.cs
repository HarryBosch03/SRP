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

        private static readonly ShaderInt
            DirLightCount = new("_DirectionalLightCount", MaxDirLightCount),
            OtherLightCount = new("_OtherLightCount", MaxOtherLightCount);
        
        private static readonly ShaderVectorArray
            DirLightColors = new("_DirectionalLightColors", MaxDirLightCount),
            DirLightDirections = new("_DirectionalLightDirections", MaxDirLightCount),
            DirLightShadowData = new("_DirectionalLightShadowData", MaxDirLightCount),
            
            OtherLightColors = new("_OtherLightColors", MaxOtherLightCount),
            OtherLightPositions = new("_OtherLightPositions", MaxOtherLightCount),
            OtherLightDirections = new("_OtherLightDirections", MaxOtherLightCount),
            OtherLightSpotAngles = new("_OtherLightSpotAngles", MaxOtherLightCount),
            OtherLightShadowData = new("_OtherLightShadowData", MaxOtherLightCount);

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
                        SetupSpotlight(otherLightCount++, i, ref light);
                        break;
                    case LightType.Directional:
                        SetupDirectionalLight(directionalLightCount++, i, ref light);
                        break;
                    case LightType.Point:
                        SetupPointLight(otherLightCount++, i, ref light);
                        break;
                    case LightType.Area:
                        break;
                    case LightType.Disc:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ShaderProperty.ActiveBuffer = buffer;
            DirLightCount.Send(directionalLightCount);
            if (directionalLightCount > 0)
            {
                ShaderProperty.SendAll(DirLightColors, DirLightDirections, DirLightShadowData);
            }
            
            OtherLightCount.Send(otherLightCount);
            if (otherLightCount > 0)
            {
                ShaderProperty.SendAll(OtherLightColors, OtherLightPositions, OtherLightDirections, OtherLightSpotAngles, OtherLightShadowData);
            }
        }

        private void SetupDirectionalLight(int index, int visibleIndex, ref VisibleLight light)
        {
            DirLightColors[index] = light.finalColor;
            DirLightDirections[index] = -light.localToWorldMatrix.GetColumn(2);
            DirLightShadowData[index] = shadows.ReserveDirectionalShadows(light.light, visibleIndex);
        }

        private void SetupOtherLight (int index, int visibleIndex, ref VisibleLight visibleLight)
        {
            OtherLightColors[index] = visibleLight.finalColor;
            
            var position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1.0f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
            OtherLightPositions[index] = position;

            OtherLightSpotAngles[index] = new Vector4(0.0f, 1.0f);

            var light = visibleLight.light;
            OtherLightShadowData[index] = shadows.ReserveOtherShadows(light, visibleIndex);
        }
        
        private void SetupPointLight(int index, int visibleIndex, ref VisibleLight visibleLight)
        {
            SetupOtherLight(index, visibleIndex, ref visibleLight);
        }

        private void SetupSpotlight(int index, int visibleIndex, ref VisibleLight visibleLight)
        {
            SetupOtherLight(index, visibleIndex, ref visibleLight);
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