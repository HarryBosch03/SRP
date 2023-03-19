using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public partial class CameraRenderer
    {
        private const string BufferName = "Render Camera";

        private readonly CommandBuffer buffer = new()
        {
            name = BufferName
        };

        private static readonly ShaderTagId UnlitShaderTagID = new("SRPDefaultUnlit");
        private static readonly ShaderTagId LitShaderTagID = new("BMLit");

        private ScriptableRenderContext context;
        private Camera camera;
        private CullingResults cullingResults;

        private readonly Lighting lighting = new();

        public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadows)
        {
            this.context = context;
            this.camera = camera;
            
            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull(shadows.maxDistance)) return;
            
            buffer.BeginSample(SampleName);
            ExecuteBuffer();
            lighting.Setup(context, cullingResults, shadows);
            buffer.EndSample(SampleName);
            
            Setup();
            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
            DrawUnsupportedShaders();
            DrawGizmos();
            lighting.Cleanup();
            Submit();
        }

        private void Setup()
        {
            context.SetupCameraProperties(camera);
            var flags = camera.clearFlags;
            buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
            buffer.BeginSample(SampleName);
            ExecuteBuffer();
        }

        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
        {
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(UnlitShaderTagID, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing,
                perObjectData = PerObjectData.Lightmaps | 
                                PerObjectData.ShadowMask | 
                                PerObjectData.LightProbe | 
                                PerObjectData.LightProbeProxyVolume | 
                                PerObjectData.ReflectionProbes | 
                                PerObjectData.OcclusionProbe | 
                                PerObjectData.OcclusionProbeProxyVolume,
            };
            drawingSettings.SetShaderPassName(1, LitShaderTagID);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

            context.DrawSkybox(camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;

            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private void Submit()
        {
            buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        private void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        private bool Cull(float maxShadowDistance)
        {
            if (!camera.TryGetCullingParameters(out var p)) return false;

            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref p);
            return true;
        }
    }
}