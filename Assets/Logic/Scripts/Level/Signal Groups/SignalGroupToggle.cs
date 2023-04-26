using BoschingMachine.Logic.Scripts.Interactables;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Signal_Groups
{
    public class SignalGroupToggle : Interactable
    {
        [Space]
        [SerializeField]
        private string displayText;

        private SignalGroup signalGroup;

        private void Awake()
        {
            signalGroup = SignalGroup.GetOrCreate(gameObject);
        }

        protected override void FinishInteract(Biped.Biped biped)
        {
            signalGroup.Call(!signalGroup.StateB);
        }

        public override string BuildInteractString(string passthrough = "")
        {
            return base.BuildInteractString(displayText);
        }
    }
}
