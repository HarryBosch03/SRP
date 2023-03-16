using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public class BmrPipeline : RenderPipeline
    {
        private readonly CameraRenderer renderer = new CameraRenderer();

        bool useDynamicBatching, useGPUInstancing;
        private ShadowSettings shadows;
        
        public BmrPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadows)
        {
            this.useDynamicBatching = useDynamicBatching;
            this.useGPUInstancing = useGPUInstancing;
            this.shadows = shadows;
            
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                renderer.Render(context, camera, useDynamicBatching, useGPUInstancing, shadows);
            }
        }
    }
}
