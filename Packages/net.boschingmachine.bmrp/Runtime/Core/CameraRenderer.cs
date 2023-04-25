using System.Collections.Generic;
using BMRP.Runtime.Core.Passes;
using BMRP.Runtime.PostFX;
using BMRP.Runtime.Scene;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime.Core
{
    public partial class CameraRenderer
    {
        private const string BufferName = "Render Camera";

        private Lighting.Lighting lighting;
        private readonly PostFXStack postFXStack = new();
        private Material depthMat = new Material(Shader.Find("Hidden/DepthWrite"));

        private static readonly Material ErrorMaterial = new(Shader.Find("Hidden/InternalErrorShader"));

        private static readonly ShaderTagId
            UnlitShaderTagId = new("SRPDefaultUnlit"),
            LitShaderTagId = new("BMLit");

        private static readonly ShaderProperty<float>
            WobbleAmount = ShaderPropertyFactory.Float("_WobbleAmount");

        private static readonly int DepthBufferID = Shader.PropertyToID("_DepthTex");
        
        public Camera Camera { get; private set; }
        public CameraSettings Settings { get; private set; }
        public CullingResults CullingResults { get; private set; }
        public ScriptableRenderContext Context { get; private set; }
        public CommandBuffer Buffer { get; } = new()
        {
            name = BufferName,
        };

        private readonly List<RenderPass> passList = new()
        {
            new DrawOpaquePass(),
            new DrawSkyboxPass(),
            new CopyDepthPass(),
            new FlarePass(),
            new DrawTransparentPass(),
        };
        
        public void Render(ScriptableRenderContext context, Camera camera, CameraSettings settings)
        {
            Context = context;
            Camera = camera;
            Settings = settings;

            PrepareBuffer();
            PrepareForSceneWindow();
            if (Cull()) return;
            
            Setup();

            lighting = new Lighting.Lighting();
            lighting.Setup(context, CullingResults);
            
            foreach (var pass in passList) pass.Execute();
            Scene.Flare.DrawAll(this);

            DrawUnsupportedShaders();
            
            DrawGizmosBeforeFX();
            postFXStack.Render();
            DrawGizmosAfterFX();
            
            Cleanup();
            
            Submit();
        }

        private void Setup()
        {
            Context.SetupCameraProperties(Camera);
            var clearFlags = Camera.clearFlags;
            
            postFXStack.Setup(Context, Camera, Buffer, Settings.postFXSettings, ref clearFlags);
            
            Buffer.ClearRenderTarget(
                clearFlags <= CameraClearFlags.Depth, 
                clearFlags == CameraClearFlags.Color, 
                clearFlags == CameraClearFlags.Color ? Camera.backgroundColor.linear : Color.clear);
            Buffer.BeginSample(BufferName);
            ExecuteBuffer();

            SetGlobals();
            
            foreach (var pass in passList) pass.Setup(this);

            Camera.depthTextureMode = DepthTextureMode.Depth;
        }

        private void SetGlobals()
        {
            WobbleAmount.Value = Settings.globalVertexWobbleAmount;
            ColorGradingVolume.SetWeights(this);
        }

        private bool Cull()
        {
            if (!Camera.TryGetCullingParameters(out var p)) return true;
            
            CullingResults = Context.Cull(ref p);
            return false;
        }

        private void Submit()
        {
            Buffer.EndSample(SampleName);
            ExecuteBuffer();
            Context.Submit();   
        }

        private void ExecuteBuffer()
        {
            Context.ExecuteCommandBuffer(Buffer);
            Buffer.Clear();
        }
        
        private void Cleanup()
        {
            foreach (var pass in passList) pass.Cleanup();
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