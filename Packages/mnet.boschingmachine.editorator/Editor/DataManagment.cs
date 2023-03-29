using System.Collections.Generic;

namespace Editorator.Editor
{
    public static partial class E
    {
        private class Data<T>
        {
            private readonly Dictionary<string, T> data = new();

            private bool Fallback(string reference, T fallback)
            {
                if (data.ContainsKey(reference)) return true;
            
                data.Add(reference, fallback);
                return false;
            }

            public T Read(string reference, T fallback)
            {
                Fallback(reference, fallback);
                return data[reference];
            }

            public Data<T> Write(string reference, T value)
            {
                if (!Fallback(reference, value)) return this;

                data[reference] = value;
                return this;
            } 
        }
    }
}