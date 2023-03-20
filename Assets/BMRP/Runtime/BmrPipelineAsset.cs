using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace BMRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/BMR Pipeline Asset")]
    public class BmrPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] private bool useDynamicBatching = true, useGPUInstancing = true;
        [FormerlySerializedAs("useSRPBatcher")] [SerializeField] private bool useSrpBatcher = true;
        [SerializeField] private ShadowSettings shadows = default;
        
        protected override RenderPipeline CreatePipeline()
        {
            return new BmrPipeline(useDynamicBatching, useGPUInstancing, useSrpBatcher, shadows);
        }
    }
}
