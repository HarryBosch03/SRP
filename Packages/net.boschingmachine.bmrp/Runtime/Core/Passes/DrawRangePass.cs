using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace BMRP.Runtime.Core.Passes
{
    public abstract class DrawRangePass : RenderPass
    {
        private static readonly ShaderTagId
            UnlitShaderTagId = new("SRPDefaultUnlit"),
            LitShaderTagId = new("BMLit");

        public abstract SortingCriteria SortingCriteria { get; }
        public abstract RenderQueueRange RenderQueueRange { get; }

        protected override void OnExecute()
        {
            var sortingSettings = new SortingSettings(Renderer.Camera)
            {
                criteria = SortingCriteria,
            };
            var drawingSettings = new DrawingSettings(UnlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = Renderer.Settings.useDynamicBatching,
                enableInstancing = Renderer.Settings.useGPUInstancing,
                perObjectData = PerObjectData.LightProbe
            };
            drawingSettings.SetShaderPassName(1, LitShaderTagId);
            var filterSettings = new FilteringSettings(RenderQueueRange);
            
            Renderer.Context.DrawRenderers(Renderer.CullingResults, ref drawingSettings, ref filterSettings);
        }
    }
}