using BMRP.Runtime.Scene;

namespace BMRP.Runtime.Core.Passes
{
    public class FlarePass : RenderPass
    {
        protected override void OnExecute()
        {
            Flare.DrawAll(Renderer);
        }
    }
}