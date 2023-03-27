using System.Collections.Generic;
using UnityEngine;

namespace BMRP.Runtime
{
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu("Rendering/BMRP/Color Grading Volume")]
    public sealed class ColorGradingVolume : MonoBehaviour
    {
        [SerializeField] private float exposureValue = 0.0f;
        [SerializeField][Range(0.0f, 2.0f)] private float contrastValue = 1.0f;
        [SerializeField] private float temperatureValue = 0.0f;
        [SerializeField] private float tintValue = 0.0f;

        [SerializeField] private bool global;
        [SerializeField]  private float priority;
        [SerializeField] [Range(0.0f, 1.0f)] private float weight;
        [SerializeField] private float blendDistance;

        public static readonly ShaderProperty<float>
            Exposure = ShaderPropertyFactory.Float("_Exposure"),
            Contrast = ShaderPropertyFactory.Float("_Contrast"),
            Temperature = ShaderPropertyFactory.Float("_Temperature"),
            Tint = ShaderPropertyFactory.Float("_Tint");

        private new static CameraRenderer renderer;

        private static readonly HashSet<ColorGradingVolume> Volumes = new();
        
        private void OnEnable()
        {
            Volumes.Add(this);
        }

        private void OnDisable()
        {
            Volumes.Remove(this);
        }

        private static void SetWeighted(ShaderProperty<float> p, System.Func<ColorGradingVolume, float> get, float v = default) => SetWeighted(p, get, Mathf.Lerp, v);
        private static void SetWeighted(ShaderProperty<Vector4> p, System.Func<ColorGradingVolume, Vector4> get, Vector4 v) => SetWeighted(p, get, Vector4.Lerp, v);

        private static void SetWeighted<T>(ShaderProperty<T> p, System.Func<ColorGradingVolume, T> get, System.Func<T, T, float, T> lerp, T v = default)
        {
            var sorted = new List<ColorGradingVolume>(Volumes);
            sorted.SortBy(e => e.priority);
            
            foreach (var volume in sorted)
            {
                v = lerp(v, get(volume), volume.GetBlendWeight());
            }
            
            p.Set(v);
        }

        private float GetBlendWeight()
        {
            return weight * (global ? 1.0f : Mathf.InverseLerp(blendDistance, 0.0f, GetClosestDistanceToCamera()));
        }
        
        private float GetClosestDistanceToCamera()
        {
            if (global) return 0.0f;

            var dist = float.MaxValue;
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                var cameraPos = (Vector3)renderer.Camera.cameraToWorldMatrix.GetColumn(3);
                var point = collider.ClosestPoint(cameraPos);
                var dist2 = (point - cameraPos).magnitude;
                if (dist2 < dist) dist = dist2;
            }

            return dist;
        }

        public static void SetWeights(CameraRenderer renderer)
        {
            ColorGradingVolume.renderer = renderer;
            
            using var ab = new ShaderProperty.BufferContext(renderer.Buffer);
            SetWeighted(Exposure, v => v.exposureValue);
            SetWeighted(Contrast, v => v.contrastValue, 1.0f);
            SetWeighted(Temperature, v => v.temperatureValue);
            SetWeighted(Tint, v => v.tintValue);
        }
    }
}