using UnityEditorInternal;
using UnityEngine;

namespace BMRP.Runtime.PostFX
{
    [System.Serializable]
    public class BloomEffect : PostEffect
    {
        public int levels = 5;

        public MaterialPropSyncFloat
            threshold = new("_Threshold"),
            strength = new("_Strength");

        private Material bloomMat;
        private Material addMat;

        private static readonly int Buffer1 = Shader.PropertyToID("_BloomBuffer1");
        private static readonly int Buffer2 = Shader.PropertyToID("_BloomBuffer2");

        public override string DisplayName => "Bloom";
        public override bool Enabled => base.Enabled && levels > 0;

        public override void Setup(PostFXStack stack)
        {
            CreateMaterialInternal("Bloom", ref bloomMat);
            CreateMaterialInternal("BloomAdd", ref addMat);

            MaterialPropSync.SyncAll(bloomMat, threshold, strength);

            stack.Buffer.GetTemporaryRT(Buffer1, stack.Width / 4, stack.Height / 4, 32, FilterMode.Point,
                RenderTextureFormat.DefaultHDR);
            stack.Buffer.GetTemporaryRT(Buffer2, stack.Width / 4, stack.Height / 4, 32, FilterMode.Point,
                RenderTextureFormat.DefaultHDR);
        }

        public override void Apply(PostFXStack stack)
        {
            stack.Buffer.Blit(stack.SourceBuffer, Buffer1);
            stack.Buffer.SetGlobalVector("_Axis", new Vector4(1.0f, 0.0f));
            stack.Buffer.Blit(Buffer1, Buffer2, bloomMat);
            stack.Buffer.SetGlobalVector("_Axis", new Vector4(0.0f, 1.0f));
            stack.Buffer.Blit(Buffer2, Buffer1, bloomMat);
            stack.Buffer.Blit(stack.SourceBuffer, stack.DestBuffer, addMat);
        }

        public override void Cleanup(PostFXStack stack)
        {
            stack.Buffer.ReleaseTemporaryRT(Buffer1);
            stack.Buffer.ReleaseTemporaryRT(Buffer2);
        }
    }
}