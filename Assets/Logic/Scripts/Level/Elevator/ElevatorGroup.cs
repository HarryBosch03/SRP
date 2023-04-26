using System.Collections.Generic;
using System.Linq;
using BoschingMachine.Logic.Scripts.Utility;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Elevator
{
    public class ElevatorGroup : MonoBehaviour
    {
        [SerializeField] private float[] floors;

        private bool[] requests;
        public float[] Floors => floors;

        public List<Elevator> Elevators { get; } = new();

        private void Awake()
        {
            requests = new bool[floors.Length];
        }

        private void Update()
        {
            foreach (var elevator in Elevators)
            {
                elevator.ResetRequests();
            }

            void DoRequest(int i)
            {
                if (!requests[i]) return;

                foreach (var elevator in Elevators)
                {
                    if (elevator.CurrentFloor == i && elevator.State == Elevator.ElevatorState.Boarding)
                    {
                        requests[i] = false;
                        return;
                    }
                    if (elevator.TargetFloor == i && elevator.State != Elevator.ElevatorState.Idle)
                    {
                        elevator.Requests[i] = true;
                        return;
                    }
                }

                var best = Elevators.Where(e => e.CurrentFloor == i && e.State != Elevator.ElevatorState.Moving).Lowest(e => Mathf.Abs(i - e.CurrentFloor));
                if (!best) best = Elevators.Where(e => e.IsFloorOnCurrentPath(i)).Lowest(e => Mathf.Abs(i - e.CurrentFloor));
                if (!best) best = Elevators.Where(e => e.State == Elevator.ElevatorState.Idle).Lowest(e => Mathf.Abs(i - e.CurrentFloor));
                if (!best) return;

                best.Requests[i] = true;
            }

            for (var i = 0; i < requests.Length; i++)
            {
                DoRequest(i);
            }
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (UnityEditor.Selection.Contains(gameObject)) return;
#endif
            DrawGizmos(Gizmos.DrawWireCube, 0.2f);
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmos(Gizmos.DrawWireCube);
            DrawGizmos(Gizmos.DrawCube, 0.2f);
        }

        private void DrawGizmos(System.Action<Vector3, Vector3> drawAction, float alpha = 1.0f)
        {
            Gizmos.color = new Color(1.0f, 0.7529411764705882f, 0.0705882352941176f, alpha);

            var elevators = GetComponentsInChildren<Elevator>();
            var bounds = new Bounds(elevators[0].transform.position, Vector3.zero);
            foreach (var elevator in elevators)
            {
                bounds.Encapsulate(elevator.transform.position);
            }

            if (floors == null) return;
            foreach (var floor in floors)
            {
                drawAction(new Vector3(bounds.center.x, floor, bounds.center.z), new Vector3(3.0f + bounds.size.x, 0, 3.0f + bounds.size.z));
            }

            Gizmos.DrawLine(
                new Vector3(bounds.center.x, Floors[0], bounds.center.z),
                new Vector3(bounds.center.x, Floors[Floors.Length - 1], bounds.center.z));
        }

        public void Request(int i)
        {
            requests[i] = true;
        }

        public Vector3 HeightToPoint(float h) => new Vector3(transform.position.x, h, transform.position.z);

        public enum ElevatorDirection
        {
            Up,
            Down,
            Waiting,
        }
    }
}
