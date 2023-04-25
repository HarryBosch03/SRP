using System;
using UnityEngine;

namespace BMRP.Runtime.PostFX
{
    [System.Serializable]
    public abstract class PostEffect : ScriptableObject
    {
        [SerializeField] private bool enabled = true;
        
        protected internal const string InternalDirectory = "BMRP/Post Process/";
        public virtual bool Enabled { get => enabled; set => enabled = value; }

        public abstract string DisplayName { get; }

        public virtual void Setup(PostFXStack stack) { }
        public abstract void Apply(PostFXStack stack);
        public virtual void Cleanup(PostFXStack stack) { }
        
        protected static void GetCachedMaterial(string name, ref Material mat)
        {
            if (mat) return;
        
            mat = new Material(Shader.Find(name));
        }

        public abstract class MaterialPropSync
        {
            [HideInInspector] public Material mat;
            [HideInInspector] public string propertyName;

            public MaterialPropSync(string propertyName)
            {
                this.propertyName = propertyName;
            }

            public abstract void Sync();

            public static void SyncAll(Material mat, params MaterialPropSync[] props)
            {
                foreach (var prop in props)
                {
                    prop.mat = mat;
                    prop.Sync();
                }
            }
        }

        [System.Serializable]
        public class MaterialPropSyncFloat : MaterialPropSync
        {
            public float value;
            public MaterialPropSyncFloat(string propertyName) : base(propertyName) { }
            
            public override void Sync()
            {
                mat.SetFloat(propertyName, value);
            }
        }

        protected internal static void CreateMaterialInternal(string name, ref Material mat) => GetCachedMaterial(InternalDirectory + name, ref mat);
        
        protected void Fallback(PostFXStack stack)
        {
            stack.Buffer.Blit(stack.SourceBuffer, stack.DestBuffer);
        }
    }
}
