using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Elevator
{
    [System.Serializable]
    public class ElevatorDoor
    {
        [SerializeField] private Transform[] doors;
        [SerializeField] private float doorOpenDistance;
        [SerializeField] private float doorOpenTime;

        private float doorPos;
        private float doorVel;

        public bool IsDoorOpen { get; set; }
        public bool DoorsClosed => doorPos < 0.01f;

        public void FixedUpdate()
        {
            doorPos = Mathf.SmoothDamp(doorPos, IsDoorOpen ? 1.0f : 0.0f, ref doorVel, doorOpenTime);
            foreach (var door in doors)
            {
                door.localPosition = door.transform.localRotation * Vector3.right * doorPos * doorOpenDistance;
            }
        }
    }
}
