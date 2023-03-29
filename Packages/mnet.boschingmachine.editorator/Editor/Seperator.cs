using UnityEditor;
using UnityEngine;

namespace Editorator.Editor
{
    public static partial class E
    {
        public static void Separator(float alignment = 0.5f, float lineHeight = 1.0f)
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * lineHeight);
            rect.y += rect.height * alignment;
            rect.height = 1.0f;
            EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.2f));
        }
    }
}