using System.Collections.Generic;
using BMRP.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BMRP.Editor
{
    [CustomEditor(typeof(PostFXSettings))]
    public class PostFXSettingsEditor : UnityEditor.Editor
    {
        private PostFXSettings Target => target as PostFXSettings;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(CollapsableSection("Render Settings",
                local => { local.Add(new IMGUIContainer(OnInspectorGUI)); }));

            root.Add(CollapsableSection("Post Effects", local =>
            {
                foreach (var effect in Target.Effects)
                {
                    local.Add(PostEffect(effect));
                }
            }));
            
            root.Add(CollapsableSection("Utility", local =>
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath("Packages/net.boschingmachine.bmrp/runtime/shaders/post processing");
                var defMats = new List<Material>();
                
                foreach (var asset in assets)
                {
                    if (asset is Material mat)
                    {
                        defMats.Add(mat);
                    }
                }

                foreach (var mat in defMats)
                {
                    local.Add(new Label(mat.name));
                }
            }));

            return root;
        }

        public VisualElement CollapsableSection(string title, System.Action<VisualElement> contentCallback)
        {
            var root = new VisualElement();
            var box = new Box();
            root.Add(box);

            var foldout = new Foldout() { text = title };
            box.Add(foldout);
            contentCallback(foldout.contentContainer);

            return root;
        }

        public VisualElement PostEffect(Material material)
        {
            if (!material) return new VisualElement();

            return CollapsableSection(material.name, local =>
            {
                var editor = CreateEditor(material);
                local.Add(new IMGUIContainer(editor.DrawHeader));
                local.Add(new IMGUIContainer(editor.OnInspectorGUI));
            });
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}