using UnityEditor;
using UnityEngine;

namespace Editorator.Editor
{
    public abstract class ScalableElement : Element
    {
        public float height = 1.0f;
        
        public sealed override void Finish()
        {
            Finish(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * height));
        }

        public abstract void Finish(Rect r);
    }
}