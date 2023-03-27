using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public partial class CameraRenderer
    {
        private const string BufferName = "Render Camera";

        private CullingResults cullingResults;
        private ScriptableRenderContext context;
        private Lighting lighting;
        private readonly PostFXStack postFXStack = new();
        private CameraSettings settings;

        private static readonly Material ErrorMaterial = new(Shader.Find("Hidden/InternalErrorShader"));
        
        private static readonly ShaderTagId 
            UnlitShaderTagId = new("SRPDefaultUnlit"),
            LitShaderTagId = new("BMLit");

        private static readonly ShaderProperty<Vector4> 
            ScreenSize = ShaderPropertyFactory.Vec("screenSize");

        private static readonly ShaderProperty<float>
            WobbleAmount = ShaderPropertyFactory.Float("_WobbleAmount");
        
        public Camera Camera { get; private set; }
        public CommandBuffer Buffer { get; } = new()
        {
            name = BufferName,
        };
        
        public void Render(ScriptableRenderContext context, Camera camera, CameraSettings settings)
        {
            this.context = context;
            this.Camera = camera;
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
            context.SetupCameraProperties(Camera);
            var clearFlags = Camera.clearFlags;
            
            postFXStack.Setup(context, Camera, Buffer, settings.postFXSettings, ref clearFlags);
            
            Buffer.ClearRenderTarget(
                clearFlags <= CameraClearFlags.Depth, 
                clearFlags == CameraClearFlags.Color, 
                clearFlags == CameraClearFlags.Color ? Camera.backgroundColor.linear : Color.clear);
            Buffer.BeginSample(BufferName);
            ExecuteBuffer();

            SetGlobals();
        }

        private void SetGlobals()
        {
            ScreenSize.Value = new Vector4(Screen.width, Screen.height, 1.0f / Screen.width, 1.0f / Screen.height);
            WobbleAmount.Value = settings.globalVertexWobbleAmount;

            ShaderProperty.SendAll(Buffer, ScreenSize, WobbleAmount);
            
            ColorGradingVolume.SetWeights(this);
        }

        private bool Cull()
        {
            if (!Camera.TryGetCullingParameters(out var p)) return true;
            
            cullingResults = context.Cull(ref p);
            return false;
        }
        
        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
        {
            var sortingSettings = new SortingSettings(Camera)
            {
                criteria = SortingCriteria.CommonOpaque,
            };
            var drawingSettings = new DrawingSettings(UnlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing,
                perObjectData = PerObjectData.LightProbe
            };
            drawingSettings.SetShaderPassName(1, LitShaderTagId);
            
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
            
            context.DrawSkybox(Camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
            
            LensFlare.DrawAll(this);
        }

        private void Submit()
        {
            Buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();   
        }

        private void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(Buffer);
            Buffer.Clear();
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

            [SerializeField] public float globalVertexWobbleAmount = 6.0f;
            
            [Space] [SerializeField] public PostFXSettings postFXSettings;
        }
    }
}