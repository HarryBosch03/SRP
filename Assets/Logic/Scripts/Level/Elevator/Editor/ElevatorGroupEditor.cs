using UnityEditor;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Elevator.Editor
{
    [CustomEditor(typeof(ElevatorGroup))]
    public class ElevatorGroupEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying) return;

            ElevatorGroup target = this.target as ElevatorGroup;
            if (!target) return;
            if (target.Floors == null) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int i = 0; i < target.Floors.Length; i++)
                {
                    if (GUILayout.Button($"Goto {Elevator.FormatIndex(i)}"))
                    {
                        target.Request(i);
                    }
                }
            }
        }
    }
}
