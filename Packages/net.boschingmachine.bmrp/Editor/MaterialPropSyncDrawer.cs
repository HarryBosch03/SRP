using BMRP.Runtime.PostFX;
using UnityEditor;
using UnityEngine;

namespace BMRP.Editor
{
    [CustomPropertyDrawer(typeof(PostEffect.MaterialPropSync), true)]
    public class MaterialPropSyncDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), label);
        }
    }
}