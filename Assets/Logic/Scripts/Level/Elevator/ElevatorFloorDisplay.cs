using TMPro;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level.Elevator
{
    public class ElevatorFloorDisplay : MonoBehaviour
    {
        private TMP_Text text;
        private Elevator elevator;

        private void Start()
        {
            elevator = GetElevator();
            text = GetComponentInChildren<TMP_Text>();
        }

        private void Update()
        {
            text.text = Elevator.FormatIndex(elevator.CurrentFloor);
        }

        private Elevator GetElevator()
        {
            void Throw ()
            {
                enabled = false;
                Debug.LogError("Elevator Floor Display cannot Find Elevator", this);
            }

            float PlanarLength(Vector3 v) => new Vector2(v.x, v.z).magnitude;

            var elevator = GetComponentInParent<Elevator>();
            if (elevator) return elevator;

            var group = GetComponentInParent<ElevatorGroup>();
            if (!group)
            {
                Throw();
                return null;
            }

            var elevators = group.GetComponentsInChildren<Elevator>();
            if (elevators.Length == 0)
            {
                Throw();
                return null;
            }
            var best = elevators[0];
            foreach (var other in elevators)
            {
                var d1 = PlanarLength(other.transform.position - transform.position);
                var d2 = PlanarLength(best.transform.position - transform.position);

                if (d1 < d2)
                {
                    best = other;
                }
            }
            return best;
        }
    }
}
