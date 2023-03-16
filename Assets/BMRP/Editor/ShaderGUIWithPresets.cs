using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Editor
{
    public class ShaderGUIWithPresets : ShaderGUI
    {
        private MaterialEditor editor;
        private Object[] materials;
        private MaterialProperty[] properties;

        private bool presetFoldout;
        
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);
            
            editor = materialEditor;
            materials = materialEditor.targets;
            this.properties = properties;

            using var sec = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            
            presetFoldout = EditorGUILayout.Foldout(presetFoldout, "Presets", true);
            if (!presetFoldout) return;

            var indent = EditorGUI.indentLevel++;

            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();

            EditorGUI.indentLevel = indent;
        }

        private RenderQueue RenderQueue 
        {
            set 
            {
                foreach (Material m in materials) 
                {
                    m.renderQueue = (int)value;
                }
            }
        }
        
        private bool Clipping 
        {
            set => SetProperty("_Clipping", "_CLIPPING", value);
        }

        private bool PremultiplyAlpha 
        {
            set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
        }

        private BlendMode SrcBlend 
        {
            set => SetProperty("_SrcBlend", (float)value);
        }

        private BlendMode DstBlend 
        {
            set => SetProperty("_DstBlend", (float)value);
        }

        private bool ZWrite 
        {
            set => SetProperty("_ZWrite", value ? 1f : 0f);
        }

        private bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");
        private bool HasProperty (string name) => FindProperty(name, properties, false) != null;
        
        private void SetProperty (string name, float value) 
        {
            FindProperty(name, properties).floatValue = value;
        }

        private void SetProperty (string name, string keyword, bool value) 
        {
            SetProperty(name, value ? 1f : 0f);
            SetKeyword(keyword, value);
        }
        
        private void SetKeyword (string keyword, bool enabled) 
        {
            if (enabled) 
            {
                foreach (Material m in materials) 
                {
                    m.EnableKeyword(keyword);
                }
            }
            else 
            {
                foreach (Material m in materials) 
                {
                    m.DisableKeyword(keyword);
                }
            }
        }

        private bool PresetButton (string name) 
        {
            if (!GUILayout.Button(name)) return false;
            
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }

        private void OpaquePreset ()
        {
            if (!PresetButton("Opaque")) return;
            
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }

        private void ClipPreset ()
        {
            if (!PresetButton("Clip")) return;
            
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }

        private void FadePreset ()
        {
            if (!PresetButton("Fade")) return;
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }

        private void TransparentPreset ()
        {
            if (!HasPremultiplyAlpha) return;
            if (!PresetButton("Transparent")) return;
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }
}
