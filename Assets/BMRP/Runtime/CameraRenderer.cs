using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public partial class CameraRenderer
    {
        private const string BufferName = "Render Camera";

        private readonly CommandBuffer buffer = new()
        {
            name = BufferName,
        };

        private CullingResults cullingResults;
        private ScriptableRenderContext context;
        private Camera camera;
        private Lighting lighting;
        private PostFXStack postFXStack = new PostFXStack();
        private CameraSettings settings;

        private static readonly Material errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        
        private static readonly ShaderTagId 
            unlitShaderTagId = new("SRPDefaultUnlit"),
            litShaderTagId = new ShaderTagId("BMLit");

        private static readonly int screenSize = Shader.PropertyToID("screenSize");

        public void Render(ScriptableRenderContext context, Camera camera, CameraSettings settings)
        {
            this.context = context;
            this.camera = camera;
            this.settings = settings;

            PrepareBuffer();
            PrepareForSceneWindow();
            if (Cull()) return;
            
            Setup();

            lighting = new Lighting();
            lighting.Setup(context, cullingResults);
            DrawVisibleGeometry(settings.useDynamicBatching, settings.useGPUInstancing);
            DrawUnsupportedShaders();
            
            DrawGizmosBeforeFX();
            postFXStack.Render();
            DrawGizmosAfterFX();
            
            Cleanup();
            
            Submit();
        }

        private void Setup()
        {
            context.SetupCameraProperties(camera);
            var clearFlags = camera.clearFlags;
            
            postFXStack.Setup(context, camera, settings.postFXSettings, buffer, ref clearFlags);
            
            buffer.ClearRenderTarget(
                clearFlags <= CameraClearFlags.Depth, 
                clearFlags == CameraClearFlags.Color, 
                clearFlags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
            buffer.BeginSample(BufferName);
            ExecuteBuffer();

            SetGlobals();
        }

        private void SetGlobals()
        {
            Shader.SetGlobalVector(screenSize, new Vector4(Screen.width, Screen.height, 1.0f / Screen.width, 1.0f / Screen.height));
        }

        private bool Cull()
        {
            if (!camera.TryGetCullingParameters(out var p)) return true;
            
            cullingResults = context.Cull(ref p);
            return false;
        }
        
        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
        {
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque,
            };
            var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing,
                perObjectData = PerObjectData.LightProbe
            };
            drawingSettings.SetShaderPassName(1, litShaderTagId);
            
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
            
            context.DrawSkybox(camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
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
        
        private void Cleanup()
        {
            postFXStack.Cleanup();
        }
        
        [System.Serializable]
        public class CameraSettings
        {
            [SerializeField] 
            public bool useDynamicBatching = true, useGPUInstancing = true, useSrpBatching = true;

            [Space] [SerializeField] public PostFXSettings postFXSettings;
        }
    }
}
