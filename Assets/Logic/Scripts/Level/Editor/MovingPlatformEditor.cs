using UnityEditor;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Editor
{
    [CustomEditor(typeof(MovingPlatform))]
    public class MovingPlatformEditor : UnityEditor.Editor
    {
        new MovingPlatform target => base.target as MovingPlatform;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target.PathParent)
            {
                if (GUILayout.Button("Add Path Point"))
                {
                    var newPoint = Instantiate(target.PathParent.GetChild(target.PathParent.childCount - 1), target.PathParent);
                    newPoint.SetAsLastSibling();
                    newPoint.name = $"Path Point.{newPoint.GetSiblingIndex() + 1}";
                }
            }
        }

        private void OnSceneGUI()
        {
            if (!target) return;
            if (!target.PathParent) return;
            if (target.PathParent.childCount == 0) return;

            foreach (Transform child in target.PathParent)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(child.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(child, "Moved Moving Platform Path Point");
                    if (target.Platform)
                    {
                        Undo.RecordObject(target.Platform.transform, "Snapped Platform to Moved Path Point");
                        target.Platform.transform.position = pos;
                    }
                    child.position = pos;
                }
            }

            Vector3 meanPosition = Vector3.zero;
            foreach (Transform child in target.PathParent)
            {
                meanPosition += child.position;
            }
            meanPosition /= target.PathParent.childCount;
            EditorGUI.BeginChangeCheck();
            Vector3 newMean = Handles.PositionHandle(meanPosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 diff = newMean - meanPosition;
                foreach (Transform child in target.PathParent)
                {
                    Undo.RecordObject(child, "Moved Path Mean");
                    child.position += diff;
                }
                if (target.Platform)
                {
                    Undo.RecordObject(target.Platform.transform, "Snapped Platform to Moved Path Point");
                    target.Platform.transform.position += diff;
                }
            }

            for (int i = 0; i < target.PathParent.childCount; i++)
            {
                Vector3 a = target.PathParent.GetChild(i).position;
                Vector3 b = target.PathParent.GetChild((i + 1) % target.PathParent.childCount).position;

                Handles.color = Color.yellow;
                DrawArrow(a, b);
            }
        }

        public void DrawArrow(Vector3 a, Vector3 b)
        {
            Vector3 mid = (a + b) / 2.0f;
            Vector3 dir = (b - a).normalized;

            Vector3 up = Vector3.Cross(dir, Vector3.Cross(dir, Vector3.up).normalized);

            float scale = 0.5f;

            Handles.DrawLine(a, b);
            Handles.DrawLine(mid + dir * scale, mid + (up - dir) * scale);
            Handles.DrawLine(mid + dir * scale, mid + (-up - dir) * scale);
        }
    }
}
