using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editorator.Editor
{
    public static partial class E
    {
        public static void Button(string text, Action content)
        {
            Button(new GUIContent(text), content);
        }

        public static void Button(Action<Rect> header, Action content)
        {
            Rect r;
            if (GUILayout.Button(""))
            {
                r = GUILayoutUtility.GetLastRect();
                content();
            }
            else r = GUILayoutUtility.GetLastRect();

            header(r);
        }

        public static void Button(GUIContent buttonContent, Action content)
        {
            if (GUILayout.Button(buttonContent)) content();
        }
        
        public static void ImageButton(Texture image, Action content)
        {
            Button(new GUIContent(image), content);
        }

        public static void ImageButton(string imageRef, Action content)
        {
            var img = EditorGUIUtility.IconContent(imageRef).image;
            ImageButton(img, content);
        }
    }
}