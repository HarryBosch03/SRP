using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editorator.Editor
{
    public class Button : ScalableElement
    {
        public GUIContent defaultContent;
        public Header content;
        public Action callback;

        public Button()
        {
            defaultContent = new GUIContent();
            content = r => { };
            callback = () => { };
        }

        public Button EditorImage(string reference)
        {
            defaultContent.image = EditorGUIUtility.IconContent(reference).image;
            return this;
        }
        
        public override void Finish(Rect r)
        {
            if (GUI.Button(r, defaultContent))
            {
                r = GUILayoutUtility.GetLastRect();
                callback();
            }
            else r = GUILayoutUtility.GetLastRect();

            content(r);
        }
    }
}