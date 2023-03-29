using UnityEngine;

namespace BMRP.Runtime.PostFX
{
    public abstract class PostEffectBlit : PostEffect
    {
        private Material mat;

        protected internal const string EffectDir = "BMRP/Post Process/";
        
        public abstract string ShaderName { get; }

        protected virtual void Awake()
        {
            mat = CreateMaterial(ShaderName);
        }

        protected virtual void OnDestroy()
        {
            DestroyImmediate(mat);
        }
        
        public override void Apply(PostFXStack stack)
        {
            stack.Buffer.Blit(stack.SourceBuffer, stack.TargetBuffer, mat);
        }

        protected static Material CreateMaterial(string name)
        {
            var mat = new Material(Shader.Find(name));
            mat.hideFlags = HideFlags.HideAndDontSave;
            return mat;
        }
    }
}