using BoschingMachine.Logic.Scripts.Interactables;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Signal_Groups
{
    public class SignalGroupButton : Interactable
    {
        [SerializeField] protected int index;
        [SerializeField] protected string text = "Press Button";

        private SignalGroup signalGroup;

        private void Awake()
        {
            signalGroup = SignalGroup.GetOrCreate(gameObject);
        }

        protected override void FinishInteract(Biped.Biped biped)
        {
            Call();
        }

        public virtual void Call()
        {
            signalGroup.Call(index);
        }

        public override string BuildInteractString(string passthrough = "")
        {
            return base.BuildInteractString(text);
        }
    }
}
