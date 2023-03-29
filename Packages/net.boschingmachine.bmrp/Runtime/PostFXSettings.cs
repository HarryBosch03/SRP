using System.Collections.Generic;
using BMRP.Runtime.PostFX;
using UnityEngine;

namespace BMRP.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/Post FX Settings")]
    public class PostFXSettings : ScriptableObject
    {
        [SerializeField] private int targetVerticalResolution = 360;
        [SerializeField][HideInInspector] private List<PostEffect> effects;

        public int TargetVerticalResolution => targetVerticalResolution;

        public List<PostEffect> Effects => effects;
    }
}