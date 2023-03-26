using System.Collections.Generic;
using Code.Scripts.Utility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Scripts.Editor
{
    public abstract class BetterEditor<T> : BetterEditor where T : Object
    {
        protected T Target => (T)target;
    }

    public abstract class BetterEditor : UnityEditor.Editor
    {
        private static readonly Dictionary<string, bool> FoldoutStates = new();

        protected static void Section(string name, System.Action bodyCallback, bool drawBackground = true) => Section(name, bodyCallback, s => s, drawBackground);
        protected static void Section(string name, System.Action bodyCallback, System.Func<GUIStyle, GUIStyle> style, bool drawBackground = true)
        {
            if (!Foldout(name)) return;

            var indent = EditorGUI.indentLevel++;
            if (drawBackground)
            {
                using var section = new EditorGUILayout.VerticalScope(style(EditorStyles.helpBox));
                bodyCallback();
                EditorGUI.indentLevel = indent;
            }
            else
            {
                bodyCallback();
                EditorGUI.indentLevel = indent;
            }
        }

        protected static bool Foldout(string name)
        {
            var get = GetFoldoutState(name);
            var set = EditorGUILayout.Foldout(get, name, true);
            SetFoldoutState(name, set);
            return set;
        }

        private static bool GetFoldoutState(string name)
        {
            name = Util.SimplifyName(name);
            return FoldoutStates.ContainsKey(name) && FoldoutStates[name];
        }

        private static void SetFoldoutState(string name, bool state)
        {
            name = Util.SimplifyName(name);

            if (FoldoutStates.ContainsKey(name)) FoldoutStates[name] = state;
            else FoldoutStates.Add(name, state);
        }

        protected static void Separator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 10.0f);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height / 2.0f, rect.width, 1), new Color(1, 1, 1, 0.1f));
        }
    }
}