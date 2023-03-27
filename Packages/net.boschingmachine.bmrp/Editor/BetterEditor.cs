using System.Collections.Generic;
using BMRP.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
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

        protected static void Section(string name, System.Action bodyCallback) => Section(name, bodyCallback, s => s);
        protected static void SectionWithoutBackground(string name, System.Action bodyCallback) => Section(name, bodyCallback, s => GUIStyle.none);
        protected static void Section(string name, System.Action bodyCallback, System.Func<GUIStyle, GUIStyle> style)
        {
            Section(name, () => { }, bodyCallback, style);
        }

        protected static void Section(string reference, System.Action headerCallback, System.Action bodyCallback, System.Func<GUIStyle, GUIStyle> style)
        {
            using var section = new EditorGUILayout.VerticalScope(style(EditorStyles.helpBox));
            var indent = EditorGUI.indentLevel++;
            
            if (Foldout(reference, headerCallback)) bodyCallback();

            EditorGUI.indentLevel = indent;
        }

        protected static bool Foldout(string name)
        {
            return Foldout(name, () => {});
        }

        protected static bool Foldout(string reference, System.Action headerCallback)
        {
            var get = GetFoldoutState(reference);

            bool set;
            using (new EditorGUILayout.HorizontalScope())
            {
                set = EditorGUILayout.Foldout(get, reference, true);
                headerCallback();
            }
            SetFoldoutState(reference, set);
            return set;
        }

        private static bool GetFoldoutState(string name)
        {
            name = Util.SimplifyName(name);
            if (!FoldoutStates.ContainsKey(name)) FoldoutStates.Add(name, true);
            return FoldoutStates[name];
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

        public void Property(string propName, string label) => Property(propName, new GUIContent(label)); 
        public void Property(string propName, GUIContent label = null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propName), label);
        }
        
        public void Properties(params string[] propNames)
        {
            foreach (var propName in propNames)
            {
                Property(propName);
            }
        }

        private System.Action<Rect> DefaultPropertyDraw(string prop, GUIContent label = null) => DefaultPropertyDraw(serializedObject.FindProperty(prop), label);
        private static System.Action<Rect> DefaultPropertyDraw(SerializedProperty prop, GUIContent label = null) => r => EditorGUI.PropertyField(r, prop, label);
        
        public void DefaultProperty(string propertyName, ResetCallbackProperty resetCallback)
        {
            DefaultProperty(propertyName, (GUIContent)null, resetCallback);
        }

        public void DefaultProperty(string propertyName, string label, ResetCallbackProperty resetCallback)
        {
            DefaultProperty(propertyName, new GUIContent(label), resetCallback);
        }

        public void DefaultProperty(string propertyName, GUIContent label, ResetCallbackProperty resetCallback)
        {
            var prop = serializedObject.FindProperty(propertyName);
            DefaultField(DefaultPropertyDraw(prop), () => resetCallback(prop));
        }

        public static void DefaultField(System.Action<Rect> drawCallback, ResetCallback resetCallback)
        {
            var icon = EditorGUIUtility.IconContent("d_Refresh@2x").image;
            var r = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

            r.width -= r.height * 2.0f;
            drawCallback(r);

            r.x += r.width;
            r.width = r.height * 2.0f;
            
            if (GUI.Button(r, icon))
            {
                resetCallback();
            }
        }

        public delegate void ResetCallback();
        public delegate void ResetCallbackProperty(SerializedProperty prop);
    }
}