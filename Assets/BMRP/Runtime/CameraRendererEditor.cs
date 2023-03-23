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
        string SampleName { get; set; }

        private static readonly ShaderTagId[] legacyShaderTagIds =
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };

        private partial void PrepareBuffer()
        {
            Profiler.BeginSample("Editor Only");
            buffer.name = SampleName = camera.name;
            Profiler.EndSample();
        }

        private partial void DrawUnsupportedShaders()
        {
            var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
            {
                overrideMaterial = errorMaterial,
            };

            for (var i = 1; i < legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
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