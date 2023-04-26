using BoschingMachine.Logic.Scripts.Interactables;
using UnityEngine;
using UnityEngine.Events;

namespace BoschingMachine.Logic.Scripts.Level.Elevator
{
    public class ElevatorCallSwitch : Interactable
    {
        [SerializeField] private UnityEvent<string> setupCallback;

        private ElevatorGroup elevatorGroup;
        private Elevator elevator;
        private int index;

        public override bool CanInteract => elevator ? !elevator.HasFloorBeenRequested(index) : true;
        public int? IndexOverride { get; set; } = null;

        private void Start()
        {
            elevator = GetComponentInParent<Elevator>();
            elevatorGroup = GetComponentInParent<ElevatorGroup>();
            index = GetButtonCallIndex();
            setupCallback.Invoke(Elevator.FormatIndex(index));
        }

        private void Update()
        {
            transform.localScale = new Vector3(1.0f, 1.0f, CanInteract ? 1.0f : 0.5f);
        }

        public int GetButtonCallIndex()
        {
            if (IndexOverride.HasValue) return IndexOverride.Value;

            if (!elevatorGroup) return 0;

            var best = 0;
            for (var i = 1; i < elevatorGroup.Floors.Length; i++)
            {
                var d1 = Mathf.Abs(elevatorGroup.Floors[i] - transform.position.y);
                var d2 = Mathf.Abs(elevatorGroup.Floors[best] - transform.position.y);

                if (d1 < d2) best = i;
            }

            return best;
        }

        protected override string InoperableAppend => "En Route";
        public override string BuildInteractString(string passthrough = "")
        {
            return CanInteract ? base.BuildInteractString("Call Elevator") : string.Empty;
        }

        protected override void FinishInteract(Biped.Biped biped)
        {
            if (elevator) elevator.Request(index);
            else elevatorGroup.Request(index);
        }
    }
}
