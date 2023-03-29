using UnityEditor;
using UnityEngine;

namespace Editorator.Editor
{
    public class Separator : ScalableElement
    {
        public float alignment = 0.5f;

        public override void Finish(Rect r)
        {
            var rect = r;
            rect.y += rect.height * alignment;
            rect.height = 1.0f;
            EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.2f));            
        }
    }
}