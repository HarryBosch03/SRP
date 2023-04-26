using UnityEditor;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Elevator.Editor
{
    [CustomEditor(typeof(Elevator))]
    public class ElevatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var elevator = target as Elevator;
            var group = elevator.GetComponentInParent<ElevatorGroup>();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int i = 0; i < group.Floors.Length; i++)
                {
                    if (GUILayout.Button($"Goto {Elevator.FormatIndex(i)}"))
                    {
                        if (Application.isPlaying)
                        {
                            elevator.Request(i);
                        }
                        else
                        {
                            elevator.transform.position = elevator.HeightToPoint(group.Floors[i]);

                        }
                    }
                }
            }
        }
    }
}
