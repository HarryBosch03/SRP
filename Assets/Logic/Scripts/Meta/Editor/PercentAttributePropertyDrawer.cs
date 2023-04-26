using UnityEditor;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Meta.Editor
{
    [CustomPropertyDrawer(typeof(PercentAttribute))]
    public class PercentAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.floatValue = EditorGUI.IntSlider(position, label + " (%)", (int)(property.floatValue * 100.0f), 0, 100) / 100.0f;
        }
    }
}
