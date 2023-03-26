using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public partial class CameraRenderer
    {
        private partial void PrepareBuffer();
        private partial void DrawUnsupportedShaders();
        private partial void PrepareForSceneWindow();
        private partial void DrawGizmosBeforeFX();
        private partial void DrawGizmosAfterFX();

#if UNITY_EDITOR
        private string SampleName { get; set; }

        private static readonly ShaderTagId[] LegacyShaderTagIds =
        {
            new("Always"),
            new("ForwardBase"),
            new("PrepassBase"),
            new("Vertex"),
            new("VertexLMRGBM"),
            new("VertexLM"),
        };

        private partial void PrepareBuffer()
        {
            Profiler.BeginSample("Editor Only");
            buffer.name = SampleName = camera.name;
            Profiler.EndSample();
        }

        private partial void DrawUnsupportedShaders()
        {
            var drawingSettings = new DrawingSettings(LegacyShaderTagIds[0], new SortingSettings(camera))
            {
                overrideMaterial = ErrorMaterial,
            };

            for (var i = 1; i < LegacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, LegacyShaderTagIds[i]);
            }

            var filteringSettings = FilteringSettings.defaultValue;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private partial void PrepareForSceneWindow()
        {
            if (camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
        }

        private partial void DrawGizmosBeforeFX()
        {
            if (!UnityEditor.Handles.ShouldRenderGizmos()) return;
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
        }

        private partial void DrawGizmosAfterFX()
        {
            if (!UnityEditor.Handles.ShouldRenderGizmos()) return;
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
#else
    const string SampleName = bufferName;
#endif
    }
}