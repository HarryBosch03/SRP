using System.Collections.Generic;
using UnityEngine;

namespace BMRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Post FX Settings")]
    public class PostFXSettings : ScriptableObject
    {
        [SerializeField] private int targetVerticalResolution = 360;
        [SerializeField] private List<Material> effects;

        public int TargetVerticalResolution => targetVerticalResolution;

        public List<Material> Effects => effects;
    }
}