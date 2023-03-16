using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public partial class CameraRenderer
    {
#if UNITY_EDITOR
        private static ShaderTagId[] legacyShaderTagIds =
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };

        private static Material errorMaterial;
        
        string SampleName { get; set; }

        partial void DrawUnsupportedShaders()
        {
            if (!errorMaterial) errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));

            var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
            {
                overrideMaterial = errorMaterial
            };

            for (var i = 1; i < legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            }

            var filterSettings = FilteringSettings.defaultValue;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
        }

        partial void DrawGizmos()
        {
            if (!Handles.ShouldRenderGizmos()) return;
            
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }

        partial void PrepareForSceneWindow()
        {
            if (camera.cameraType != CameraType.SceneView) return;
            
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }

        partial void PrepareBuffer()
        {
            Profiler.BeginSample("Editor Only");
            buffer.name = camera.name;
            SampleName = camera.name;
            Profiler.EndSample();
        }
#else
        string SampleName => bufferName;
#endif

        partial void DrawUnsupportedShaders();
        partial void DrawGizmos();
        partial void PrepareForSceneWindow();
        partial void PrepareBuffer();
    }
}