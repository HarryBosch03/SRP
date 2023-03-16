using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public class Shadows
    {
        private const string BufferName = "Shadows";
        private const int MaxShadowedDirectionalLightCount = 1;
        private int shadowedDirectionalLightCount;

        private static readonly int DirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

        private readonly CommandBuffer buffer = new CommandBuffer 
        {
            name = BufferName
        };

        private ScriptableRenderContext context;
        private CullingResults cullingResults;
        private ShadowSettings settings;


        private readonly ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];

        public void Setup (ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
        {
            this.context = context;
            this.cullingResults = cullingResults;
            this.settings = settings;
            shadowedDirectionalLightCount = 0;
        }

        private void ExecuteBuffer () 
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
        {
            if (shadowedDirectionalLightCount >= MaxShadowedDirectionalLightCount) return;
            if (light.shadows == LightShadows.None) return;
            if (light.shadowStrength <= 0.0f) return;
            if (!cullingResults.GetShadowCasterBounds(visibleLightIndex, out var bounds)) return;
            
            shadowedDirectionalLights[shadowedDirectionalLightCount++] =
                new ShadowedDirectionalLight {
                    VisibleLightIndex = visibleLightIndex
                };
        }

        private struct ShadowedDirectionalLight 
        {
            public int VisibleLightIndex;
        }

        public void Render()
        {
            if (shadowedDirectionalLightCount > 0) RenderDirectionalShadows();
            else buffer.GetTemporaryRT(DirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear ,RenderTextureFormat.Shadowmap);
        }

        private void RenderDirectionalShadows()
        {
            var atlasSize = (int)settings.directional.atlasSize;
            buffer.GetTemporaryRT(DirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            buffer.SetRenderTarget(DirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            buffer.ClearRenderTarget(true, false, Color.clear);
            buffer.BeginSample(BufferName);
            ExecuteBuffer();

            for (var i = 0; i < MaxShadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadows(i, atlasSize);
            }
            
            buffer.EndSample(BufferName);
            ExecuteBuffer();
        }

        private void RenderDirectionalShadows(int index, int tileSize)
        {
            var light = shadowedDirectionalLights[index];
            var shadowSettings = new ShadowDrawingSettings(cullingResults, light.VisibleLightIndex);
            
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.VisibleLightIndex, 0, 1,
                Vector3.zero, tileSize, 0.0f, out var viewMatrix, out var projMatrix, out var splitData);
            
            shadowSettings.splitData = splitData;
            buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
        }

        public void Cleanup()
        {
            //buffer.ReleaseTemporaryRT(dirShadowAtlasId);
            ExecuteBuffer();
        }
    }
}
