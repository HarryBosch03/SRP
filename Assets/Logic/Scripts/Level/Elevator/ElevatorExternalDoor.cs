using BoschingMachine.Logic.Scripts.Utility;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Elevator
{
    public class ElevatorExternalDoor : MonoBehaviour
    {
        [SerializeField] private ElevatorDoor doors;

        private ElevatorGroup group;
        private Elevator elevator;
        private int floor;

        private void Awake()
        {
            group = GetComponentInParent<ElevatorGroup>();
        }

        private void Start()
        {
            Vector2 ToPlanar(Vector3 v) => new Vector2(v.x, v.z);
            elevator = group.Elevators.Lowest(e => ToPlanar(e.transform.position - transform.position).magnitude);
            group.Floors.Lowest(e => Mathf.Abs(e - transform.position.y), out floor);
        }

        private void Update()
        {
            if (elevator.CurrentFloor == floor)
            {
                doors.IsDoorOpen = elevator.Doors.IsDoorOpen;
            }
            else doors.IsDoorOpen = false;
        }

        private void FixedUpdate() => doors.FixedUpdate();
        
    }
}
