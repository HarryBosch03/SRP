using System.Linq;
using BoschingMachine.Logic.Scripts.Tags;
using BoschingMachine.Logic.Scripts.Utility;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Player
{
    [System.Serializable]
    public sealed class PlayerPickerUpper
    {
        [SerializeField] private float grabRange;
        [SerializeField] private float throwSpeed;
        [SerializeField] private float maxDistance;

        [Space]
        [SerializeField]
        private Spring spring;

        [Space]
        [SerializeField]
        private float rotationDamper;

        [Space]
        [SerializeField]
        private LineRenderer lines;
        [SerializeField] private float lineVolume;
        [SerializeField] private PlayerPickerUpperUI ui;
        [SerializeField] private Tag ignoreTag;

        public Rigidbody HeldObject { get; private set; }
        public Rigidbody LookingAt { get; private set; }

        public void FixedProcess(Biped.Biped biped, Transform holdTarget)
        {
            TryGetLookingAt(biped);

            if (!HeldObject) return;

            var vec = holdTarget.position - HeldObject.position;

            if (vec.sqrMagnitude > maxDistance * maxDistance)
            {
                HeldObject = null;
                return;
            }

            spring.Drive(HeldObject, biped.Rigidbody, holdTarget.position);

            HeldObject.AddTorque(-HeldObject.angularVelocity * rotationDamper);
        }

        public void Update(Transform holdTarget)
        {
            if (HeldObject)
            {
                lines.enabled = true;

                var segments = 16;
                lines.positionCount = segments;
                for (var i = 0; i < segments; i++)
                {
                    var p = i / (segments - 1.0f);
                    lines.SetPosition(i, Vector3.Lerp(HeldObject.position, holdTarget.position, p));
                }

                var distance = (HeldObject.position - holdTarget.position).magnitude;
                var factor = Mathf.Sqrt(lineVolume / distance);
                if (factor > 1.0f) factor = 1.0f;

                lines.widthCurve.MoveKey(1, new Keyframe(0.5f, factor));
            }
            else
            {
                lines.enabled = false;
            }

            ui.Update(this);
        }

        public bool TryGrabOrDrop(Biped.Biped biped)
        {
            if (HeldObject)
            {
                HeldObject = null;
                return true;
            }
            else if (LookingAt)
            {
                HeldObject = LookingAt;
                return true;
            }

            return false;
        }

        public bool Throw(Biped.Biped biped)
        {
            if (!HeldObject) return false;

            var throwForce = biped.Head.forward * throwSpeed * HeldObject.mass;
            throwForce = Vector3.ClampMagnitude(throwForce, spring.maxForce);

            HeldObject.AddForce(throwForce, ForceMode.Impulse);
            HeldObject = null;

            return true;
        }

        public void TryGetLookingAt(Biped.Biped biped)
        {
            LookingAt = null;

            var ray = new Ray(biped.Head.position, biped.Head.forward);

            var results = Physics.RaycastAll(ray, grabRange).OrderBy(a => a.distance).Where(a => !a.transform.HasTag(ignoreTag));

            if (results.Count() == 0) return;

            var hit = results.First();

            if (!hit.rigidbody) return;
            if (hit.rigidbody.isKinematic) return;

            LookingAt = hit.rigidbody;
            return;
        }
    }
}
