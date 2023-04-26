using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Editor
{
    public static class FancyLines
    {
        public static void DottedLine (Vector3 a, Vector3 b, float size = 0.1f, float offset = 0)
        {
            float distance = 0.0f;
            float length = (b - a).magnitude;
            float actualLength = length;
            length = Mathf.Ceil(length / 2.0f) * 2.0f;
            float sf = actualLength / length;

            Vector3 dir = (b - a) / actualLength;
            while (distance < length)
            {
                float l1 = (distance + offset) % length;
                float l2 = Mathf.Min(l1 + size, length);

                Handles.DrawAAPolyLine(a + dir * l1 * sf, a + dir * l2 * sf);
                distance += size * 2.0f;
            }
        }

        public static void DotLine(Vector3 a, Vector3 b, float size = 0.1f, float offset = 0)
        {
            float distance = 0.0f;
            float length = (b - a).magnitude;
            float actualLength = length;
            length = Mathf.Ceil(length / 2.0f) * 2.0f;
            float sf = actualLength / length;

            Vector3 dir = (b - a) / actualLength;

            var points = new List<Vector3>();

            while (distance < length)
            {
                float l1 = (distance + offset) % length;
                Vector3 point = a + dir * l1 * sf; 
                
                points.Add(point);
                
                distance += size * 2.0f;
            }

            var toCamMatrix = SceneView.currentDrawingSceneView.camera.worldToCameraMatrix;
            points = points.OrderBy(e => -(toCamMatrix * new Vector4(e.x, e.y, e.z, 1.0f)).sqrMagnitude).ToList();

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = points[i];
                Handles.SphereHandleCap(0, points[i], Quaternion.identity, size, EventType.Repaint);
            }
        }
    }
}
