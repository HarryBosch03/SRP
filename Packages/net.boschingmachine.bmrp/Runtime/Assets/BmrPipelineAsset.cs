using BMRP.Runtime.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime.Assets
{
    [CreateAssetMenu(menuName = "Rendering/Bmr Pipeline Asset")]
    public class BmrPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] private CameraRenderer.CameraSettings settings;

        public CameraRenderer.CameraSettings Settings => settings;
        
        protected override RenderPipeline CreatePipeline()
        {
            return new BmrPipeline(settings);
        }
    }
}