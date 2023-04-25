using System.Threading;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace BMRP.Runtime.Core.Passes
{
    public abstract class RenderPass
    {
        private CustomSampler sampler;
        
        protected CameraRenderer Renderer { get; private set; }
        public CommandBuffer Buffer { get; private set; }

        public virtual void Setup(CameraRenderer renderer)
        {
            Renderer = renderer;
            
            sampler = CustomSampler.Create(GetType().Name, true);
            Buffer = new CommandBuffer();
            Buffer.name = GetType().Name;
            Buffer.BeginSample(sampler);
        }

        public void Execute()
        {
            OnExecute();
            
            Buffer.EndSample(sampler);
            Renderer.Context.ExecuteCommandBuffer(Buffer);
            Buffer.Clear();
        }

        protected abstract void OnExecute();

        public virtual void Cleanup()
        {
            
        }
    }
}