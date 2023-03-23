using System.Collections.Generic;
using UnityEngine;

namespace BMRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Post FX Settings")]
    public class PostFXSettings : ScriptableObject
    {
        [SerializeField] private List<Material> effects = new();

        public List<Material> Effects => effects;
    }
}