using System.Collections.Generic;
using System.Reflection;
using BMRP.Runtime;
using UnityEditor;
using UnityEngine;
using BMRP.Runtime.PostFX;
using UnityEditor.Compilation;
using UnityEditorInternal;

namespace BMRP.Editor
{
    [CustomEditor(typeof(PostFXSettings))]
    public class PostFXSettingsEditor : BetterEditor<PostFXSettings>
    {
        private readonly List<PostEffect> buffer = new();
        
        public override void OnInspectorGUI()
        {
            ReinstanceEffects();

            buffer.Clear();
            buffer.AddRange(Target.Effects);
            
            Foldout(0, "General Settings", () => { base.OnInspectorGUI(); });

            Separator();

            Foldout(1, "Post Effect Settings", () =>
            {
                foreach (var effect in Target.Effects)
                {
                    DrawEffect(effect);
                }
            });
            
            Target.Effects.Clear();
            Target.Effects.AddRange(buffer);
        }

        private void ReinstanceEffects()
        {
            var looseEffects = new List<PostEffect>();
            looseEffects.AddRange(FindObjectsOfType<PostEffect>());

            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Target));
            foreach (var asset in assets)
            {
                if (asset is not PostEffect effect) continue;
                looseEffects.Add(effect);
            }

            foreach (var looseEffect in looseEffects)
            {
                if (Target.Effects.Contains(looseEffect)) continue;
                
                if (Target.Effects.Exists(e => e.GetType() == looseEffects.GetType()))
                {
                    DestroyImmediate(looseEffect, true);
                    continue;
                }
                Target.Effects.Add(looseEffect);
            }

            var dirty = false;
            Target.ReinstanceEffects(effect =>
            {
                AssetDatabase.AddObjectToAsset(effect, Target);
                dirty = true;
            });

            foreach (var effect in Target.Effects)
            {
                if (effect.name == effect.DisplayName) continue;
                dirty = true;
                effect.name = effect.DisplayName;
            }

            if (!dirty) return;
            
            AssetDatabase.SaveAssets();                
            EditorUtility.SetDirty(Target);
            SceneView.RepaintAll();
        }

        private void DrawEffect(PostEffect effect)
        {
            using (new Section())
            {
                var i = effect.GetInstanceID();
                using (new EditorGUILayout.HorizontalScope())
                {
                    foldoutState[i] = EditorGUILayout.Foldout(foldoutState[i], effect.name, true);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Move Prev"))
                    {
                        var j = buffer.FindIndex(e => e == effect);
                        if (j > 0)
                        {
                            var a = buffer[j];
                            var b = buffer[j - 1];

                            buffer[j] = b;
                            buffer[j - 1] = a;
                        }
                    }

                    if (GUILayout.Button("Move Next"))
                    {
                        var j = buffer.FindIndex(e => e == effect);
                        if (j < buffer.Count - 1)
                        {
                            var a = buffer[j];
                            var b = buffer[j + 1];

                            buffer[j] = b;
                            buffer[j + 1] = a;
                        }
                    }
                }

                if (!foldoutState[i]) return;

                var editor = CreateEditor(effect);
                editor.OnInspectorGUI();
            }
        }
    }
}