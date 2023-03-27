using System.Collections.Generic;
using System.IO;
using BMRP.Runtime;
using Code.Scripts.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BMRP.Editor
{
    [CustomEditor(typeof(PostFXSettings))]
    public class PostFXSettingsEditor : BetterEditor
    {
        private PostFXSettings Target => target as PostFXSettings;

        public override void OnInspectorGUI()
        {
            Section("General Settings", () => { base.OnInspectorGUI(); });
            Separator();
            SectionWithoutBackground("Material Settings", () =>
            {
                foreach (var effects in Target.Effects)
                {
                    SectionWithoutBackground(effects.name, () =>
                    {
                        var editor = CreateEditor(effects);
                        InternalEditorUtility.SetIsInspectorExpanded(effects, true);
                        editor.OnInspectorGUI();
                    });
                }
            });
            Separator();
            Section("Utility", () =>
            {
                var defaultMaterials = new List<Material>();

                const string searchPath = "Packages/net.boschingmachine.bmrp/runtime/materials/post process";
                foreach (var path in Directory.GetFiles(searchPath))
                {
                    var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (material)
                    {
                        defaultMaterials.Add(material);
                    }
                }
                
                foreach (var material in defaultMaterials)
                {
                    if (!GUILayout.Button($"Add {material.name}")) continue;
                    
                    Target.Effects.Add(material); 
                    EditorUtility.SetDirty(target);
                }
            });
        }
    }
}