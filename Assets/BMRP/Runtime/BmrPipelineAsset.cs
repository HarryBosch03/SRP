using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/BMR Pipeline Asset")]
    public class BmrPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] private bool useDynamicBatching = true, useGPUInstancing = true, useSRPBatcher = true;
        [SerializeField] private ShadowSettings shadows = default;
        
        protected override RenderPipeline CreatePipeline()
        {
            return new BmrPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher, shadows);
        }
    }
}
