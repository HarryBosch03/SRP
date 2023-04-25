using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime.Core
{
    public abstract class ShaderProperty
    {
        public static CommandBuffer ActiveBuffer { get; private set; }

        public static void SendAll(CommandBuffer buffer, params ShaderProperty[] properties)
        {
            using var c = new BufferContext(buffer);
            foreach (var property in properties)
            {
                property.Send();
            }
        }

        public abstract void Send();

        public class BufferContext : IDisposable
        {
            public BufferContext(CommandBuffer buffer)
            {
                ActiveBuffer = buffer;
            }
            
            public void Dispose()
            {
                ActiveBuffer = null;
            }
        }
    }

    public class ShaderProperty<T> : ShaderProperty
    {
        public readonly int Handle;
        
        private readonly Action<CommandBuffer, ShaderProperty<T>> setCallback;
        public T Value { get; set; }
        public T ActualValue { get; private set; }

        public ShaderProperty(string reference, Action<CommandBuffer, ShaderProperty<T>> setCallback)
        {
            Handle = Shader.PropertyToID(reference);
            this.setCallback = setCallback;
        }

        public ShaderProperty<T> Set(T value)
        {
            Value = value;
            Send();
            return this;
        }
        
        public override void Send()
        {
            if (ActiveBuffer == null) throw new NullReferenceException("Active buffer was not set!");
                setCallback(ActiveBuffer, this);
            ActualValue = Value;
        }
    }

    public static class ShaderPropertyFactory
    {
        public static ShaderProperty<int> Int(string reference) => new(reference, (b, p) => b.SetGlobalInt(p.Handle, p.Value));
        public static ShaderProperty<float> Float(string reference) => new(reference, (b, p) => b.SetGlobalFloat(p.Handle, p.Value));
        public static ShaderProperty<Vector4> Vec(string reference) => new(reference, (b, p) => b.SetGlobalVector(p.Handle, p.Value));
        public static ShaderProperty<Vector4[]> VecArray(string reference, int capacity)
        {
            var b = new ShaderProperty<Vector4[]>(reference, (b, p) => b.SetGlobalVectorArray(p.Handle, p.Value));
            b.Value = new Vector4[capacity];
            return b;
        }
    }
}