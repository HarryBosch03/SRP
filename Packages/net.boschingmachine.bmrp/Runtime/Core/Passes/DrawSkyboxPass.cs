namespace BMRP.Runtime.Core.Passes
{
    public class DrawSkyboxPass : RenderPass
    {
        protected override void OnExecute()
        {
            Renderer.Context.DrawSkybox(Renderer.Camera);
        }
    }
}