using System.Collections.Generic;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class DeadBody : MonoBehaviour
    {
        [SerializeField] private Transform bodyRoot;

        private List<Rigidbody> segments;

        private void Awake()
        {
            segments = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
        }

        private void FixedUpdate()
        {
            var translation = Vector3.zero;

            foreach (var segment in segments)
            {
                var vector = segment.transform.position - transform.position;
                translation += vector;
            }

            translation /= segments.Count;

            transform.position += translation;
            bodyRoot.position -= translation;
        }
    }
}
