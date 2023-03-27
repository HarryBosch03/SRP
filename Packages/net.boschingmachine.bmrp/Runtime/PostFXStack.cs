using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public partial class PostFXStack
    {
        private const string BufferName = "Post FX";

        private ScriptableRenderContext context;
        private PostFXSettings settings;
        private int width;
        private int height;
        private CommandBuffer outerBuffer;

        public CommandBuffer Buffer { get; } = new()
        {
            name = BufferName,
        };

        public Camera Camera { get; set; }
        public bool IsActive => settings;

        private readonly int
            fxBuffer1 = Shader.PropertyToID("_FXBuffer1"),
            fxBuffer2 = Shader.PropertyToID("_FXBuffer2");

        public int SourceBuffer { get; private set; }
        public int TargetBuffer { get; private set; }

        public void Setup(ScriptableRenderContext context, Camera camera, CommandBuffer outerBuffer, PostFXSettings settings, ref CameraClearFlags clearFlags)
        {
            this.context = context;
            Camera = camera;
            this.settings = camera.cameraType <= CameraType.SceneView ? settings : null;
            this.outerBuffer = outerBuffer;

            ApplySceneViewState();

            if (!IsActive) return;

            SourceBuffer = fxBuffer1;
            TargetBuffer = fxBuffer2;

            var downscale = Mathf.Max(1.0f, (float)camera.pixelHeight / settings.TargetVerticalResolution);
            
            width = (int)(camera.pixelWidth / downscale);
            height = (int)(camera.pixelHeight / downscale);

            outerBuffer.GetTemporaryRT(SourceBuffer, width, height, 32, FilterMode.Point,
                RenderTextureFormat.DefaultHDR);
            outerBuffer.GetTemporaryRT(TargetBuffer, width, height, 32, FilterMode.Point,
                RenderTextureFormat.DefaultHDR);

            outerBuffer.SetRenderTarget(SourceBuffer, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

            if (clearFlags > CameraClearFlags.Color)
            {
                clearFlags = CameraClearFlags.Color;
            }
        }

        public void Render()
        {
            if (!IsActive) return;

            foreach (var effect in settings.Effects)
            {
                if (!effect) continue;
            
                Buffer.Blit(SourceBuffer, TargetBuffer, effect);
                SwapBuffers();
            }

            Buffer.Blit(SourceBuffer, BuiltinRenderTextureType.CameraTarget);

            context.ExecuteCommandBuffer(Buffer);
            Buffer.Clear();
        }

        public void Cleanup()
        {
            if (!IsActive) return;

            outerBuffer.ReleaseTemporaryRT(fxBuffer1);
            outerBuffer.ReleaseTemporaryRT(fxBuffer2);
        }

        public void SwapBuffers()
        {
            var s = SourceBuffer;
            var t = TargetBuffer;

            SourceBuffer = t;
            TargetBuffer = s;

            Buffer.SetRenderTarget(TargetBuffer, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        }
    }
}