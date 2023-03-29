using UnityEngine;

namespace BMRP.Runtime.PostFX
{
    public abstract class PostEffectSettings : ScriptableObject
    {
        public bool enabled;

        protected class MaterialProperty<T>
        {
            private readonly string reference;

            private readonly Material material;
            private readonly System.Func<Material, T> get;
            private readonly System.Action<Material, T> set;

            public T Value
            {
                get => get(material);
                set => set(material, value);
            }
            
            public MaterialProperty(Material material, string reference, System.Func<Material, T> get, System.Action<Material, T> set)
            {
                this.material = material;
                this.reference = reference;
                this.get = get;
                this.set = set;
            }
        }

        protected class MaterialPropertyFactory
        {
            private readonly Material material ;

            public MaterialPropertyFactory(Material material)
            {
                this.material = material;
            }

            public MaterialProperty<float> Float(string reference) => new MaterialProperty<float>(material, reference, m => m.GetFloat(reference), (m, v) => m.SetFloat(reference, v));
        }
    }
}