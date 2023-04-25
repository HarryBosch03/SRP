using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime.PostFX
{
    public partial class PostFXStack
    {
        private const string BufferName = "Post FX";

        private ScriptableRenderContext context;
        private PostFXSettings settings;
        private CommandBuffer outerBuffer;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public CommandBuffer Buffer { get; } = new()
        {
            name = BufferName,
        };

        public Camera Camera { get; set; }
        public bool IsActive => settings;

        public static readonly int
            FXBuffer1 = Shader.PropertyToID("_FXBuffer1"),
            FXBuffer2 = Shader.PropertyToID("_FXBuffer2"),
            DepthAttach = Shader.PropertyToID("_CameraDepthAttachment");

        public int SourceBuffer { get; private set; }
        public int DestBuffer { get; private set; }

        public void Setup(ScriptableRenderContext context, Camera camera, CommandBuffer outerBuffer,
            PostFXSettings settings, ref CameraClearFlags clearFlags)
        {
            this.context = context;
            Camera = camera;
            this.settings = camera.cameraType <= CameraType.SceneView ? settings : null;
            this.outerBuffer = outerBuffer;

            ApplySceneViewState();

            if (!IsActive) return;

            SourceBuffer = FXBuffer1;
            DestBuffer = FXBuffer2;

            Width = camera.pixelWidth;
            Height = camera.pixelHeight;

            outerBuffer.GetTemporaryRT(SourceBuffer, Width, Height, 32, FilterMode.Point,
                RenderTextureFormat.DefaultHDR);
            outerBuffer.GetTemporaryRT(DestBuffer, Width, Height, 32, FilterMode.Point,
                RenderTextureFormat.DefaultHDR);
            outerBuffer.GetTemporaryRT(DepthAttach, Width, Height, 32, FilterMode.Point, RenderTextureFormat.Depth);

            outerBuffer.SetRenderTarget(
                SourceBuffer, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                DepthAttach, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

            if (clearFlags > CameraClearFlags.Color)
            {
                clearFlags = CameraClearFlags.Color;
            }

            foreach (var effect in settings.Effects)
            {
                if (!effect) continue;
                if (!effect.Enabled) continue;
                effect.Setup(this);
            }
        }

        public void Render()
        {
            if (!IsActive) return;

            foreach (var effect in settings.Effects)
            {
                if (!effect) continue;
                if (!effect.Enabled) continue;

                effect.Apply(this);
                SwapBuffers();
            }

            Buffer.Blit(SourceBuffer, BuiltinRenderTextureType.CameraTarget);

            context.ExecuteCommandBuffer(Buffer);
            Buffer.Clear();
        }

        public void Cleanup()
        {
            if (!IsActive) return;

            outerBuffer.ReleaseTemporaryRT(FXBuffer1);
            outerBuffer.ReleaseTemporaryRT(FXBuffer2);
            outerBuffer.ReleaseTemporaryRT(DepthAttach);

            foreach (var effect in settings.Effects)
            {
                if (!effect) continue;
                if (!effect.Enabled) continue;
                effect.Cleanup(this);
            }
        }

        public void SwapBuffers()
        {
            var s = SourceBuffer;
            var t = DestBuffer;

            SourceBuffer = t;
            DestBuffer = s;

            Buffer.SetRenderTarget(DestBuffer, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        }
    }
}