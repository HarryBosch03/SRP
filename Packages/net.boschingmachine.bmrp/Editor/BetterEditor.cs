using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BMRP.Editor
{
    public abstract class BetterEditor<T> : BetterEditor where T : Object
    {
        protected T Target => (T)target;
    }

    public abstract class BetterEditor : UnityEditor.Editor
    {
        protected readonly FoldoutStateGroup foldoutState = new();

        protected void OnEnable()
        {
            foldoutState.Initialize(target);
        }

        protected class Section : IDisposable
        {
            private readonly EditorGUILayout.VerticalScope v;

            public Section()
            {
                v = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            }

            public void Dispose()
            {
                v.Dispose();
            }
        }

        protected void FoldoutSection(int key, string label, Action callback, bool toggleOnLabelClick = true)
        {
            using (new Section())
            {
                Foldout(key, label, callback, toggleOnLabelClick);
            }
        }

        protected void Foldout(int key, string label, Action callback, bool toggleOnLabelClick = true)
        {
            foldoutState[key] = EditorGUILayout.Foldout(foldoutState[key], label, toggleOnLabelClick);
            if (!foldoutState[key]) return;

            using (new EditorGUI.IndentLevelScope())
            {
                callback();
            }
        }

        public class FoldoutStateGroup
        {
            private readonly Dictionary<int, bool> states = new();
            private int id;
            
            public void Initialize(Object target)
            {
                id = target.GetInstanceID();
            }
            
            public bool this[int key]
            {
                get
                {
                    if (!states.ContainsKey(key)) states.Add(key, EditorPrefs.GetBool(id.ToString(), false));
                    return states[key];
                }
                set
                {
                    if (states.ContainsKey(key)) states[key] = value;
                    else states.Add(key, value);
                    EditorPrefs.SetBool(id.ToString(), value);
                }
            }
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

        private Action<Rect> DefaultPropertyDraw(string prop, GUIContent label = null) =>
            DefaultPropertyDraw(serializedObject.FindProperty(prop), label);

        private static Action<Rect> DefaultPropertyDraw(SerializedProperty prop, GUIContent label = null) =>
            r => EditorGUI.PropertyField(r, prop, label);

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

        public static void DefaultField(Action<Rect> drawCallback, ResetCallback resetCallback)
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