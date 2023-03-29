using System;
using UnityEditor;
using UnityEngine;

namespace Editorator.Editor
{
    public static partial class E
    {
        private static readonly Data<bool> FoldoutData = new();

        public static void Foldout(string name, Action body)
        {
            Foldout(name, r => EditorGUI.LabelField(r, name), body);
        }

        public static void Foldout(string reference, Action<Rect> header, Action body)
        {
            var s = EditorGUILayout.Foldout(FoldoutData.Read(reference, false), "", true);
            var r = GUILayoutUtility.GetLastRect();
            header(r);
            FoldoutData.Write(reference, s);
            if (!s) return;

            using (new EditorGUI.IndentLevelScope())
            {
                body();
            }
        }
    }
}