using System.Collections.Generic;
using BMRP.Runtime.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime.Scene
{
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    public class Flare : MonoBehaviour
    {
        [SerializeField] private Texture2D texture;
        [SerializeField] private Color color;
        [SerializeField] private float brightness, size, length;
        [SerializeField][Range(-1.0f, 1.0f)] private float asymmetry;
        [SerializeField] private int texSize;
        [SerializeField] private int childCount;
        [SerializeField] private float childSize, childSlope;

        private new Light light;
        
        [HideInInspector] public Material material;

        private static readonly HashSet<Flare> Flares = new();

        private static CommandBuffer buffer;

        private static readonly int
            TextureProp = Shader.PropertyToID("_MainTex"),
            ColorProp = Shader.PropertyToID("_Color"),
            BrightnessProp = Shader.PropertyToID("_Value"),
            SizeProp = Shader.PropertyToID("_Size"),
            AsymmetryProp = Shader.PropertyToID("_Asymmetry"),
            TexSizeProp = Shader.PropertyToID("_PixelSize"),
            LengthProp = Shader.PropertyToID("_Length"),
            ChildCountProp = Shader.PropertyToID("_ChildCount"),
            ChildSizeProp = Shader.PropertyToID("_ChildSize"),
            ChildSlopeProp = Shader.PropertyToID("_ChildSlope");

        public Light Light
        {
            get
            {
                if (!light) light = GetComponent<Light>();
                return light;
            }
        }
        
        private void OnEnable()
        {
            material = new Material(Shader.Find("Hidden/LensFlare"));
            material.hideFlags = HideFlags.HideAndDontSave;
            
            Flares.Add(this);
        }

        private void OnDisable()
        {
            Flares.Remove(this);
        }

        private void Draw(CameraRenderer renderer)
        {
            material.SetTexture(TextureProp, texture);
            material.SetColor(ColorProp, color * Light.color.linear);
            material.SetFloat(BrightnessProp, brightness * Light.intensity);
            material.SetFloat(SizeProp, size);
            material.SetFloat(AsymmetryProp, asymmetry);
            material.SetFloat(TexSizeProp, texSize);
            material.SetFloat(LengthProp, length);
            material.SetFloat(ChildCountProp, childCount);
            material.SetFloat(ChildSizeProp, childSize);
            material.SetFloat(ChildSlopeProp, childSlope);

            var cam = renderer.Camera;
            var fwd = cam.cameraToWorldMatrix.GetColumn(2);

            var rot = Quaternion.LookRotation(fwd, Vector3.up);

            var mat = Matrix4x4.TRS(transform.position, rot, Vector3.one);
            buffer.DrawProcedural(mat, material, -1, MeshTopology.Triangles, 6 + Mathf.Max(childCount, 1) * 6);
        }

        public static void DrawAll(CameraRenderer renderer)
        {
            buffer ??= new CommandBuffer()
            {
                name = "Flares",
            };
            buffer.BeginSample("Flares");

            foreach (var flare in Flares)
            {
                flare.Draw(renderer);
            }

            buffer.EndSample("Flares");
            renderer.Context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
    }
}