using UnityEditor;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Elevator.Editor
{
    [CustomEditor(typeof(ElevatorCallSwitch))]
    public class ElevatorCallSwitchEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (PrefabUtility.IsPartOfAnyPrefab((ElevatorCallSwitch)target)) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label($"Currently Calls floor {((ElevatorCallSwitch)target).GetButtonCallIndex()}");
            }
        }
    }
}
