using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public class BmrPipeline : RenderPipeline
    {
        private readonly CameraRenderer renderer = new();

        private CameraRenderer.CameraSettings settings;
        
        public BmrPipeline(CameraRenderer.CameraSettings settings)
        {
            this.settings = settings;
            GraphicsSettings.useScriptableRenderPipelineBatching = settings.useSrpBatching;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                renderer.Render(context, camera, settings);
            }
        }
    }
}
