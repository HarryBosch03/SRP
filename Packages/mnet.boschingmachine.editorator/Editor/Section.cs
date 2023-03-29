using System;
using UnityEditor;

namespace Editorator.Editor
{
    public static partial class E
    {
        public static void Section(Action body)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            using (new EditorGUI.IndentLevelScope())
            {
                body();
            }
        }
    }
}