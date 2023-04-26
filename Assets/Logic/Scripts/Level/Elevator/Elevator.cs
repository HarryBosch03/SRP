using System.Collections;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Elevator
{
    public class Elevator : MonoBehaviour
    {
        [SerializeField] private float smoothTime;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float threshold = 0.1f;
        [SerializeField] private int startingFloor;
        [SerializeField] private float waitTime;

        [Space]
        [SerializeField]
        private ElevatorDoor doors;

        private new Rigidbody rigidbody;

        private ElevatorGroup group;

        private float elevatorTarget;
        private float elevatorPosition;
        private float elevatorVelocity;

        private bool openDoors;
        private bool closeDoors;

        private bool[] internalRequests;

        public bool[] Requests { get; private set; }
        public ElevatorState State { get; private set; }

        public int TargetFloor { get; private set; }
        public int CurrentFloor { get; private set; }
        public int MoveDirection { get; private set; }
        public ElevatorDoor Doors => doors;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            group = GetComponentInParent<ElevatorGroup>();

            internalRequests = new bool[group.Floors.Length];
            Requests = new bool[group.Floors.Length];

            State = ElevatorState.Idle;
        }

        private void OnEnable()
        {
            group.Elevators.Add(this);
        }

        private void OnDisable()
        {
            group.Elevators.Remove(this);
        }

        private void Start()
        {
            transform.position = HeightToPoint(group.Floors[startingFloor]);
            TargetFloor = startingFloor;
        }

        private void Update()
        {
            CurrentFloor = GetCurrentFloor();

            if (State != ElevatorState.Idle) return;
            if (openDoors)
            {
                StartCoroutine(WaitAtFloor());
                return;
            }
            if (!HasRequests()) return;

            TargetFloor = FindTargetFloor();

            StartCoroutine(Move());
        }

        private IEnumerator Move()
        {
            State = ElevatorState.Moving;
            MoveDirection = (int)Mathf.Sign(TargetFloor - CurrentFloor);

            bool moving;
            do
            {
                TargetFloor = FindTargetFloor(MoveDirection);
                elevatorTarget = group.Floors[TargetFloor];

                var distance = Mathf.Abs(rigidbody.position.y - elevatorTarget);
                moving = distance > threshold;

                yield return null;
            }
            while (moving);

            yield return StartCoroutine(WaitAtFloor());

            if (HasRequests(MoveDirection))
            {
                TargetFloor = FindTargetFloor(MoveDirection);
                StartCoroutine(Move());
                yield break;
            }

            State = ElevatorState.Idle;
        }

        private IEnumerator WaitAtFloor()
        {
            State = ElevatorState.Boarding;
            internalRequests[TargetFloor] = false;

            var timer = 0.0f;
            while (timer < waitTime || !doors.DoorsClosed)
            {
                doors.IsDoorOpen = timer < waitTime;
                if (Requests[CurrentFloor] || openDoors)
                {
                    internalRequests[CurrentFloor] = false;
                    openDoors = false;
                    timer = 0.0f;
                }
                if (closeDoors)
                {
                    timer = waitTime;
                    closeDoors = false;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            State = ElevatorState.Idle;
        }

        private void FixedUpdate()
        {
            elevatorPosition = Mathf.SmoothDamp(elevatorPosition, elevatorTarget, ref elevatorVelocity, smoothTime, maxSpeed);
            rigidbody.MovePosition(HeightToPoint(elevatorPosition));

            doors.FixedUpdate();
        }

        private bool HasRequests(int direction = 0)
        {
            var i = 0;
            foreach (var request in Requests)
            {
                if (request)
                {
                    if ((i - CurrentFloor) * direction >= 0) return true;
                }
                i++;
            }
            return false;
        }

        private int FindTargetFloor(int direction) => FindTargetFloor(direction >= 0, direction <= 0);
        private int FindTargetFloor(bool searchForward = true, bool searchBack = true)
        {
            var floors = group.Floors.Length;
            var requestExists = false;
            for (var i = 0; i < floors; i++)
            {
                if (Requests[i])
                {
                    requestExists = true;
                    break;
                }
            }

            if (!requestExists) return CurrentFloor;

            var fForward = -1;
            var fBack = -1;

            for (var i = CurrentFloor; i < floors && searchForward; i++)
            {
                if (Requests[i])
                {
                    fForward = i;
                    break;
                }
            }

            for (var i = CurrentFloor; i >= 0 && searchBack; i--)
            {
                if (Requests[i])
                {
                    fBack = i;
                    break;
                }
            }

            if (fBack == -1 && fForward == -1) return CurrentFloor;

            if (fBack == -1) return fForward;
            if (fForward == -1) return fBack;

            if (MoveDirection > 0) return fForward;
            if (MoveDirection < 0) return fBack;

            return fForward - CurrentFloor < CurrentFloor - fBack ? fForward : fBack;
        }

        public void ResetRequests()
        {
            for (var i = 0; i < Requests.Length; i++)
            {
                Requests[i] = internalRequests[i];
            }
        }

        public void Request(int i)
        {
            internalRequests[i] = true;
        }

        public Vector3 HeightToPoint(float h) => new Vector3(transform.position.x, h, transform.position.z);

        public int GetCurrentFloor()
        {
            var best = 0;
            for (var i = 0; i < group.Floors.Length; i++)
            {
                var d1 = Mathf.Abs(group.Floors[i] - rigidbody.position.y);
                var d2 = Mathf.Abs(group.Floors[best] - rigidbody.position.y);

                if (d1 < d2) best = i;
            }

            return best;
        }

        public static string FormatIndex(int i) => i == 0 ? "G" : $"L{i}";

        public bool HasFloorBeenRequested(int floor) => Requests[floor];

        public bool IsFloorOnCurrentPath (int f)
        {
            if (State != ElevatorState.Moving) return false;

            f -= CurrentFloor;
            var tf = TargetFloor - CurrentFloor;

            if (tf * f < 0) return false;
            return Mathf.Abs(f) < Mathf.Abs(tf);
        }

        public void OpenDoors() => openDoors = true;
        public void CloseDoors() => closeDoors = true;

        public enum ElevatorState
        {
            Moving,
            Boarding,
            Idle
        }
    }
}
