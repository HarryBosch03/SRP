using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
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
            otherLightPositions = ShaderPropertyFactory.VecArray("_OtherLightPositions", MaxOtherLights);

        private readonly CommandBuffer buffer = new CommandBuffer()
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

            ShaderProperty.SendAll(buffer, dirLightCount, dirLightColors, dirLightDirections, otherLightCount, otherLightColors, otherLightPositions);
        }

        private void SetupDirectionalLight(VisibleLight light)
        {
            if (dirLightCount.Value >= MaxDirectionalLights) return;
            
            dirLightColors.Value[dirLightCount.Value] = light.finalColor;
            dirLightDirections.Value[dirLightCount.Value] = light.localToWorldMatrix.GetColumn(2);
            
            dirLightCount.Value++;
        }

        private void SetupOtherLight(VisibleLight light)
        {
            if (otherLightCount.Value >= MaxOtherLights) return;
            
            otherLightColors.Value[otherLightCount.Value] = light.finalColor;
            otherLightPositions.Value[otherLightCount.Value] = light.localToWorldMatrix.GetColumn(3);

            otherLightCount.Value++;
        }

        private void SetupPointLight(VisibleLight light)
        {
            SetupOtherLight(light);
        }
    }
}