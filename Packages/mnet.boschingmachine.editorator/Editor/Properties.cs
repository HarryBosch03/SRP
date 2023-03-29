using UnityEditor;
using UnityEngine;

namespace Editorator.Editor
{
    public static partial class E
    {
        public static SerializedObject PropertyContent { get; set; }

        public static void PropertyContext(SerializedObject context)
        {
            PropertyContent = context;
        }
        
        public static void Property(string propertyPath)
        {
            var sp = PropertyContent.FindProperty(propertyPath);
            Property(sp);
        }

        public static void Property(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
        }

        public static void AllVisibleProperties(Object uo) => AllVisibleProperties(new SerializedObject(uo));
        public static void AllVisibleProperties(SerializedObject so)
        {
            var prop = so.GetIterator();
            prop.Next(true);
            prop.NextVisible(true);
            while (prop.NextVisible(true))
            {
                EditorGUILayout.PropertyField(prop);
            }

            so.ApplyModifiedProperties();
        }
    }
}