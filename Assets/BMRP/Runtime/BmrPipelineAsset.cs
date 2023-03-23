using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Bmr Pipeline Asset")]
    public class BmrPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] private CameraRenderer.CameraSettings settings;
        
        protected override RenderPipeline CreatePipeline()
        {
            return new BmrPipeline(settings);
        }
    }
}
