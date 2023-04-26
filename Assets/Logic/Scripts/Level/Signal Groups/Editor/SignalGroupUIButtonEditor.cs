using UnityEditor;
using UnityEditor.UI;

namespace BoschingMachine.Logic.Scripts.Level.Signal_Groups.Editor
{
    [CustomEditor(typeof(SignalGroupUIButton))]
    public class SignalGroupUIButtonEditor : ButtonEditor
    {
        new public SignalGroupUIButton target => (SignalGroupUIButton)base.target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Property("toggle");
            if (!target.toggle) Property("callData");

            serializedObject.ApplyModifiedProperties();
        }

        public void Property (string name)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
        }
    }
}
