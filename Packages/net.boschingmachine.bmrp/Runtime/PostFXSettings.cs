using System.Collections.Generic;
using UnityEngine;

namespace BMRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Post FX Settings")]
    public class PostFXSettings : ScriptableObject
    {
        [SerializeField] private int downscale = 0;
        [SerializeField] private List<Material> effects;

        public int Downscale => downscale;

        public List<Material> GetEffects()
        {
            return effects;
        }
    }
}