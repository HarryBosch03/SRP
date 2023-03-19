using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace BMRP.Runtime
{
    public abstract class ShaderProperty
    {
        public readonly int PropertyId;

        public ShaderProperty(string name)
        {
            PropertyId = Shader.PropertyToID(name);
        }

        public static CommandBuffer ActiveBuffer { get; set; }
        
        protected abstract void Send();

        public static void SendAll(params ShaderProperty[] properties)
        {
            foreach (var property in properties)
            {
                property.Send();
            }
        }
    }
    
    public abstract class ShaderProperty<T> : ShaderProperty
    {
        public T Value;

        protected abstract SetOperation Set { get; }

        protected delegate void SetOperation(int id, T val);
        
        protected ShaderProperty(string name, T value = default) : base(name)
        {
            Value = value;
        }

        public void Send(T val)
        {
            Value = val;
            Send();
        }
        
        protected override void Send()
        {
            Set(PropertyId, Value);
        }
    }

    public abstract class ShaderArray<T> : ShaderProperty<T[]>
    {
        public T this[int i]
        {
            get => Value[i];
            set => Value[i] = value;
        }
        protected ShaderArray(string name, int size) : base(name, new T[size]) { }
    }

    public class ShaderVectorArray : ShaderArray<Vector4>
    {
        public ShaderVectorArray(string name, int size) : base(name, size) { }
        protected override SetOperation Set => ActiveBuffer.SetGlobalVectorArray;
    }

    public class ShaderMatrixArray : ShaderArray<Matrix4x4>
    {
        public ShaderMatrixArray(string name, int size) : base(name, size) { }
        protected override SetOperation Set => ActiveBuffer.SetGlobalMatrixArray;
    }

    public class ShaderInt : ShaderProperty<int>
    {
        public ShaderInt(string name, int value = default) : base(name, value) { }
        protected override SetOperation Set => ActiveBuffer.SetGlobalInt;
    }
    
    public class ShaderVector : ShaderProperty<Vector4>
    {
        public ShaderVector(string name, Vector4 value = default) : base(name, value) { }
        protected override SetOperation Set => ActiveBuffer.SetGlobalVector;
    }
}