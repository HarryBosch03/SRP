using System;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Signals
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Signal")]
    public class Signal : ScriptableObject
    {
        public event System.EventHandler RaiseEvent;

        public void Raise(object caller, EventArgs args)
        {
            Debug.Log($"\"{name}\" Signal Raised.");
            RaiseEvent?.Invoke(caller, args);
        }
    }
}
