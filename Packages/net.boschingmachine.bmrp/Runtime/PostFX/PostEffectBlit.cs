using UnityEngine;

namespace BMRP.Runtime.PostFX
{
    public abstract class PostEffectBlit : PostEffect
    {
        private Material mat;
        
        public abstract string ShaderName { get; }

        public override void Setup(PostFXStack stack)
        {
            GetCachedMaterial(ShaderName, ref mat);
        }

        public override void Apply(PostFXStack stack)
        {
            stack.Buffer.Blit(stack.SourceBuffer, stack.DestBuffer, mat);
        }
    }
}