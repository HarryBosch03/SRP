using UnityEditor;
using UnityEngine;

namespace Editorator.Editor
{
    public static class Utility
    {
        public static Rect GetLastRect()
        {
            var r = GUILayoutUtility.GetLastRect();
            var indent = EditorGUI.indentLevel * 15.0f;
            r.x += indent;
            r.width -= indent;
            return r;
        }
    }
}