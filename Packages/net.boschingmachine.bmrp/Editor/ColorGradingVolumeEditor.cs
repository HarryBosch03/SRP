using BMRP.Runtime;
using Code.Scripts.Editor;
using UnityEditor;
using UnityEngine;

namespace BMRP.Editor
{
    [CustomEditor(typeof(ColorGradingVolume))]
    public class ColorGradingVolumeEditor : BetterEditor<ColorGradingVolume>
    {
        private SerializedProperty
            temperature,
            tint;
        
        public override void OnInspectorGUI()
        {
            Section("Balance", () =>
            {
                DefaultProperty("exposureValue", "Exposure (EV)", p => p.floatValue = 0.0f);
                DefaultProperty("contrastValue", "Contrast", p => p.floatValue = 1.0f);

                temperature = serializedObject.FindProperty("temperatureValue");
                tint = serializedObject.FindProperty("tintValue");
                
                TemperatureSlider(temperature, "Temperature", Color.blue, Color.red, temperature.floatValue, 0.0f);
                TemperatureSlider(tint, "Tint", Color.magenta, Color.green, 0.0f, tint.floatValue);
            });

            Separator();

            Section("Blending", () =>
            {
                Properties("global", "weight", "blendDistance");
            });
            
            serializedObject.ApplyModifiedProperties();
        }

        public void TemperatureSlider(SerializedProperty property, string label, Color colA, Color colB, float temperature, float tint)
        {
            void draw(Rect r)
            {
                property.floatValue = EditorGUI.Slider(r, label, property.floatValue, -1.0f, 1.0f);

                var col = Util.WhiteBalance(Color.white, temperature, tint);
                col.a = 0.2f;
                EditorGUI.DrawRect(r, col);
            }
            
            DefaultField(draw, () => property.floatValue = 0.0f);
        }
    }
}
