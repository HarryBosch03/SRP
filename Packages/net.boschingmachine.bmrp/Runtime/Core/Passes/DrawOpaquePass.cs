using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime.Core.Passes
{
    public class DrawOpaquePass : DrawRangePass
    {
        public override SortingCriteria SortingCriteria => SortingCriteria.CommonOpaque;
        public override RenderQueueRange RenderQueueRange => RenderQueueRange.opaque;
    }
}