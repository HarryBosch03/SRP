using System.Collections.Generic;
using BoschingMachine.Logic.Scripts.Interactables;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Elevator
{
    public class ElevatorExtraSwitch : Interactable
    {
        [SerializeField] private SwitchType type;

        private Elevator elevator;

        public static readonly Dictionary<SwitchType, string> Labels = new()
        {
            { SwitchType.CloseDoors, "Close Doors" },
            { SwitchType.OpenDoors, "Open Doors" },
            { SwitchType.CallHelp, "Call For Help" },
        };

        private void Awake()
        {
            elevator = GetComponentInParent<Elevator>();
        }

        protected override void FinishInteract(Biped.Biped biped)
        {
            switch (type)
            {
                case SwitchType.CloseDoors:
                    elevator.CloseDoors();
                    break;
                case SwitchType.OpenDoors:
                    elevator.OpenDoors();
                    break;
                case SwitchType.CallHelp:
                    System.Diagnostics.Process.Start("https://www.wikihow.com/Survive-Being-Stuck-in-a-Lift");
                    break;
            }
        }

        public override string BuildInteractString(string passthrough = "")
        {
            return Labels[type];
        }

        public enum SwitchType
        {
            CloseDoors,
            OpenDoors,
            CallHelp,
        }
    }
}
