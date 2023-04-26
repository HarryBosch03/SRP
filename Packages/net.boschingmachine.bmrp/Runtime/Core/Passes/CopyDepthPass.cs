using BMRP.Runtime.PostFX;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime.Core.Passes
{
    public class CopyDepthPass : RenderPass
    {
        private static readonly int DepthTex = Shader.PropertyToID("_DepthTex");

        public override void Setup(CameraRenderer renderer)
        {
            base.Setup(renderer);
            Buffer.GetTemporaryRT(DepthTex, Renderer.Camera.pixelWidth, Renderer.Camera.pixelHeight, 32, FilterMode.Point, RenderTextureFormat.DefaultHDR);
        }

        protected override void OnExecute()
        {
            Buffer.Blit(PostFXStack.DepthAttach, DepthTex);
            Buffer.SetRenderTarget(
                PostFXStack.FXBuffer1, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, 
                PostFXStack.DepthAttach, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Buffer.ReleaseTemporaryRT(DepthTex);
        }
    }
}