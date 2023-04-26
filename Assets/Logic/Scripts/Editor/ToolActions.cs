using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Editor
{
    public static class ToolActions
    {
        [MenuItem("Tools/Actions/Reset Scale")]
        public static void ResetScale()
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                ResetScale(gameObject.transform);
            }
        }

        public static void ResetScale(Transform transform)
        {
            var undoObjects = new List<Object>();
            foreach (Transform child in transform) undoObjects.Add(child);
            undoObjects.Add(transform);

            Undo.RecordObjects(undoObjects.ToArray(), $"Reset Scale on {transform.name}");

            var matrix = Matrix4x4.Scale(transform.localScale);
            var invMatrix = matrix.inverse;

            Vector4 p(Vector3 v) => new Vector4(v.x, v.y, v.z, 1.0f);

            foreach (Transform child in transform)
            {
                child.localPosition = matrix * p(child.localPosition);
                child.localScale = matrix * child.localScale;
            }

            transform.localScale = invMatrix * transform.localScale;
        }
    }
}
