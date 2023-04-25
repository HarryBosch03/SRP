using UnityEngine.Rendering;

namespace BMRP.Runtime.Core.Passes
{
    public class DrawTransparentPass : DrawRangePass
    {
        public override SortingCriteria SortingCriteria => SortingCriteria.CommonOpaque;
        public override RenderQueueRange RenderQueueRange => RenderQueueRange.opaque;
    }
}