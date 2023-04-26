using UnityEditor;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Signal_Groups.Editor
{
    [CustomEditor(typeof(SignalGroup))]
    public class SignalGroupEditor : UnityEditor.Editor
    {
        new public SignalGroup target => base.target as SignalGroup;

        private void OnSceneGUI()
        {
            Vector3 meanPos = Vector3.zero;
            foreach (Transform child in target.transform)
            {
                meanPos += child.position;
            }
            meanPos /= target.transform.childCount;

            Handles.color = Color.yellow;
            foreach (Transform child in target.transform)
            {
                Handles.DrawAAPolyLine(meanPos, child.position);
            }
        }

        private void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }
    }
}
