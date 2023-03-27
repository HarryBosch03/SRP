using System;
using UnityEngine;

namespace BMRP.Runtime
{
    public class AcesEffect : PostEffect
    {
        private Material mat;

        private void Awake()
        {
            mat = new Material(Shader.Find("BMRP/Post Process/ACES"));
        }

        private void OnDestroy()
        {
            DestroyImmediate(mat);
        }

        public override string DisplayName => "Tonemapping";

        public override void Apply(PostFXStack stack)
        {
            stack.Buffer.Blit(stack.SourceBuffer, stack.TargetBuffer, mat);
        }
    }
}
