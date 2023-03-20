using System;
using UnityEngine;

namespace BMRP.Runtime
{
    [Serializable]
    public class ShadowSettings {

        [Min(0.001f)] public float maxDistance = 100f;
        [Range(0.001f, 1.0f)] public float distanceFade = 0.1f;
        
        public Directional directional = new()
        {
            atlasSize = TextureSize._1024,
            filter = FilterMode.Pcf2X2,
            cascadeBlend = Directional.CascadeBlendMode.Dither,
            cascadeCount = 4,
            cascade1 = 0.1f,
            cascade2 = 0.25f,
            cascade3 = 0.5f,
            cascadeFade = 0.1f
        };

        public Other other = new()
        {
            atlasSize = TextureSize._1024,
            filter = FilterMode.Pcf2X2,
        };
        
        [Serializable]
        public struct Directional 
        {
            public TextureSize atlasSize;
            public FilterMode filter;
            public CascadeBlendMode cascadeBlend;

            [Range(1, 4)] 
            public int cascadeCount;
            
            [Range(0.0f, 1.0f)] 
            public float cascade1, cascade2, cascade3;
            
            [Range(0.001f, 1f)] 
            public float cascadeFade;
            
            public Vector3 CascadeRatios => new(cascade1, cascade2, cascade3);
            
            public enum CascadeBlendMode 
            {
                Hard, Soft, Dither
            }
        }

        [Serializable]
        public struct Other
        {
            public TextureSize atlasSize;
            public FilterMode filter;
        }
        
        public enum TextureSize 
        {
            _256 = 256, _512 = 512, _1024 = 1024,
            _2048 = 2048, _4096 = 4096, _8192 = 8192
        }
        
        public enum FilterMode 
        {
            Pcf2X2, Pcf3X3, Pcf5X5, Pcf7X7
        }
    }
}
