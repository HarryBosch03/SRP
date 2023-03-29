using System;
using UnityEditor;

namespace Editorator.Editor
{
    public class Section : Element
    {
        public Body body;
        
        public override void Finish()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            using (new EditorGUI.IndentLevelScope(1 - EditorGUI.indentLevel))
            {
                body();
            }
        }
    }
}