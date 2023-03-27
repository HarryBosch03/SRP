using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMRP.Runtime
{
    public class LensFlare : MonoBehaviour
    {
        [SerializeField] private Texture2D texture;
        [SerializeField] private Color color;
        [SerializeField] private float brightness;

        public Material material;

        private static readonly HashSet<LensFlare> Flares = new();

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
            var mat = Matrix4x4.LookAt(transform.position, renderer.Camera.cameraToWorldMatrix.GetColumn(3), Vector3.up);
            renderer.Buffer.DrawProcedural(mat, material, -1, MeshTopology.Triangles, 4);
        }

        public static void DrawAll(CameraRenderer renderer)
        {
            foreach (var flare in Flares)
            {
                flare.Draw(renderer);
            }
        }
    }
}
