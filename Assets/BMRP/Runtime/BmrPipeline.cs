using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public class BmrPipeline : RenderPipeline
    {
        private readonly CameraRenderer renderer = new CameraRenderer();

        bool useDynamicBatching, useGPUInstancing;
        
        public BmrPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
        {
            this.useDynamicBatching = useDynamicBatching;
            this.useGPUInstancing = useGPUInstancing;
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                renderer.Render(context, camera, useDynamicBatching, useGPUInstancing);
            }
        }
    }
}
