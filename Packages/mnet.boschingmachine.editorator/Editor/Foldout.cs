using UnityEditor;
using UnityEngine;

namespace Editorator.Editor
{
    public class Foldout
    {
        public string reference;
        public Header header;
        public Body body;
        public bool toggleOnLabelClick = true;

        private static readonly Data<bool> FoldoutData = new();

        public Foldout(string reference)
        {
            this.reference = reference;
            
            header = DefaultHeader;
            body = () => {};
        }
        
        public void Finish()
        {
            var s = EditorGUILayout.Foldout(FoldoutData.Read(reference, false), "", toggleOnLabelClick);
            var r = Utility.GetLastRect();
            header(r);
            FoldoutData.Write(reference, s);
            if (!s) return;

            using (new EditorGUI.IndentLevelScope())
            {
                body();
            }
        }

        private void DefaultHeader(Rect r) => GUI.Label(r, reference);
    }
}