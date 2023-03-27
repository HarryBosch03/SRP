using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    public class Flare : MonoBehaviour
    {
        [SerializeField] private Texture2D texture;
        [SerializeField] private Color color;
        [SerializeField] private float brightness;

        private Light light;
        
        public Material material;

        private static readonly HashSet<Flare> Flares = new();

        private static CommandBuffer buffer;

        private static readonly int 
            TextureProp = Shader.PropertyToID("_MainTex"),
            ColorProp = Shader.PropertyToID("_Color"),
            BrightnessProp = Shader.PropertyToID("_Value");

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

            var cam = renderer.Camera;
            var fwd = cam.cameraToWorldMatrix.GetColumn(2);

            var rot = Quaternion.LookRotation(fwd, Vector3.up);

            var mat = Matrix4x4.TRS(transform.position, rot, Vector3.one);

            buffer.DrawProcedural(mat, material, -1, MeshTopology.Triangles, 6);
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