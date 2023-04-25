using System;
using BMRP.Runtime.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime.Lighting
{
    public class Lighting
    {
        private const string BufferName = "Lighting";
        private const int MaxDirectionalLights = 4, MaxOtherLights = 64;

        private readonly ShaderProperty<int>
            dirLightCount = ShaderPropertyFactory.Int("_DirectionalLightCount"),
            otherLightCount = ShaderPropertyFactory.Int("_OtherLightCount");

        private readonly ShaderProperty<Vector4[]>
            dirLightColors = ShaderPropertyFactory.VecArray("_DirectionalLightColors", MaxDirectionalLights),
            dirLightDirections = ShaderPropertyFactory.VecArray("_DirectionalLightDirections", MaxDirectionalLights),

            otherLightColors = ShaderPropertyFactory.VecArray("_OtherLightColors", MaxOtherLights),
            otherLightPositions = ShaderPropertyFactory.VecArray("_OtherLightPositions", MaxOtherLights),
            otherLightDirections = ShaderPropertyFactory.VecArray("_OtherLightDirections", MaxOtherLights),
            otherLightSpotAngles = ShaderPropertyFactory.VecArray("_OtherLightSpotAngles", MaxOtherLights);

        private readonly CommandBuffer buffer = new()
        {
            name = BufferName,
        };

        private CullingResults cullingResults;

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
        {
            this.cullingResults = cullingResults;

            dirLightCount.Value = 0;
            otherLightCount.Value = 0;
            
            buffer.BeginSample(BufferName);
            
            SetupLights();
            
            buffer.EndSample(BufferName);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        private void SetupLights()
        {
            foreach (var light in cullingResults.visibleLights)
            {
                switch (light.lightType)
                {
                    case LightType.Spot:
                        SetupSpotlight(light);
                        break;
                    case LightType.Directional:
                        SetupDirectionalLight(light);
                        break;
                    case LightType.Point:
                        SetupPointLight(light);
                        break;
                    case LightType.Area:
                        break;
                    case LightType.Disc:
                        throw new Exception("What the Fuck is a Disc Light");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ShaderProperty.SendAll(buffer,
                dirLightCount, 
                dirLightColors, 
                dirLightDirections, 
                otherLightCount,
                otherLightColors, 
                otherLightPositions,
                otherLightDirections,
                otherLightSpotAngles);
        }

        private void SetupDirectionalLight(VisibleLight light)
        {
            if (dirLightCount.Value >= MaxDirectionalLights) return;
            
            dirLightColors.Value[dirLightCount.Value] = light.finalColor;
            dirLightDirections.Value[dirLightCount.Value] = light.localToWorldMatrix.GetColumn(2);
            
            dirLightCount.Value++;
        }

        private bool SetupOtherLight(VisibleLight light) => SetupOtherLight(light, out _);
        private bool SetupOtherLight(VisibleLight light, out int index)
        {
            index = -1;
            if (otherLightCount.Value >= MaxOtherLights) return false;
            
            index = GetOtherLightIndex();
            
            otherLightColors.Value[index] = light.finalColor;
            otherLightPositions.Value[index] = light.localToWorldMatrix.GetColumn(3);
            otherLightDirections.Value[index] = Vector4.zero;
            otherLightSpotAngles.Value[index] = new Vector4(0.0f, 1.0f);

            return true;
        }

        private void SetupPointLight(VisibleLight light)
        {
            SetupOtherLight(light);
        }

        private void SetupSpotlight(VisibleLight visibleLight)
        {
            SetupOtherLight(visibleLight, out var i);
            
            Vector3 d = -visibleLight.localToWorldMatrix.GetColumn(2);
            otherLightDirections.Value[i] = new Vector4(d.x, d.y, d.z, 1.0f);

            var light = visibleLight.light;
            var innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
            var outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.spotAngle);
            var angleRangeInv = 1.0f / Mathf.Max(innerCos - outerCos, 0.001f);

            otherLightSpotAngles.Value[i] = new Vector4()
            {
                x = outerCos,
                y = -outerCos * angleRangeInv,
            };
        }

        private int GetOtherLightIndex() => otherLightCount.Value++;
    }
}