using BoschingMachine.Logic.Scripts.Interactables;
using BoschingMachine.Logic.Scripts.Tags;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level
{
    public class Electromagnet : Interactable
    {
        [SerializeField] private Vector3 poleMinOffset;
        [SerializeField] private Vector3 poleMaxOffset;
        [SerializeField] private float constant;
        [SerializeField] private float minForce;
        [SerializeField] private float maxForce;
        [SerializeField] private bool active;
        [SerializeField] private Tag magneticTag;

        private float Radius => Mathf.Sqrt(constant / minForce);
        private Vector3 PoleMin => transform.TransformPoint(poleMinOffset);
        private Vector3 PoleMax => transform.TransformPoint(poleMaxOffset);

        protected override void FinishInteract(Biped.Biped biped)
        {
            active = !active;
        }

        public override string BuildInteractString(string passthrough = "")
        {
            return active ? "Deactivate Electromagnet" : "Activate Electromagnet";
        }

        private void FixedUpdate()
        {
            if (!active) return;

            var bodies = Tag.GetHoldersForTag(magneticTag);
            foreach (var body in bodies)
            {
                if (!body.TryGetComponent(out Rigidbody rigidbody)) return;

                var point = GetPoint(rigidbody);
                var vec = point - rigidbody.position;
                var sqrDist = vec.sqrMagnitude;
                var dir = vec.normalized;

                rigidbody.AddForce(dir * (constant / sqrDist));
            }
        }

        private Vector3 GetPoint(Rigidbody rigidbody)
        {
            var dist = (PoleMax - PoleMin).magnitude;
            var dir = (PoleMax - PoleMin).normalized;
            var dot = Vector3.Dot(rigidbody.position - PoleMin, dir);
            dot = Mathf.Clamp(dot, 0.0f, dist);

            return PoleMin + dir * dot;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(PoleMax, 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(PoleMin, 0.1f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere((PoleMax + PoleMin) * 0.5f, Radius);
        }

        private void OnValidate()
        {
            minForce = Mathf.Max(minForce, 0.001f);
        }
    }
}
