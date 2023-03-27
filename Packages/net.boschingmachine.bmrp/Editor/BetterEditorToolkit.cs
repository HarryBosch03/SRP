using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BMRP.Editor
{
    public class BetterEditorToolkit<T> : BetterEditorToolkit where T : Object
    {
        protected T Target => (T)target;
    }

    public class BetterEditorToolkit : UnityEditor.Editor
    {
        protected VisualElement Section(string name, ContentCallback callback)
        {
            var root = new Box();
            var foldout = new Foldout { text = name };
            root.Add(foldout);

            callback(foldout.contentContainer);
            
            return root;
        }

        protected static VisualElement Separator()
        {
            void separator()
            {
                var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                rect.y += rect.height / 2.0f;
                rect.height = 1.0f;
                EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.2f));
            }
            
            var root = new VisualElement();

            root.Add(new IMGUIContainer(separator));
            
            return root;
        }

        protected VisualElement Property(VisualElement parent, string varName)
        {
            var prop = new PropertyField(serializedObject.FindProperty(varName));
            parent.Add(prop);
            return prop;
        }

        protected void Property(VisualElement parent, params string[] varNames)
        {
            foreach (var varName in varNames)
            {
                Property(parent, varName);
            }
        }
        
        public delegate void ContentCallback(VisualElement local);
    }
}