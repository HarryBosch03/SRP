using UnityEngine;

namespace BMRP.Runtime.PostFX
{
    [System.Serializable]
    public abstract class PostEffect : ScriptableObject
    {
        [SerializeField] private bool enabled = true;
        
        public bool Enabled { get => enabled; set => enabled = value; }
        
        public abstract string DisplayName { get; }
        public abstract void Apply(PostFXStack stack);
    }
}
