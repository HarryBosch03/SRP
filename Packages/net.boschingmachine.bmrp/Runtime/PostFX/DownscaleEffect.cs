using UnityEngine;

namespace BMRP.Runtime.PostFX
{
    public class DownscaleEffect : PostEffect
    {
        public int factor;

        private static readonly int buffer = Shader.PropertyToID("_Buffer");

        public override string DisplayName => "Downscale";

        public override bool Enabled => base.Enabled && factor > 0;

        public override void Setup(PostFXStack stack)
        {
            stack.Buffer.GetTemporaryRT(buffer, stack.Width / factor, stack.Height / factor, 32, FilterMode.Point, RenderTextureFormat.DefaultHDR);
        }

        public override void Apply(PostFXStack stack)
        {
            stack.Buffer.Blit(stack.SourceBuffer, buffer);
            stack.Buffer.Blit(buffer, stack.DestBuffer);
        }

        public override void Cleanup(PostFXStack stack)
        {
            stack.Buffer.ReleaseTemporaryRT(buffer);
        }
    }
}