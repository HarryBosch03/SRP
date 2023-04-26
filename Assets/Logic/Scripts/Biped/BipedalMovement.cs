using System.Collections.Generic;
using BoschingMachine.Logic.Scripts.Meta;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Biped
{
    [System.Serializable]
    public sealed class BipedalMovement
    {
        [SerializeField] private float moveSpeed = 10.0f;
        [SerializeField] private float accelerationTime = 0.06f;
        [SerializeField] private float decelerationTime = 0.06f;

        [Space]
        [SerializeField][Percent]
        private float airAccelerationPenalty = 0.2f;

        [Space]
        [SerializeField]
        private float jumpHeight = 2.5f;
        [SerializeField] private float upGravity = 3.0f;
        [SerializeField] private float downGravity = 4.0f;
        [SerializeField] private float jumpSpringPauseTime = 0.1f;

        [Space]
        [SerializeField]
        private float springDistance = 1.2f;
        [SerializeField] private float springForce = 500.0f;
        [SerializeField] private float springDamper = 25.0f;
        [SerializeField] private float groundCheckRadius = 0.4f;
        [SerializeField] private float maxWalkableSlope = 35.0f;
        [SerializeField] private LayerMask groundCheckMask = 0b1;

        private bool previousJumpState;
        private float lastJumpTime;
        private Vector3 lastGroundPosition;
        private Vector3 lastGroundVelocity;

        public float MaxMoveSpeed => moveSpeed;
        public float JumpForce => Mathf.Sqrt(2.0f * -Physics.gravity.y * upGravity * jumpHeight);
        
        public float DistanceToGround { get; private set; }
        public Vector3 GroundNormal { get; set; }
        public bool IsGrounded => DistanceToGround < 0.0f && GroundAngle < maxWalkableSlope;
        public float GroundAngle => GetGroundAngle(GroundNormal);
        public Vector3 GroundVelocity
        {
            get
            {   
                if (IsGrounded && GroundRigidbody) return GroundRigidbody.GetPointVelocity(lastGroundPosition);
                return Vector3.zero;
            }
        }

        public GameObject Ground { get; private set; }
        public Rigidbody GroundRigidbody { get; private set; }

        public Vector3 RelativeVelocity(Rigidbody rigidbody) => rigidbody.velocity - GroundVelocity;
        public Vector2 MoveSpeed(Rigidbody rigidbody) => new Vector2(rigidbody.velocity.x, rigidbody.velocity.z);
        public Vector2 LocalMoveSpeed(Rigidbody rigidbody) => MoveSpeed(rigidbody) - new Vector2(GroundVelocity.x, GroundVelocity.z);

        public void Move(Rigidbody rigidbody, Vector3 moveDirection, bool jump)
        {
            DistanceToGround = GetDistanceToGround(rigidbody) - springDistance;

            MoveCharacter(rigidbody, moveDirection);

            if (jump && !previousJumpState)
            {
                TryJump(rigidbody);
            }
            previousJumpState = jump;

            ApplySpring(rigidbody);
            ApplyGravity(rigidbody, jump);
            ApplyGroundForces(rigidbody);

            lastGroundVelocity = GroundVelocity;
        }

        private void ApplyGroundForces(Rigidbody rigidbody)
        {
            if (!IsGrounded) return;
            if (!GroundRigidbody) return;

            var force = GroundVelocity - lastGroundVelocity;
            rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        private void ApplySpring(Rigidbody rigidbody)
        {
            if (IsGrounded && Time.time > lastJumpTime + jumpSpringPauseTime)
            {
                var contraction = 1.0f - (DistanceToGround + springDistance) / springDistance;
                var moment = Vector3.up * contraction * springForce;
                moment -= Vector3.up * RelativeVelocity(rigidbody).y * springDamper;

                AddMomentToSelfAndGround(rigidbody, moment);
            }
        }

        private void ApplyGravity(Rigidbody rigidbody, bool jump)
        {
            rigidbody.useGravity = false;
            rigidbody.AddForce(GetGravity(rigidbody, jump), ForceMode.Acceleration);
        }

        private void MoveCharacter(Rigidbody rigidbody, Vector3 moveDirection)
        {
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1.0f);

            var target = moveDirection * moveSpeed + GroundVelocity;
            var current = rigidbody.velocity;

            var delta = Vector3.ClampMagnitude(target - current, moveSpeed);
            delta.y = 0.0f;

            var acceleration = moveSpeed / (target.sqrMagnitude > current.sqrMagnitude ? accelerationTime : decelerationTime);

            var moment = delta / moveSpeed * acceleration;

            if (!IsGrounded) moment *= airAccelerationPenalty;

            AddMomentToSelfAndGround(rigidbody, moment);
        }

        private void TryJump(Rigidbody rigidbody)
        {
            if (IsGrounded)
            {
                if (rigidbody.velocity.y < 0.0f) rigidbody.AddForce(Vector3.up * -rigidbody.velocity.y, ForceMode.VelocityChange);

                AddMomentToSelfAndGround(rigidbody, Vector3.up * JumpForce, ForceMode.Impulse);

                lastJumpTime = Time.time;
            }
        }

        private Vector3 GetGravity(Rigidbody rigidbody, bool jump)
        {
            var scale = upGravity;
            if (!jump)
            {
                scale = downGravity;
            }
            else if (rigidbody.velocity.y < 0.0f)
            {
                scale = downGravity;
            }

            return Physics.gravity * scale;
        }

        public void AddMomentToSelfAndGround(Rigidbody self, Vector3 moment, ForceMode forceMode = ForceMode.Force)
        {
            var force = moment * self.mass;
            self.AddForce(force, forceMode);
            if (GroundRigidbody && IsGrounded && false)
            {
                GroundRigidbody.AddForce(-force, forceMode);
            }
        }

        public float GetDistanceToGround(Rigidbody rigidbody)
        {
            var hits = new List<RaycastHit>(Physics.SphereCastAll(rigidbody.position + Vector3.up * groundCheckRadius, groundCheckRadius, Vector3.down, springDistance, groundCheckMask, QueryTriggerInteraction.Ignore));

            RaycastHit? hit = null;
            foreach (var other in hits)
            {
                if (other.rigidbody == rigidbody) continue;
                if (other.distance > springDistance) continue;
                if (GetGroundAngle(other.normal) > maxWalkableSlope) continue;

                if (hit.HasValue) hit = GetBetterHit(hit.Value, other);
                else hit = other;
            }

            if (hit.HasValue)
            {
                Ground = hit.Value.transform.gameObject;
                GroundRigidbody = hit.Value.rigidbody;
                GroundNormal = hit.Value.normal;
                lastGroundPosition = hit.Value.point;
                return hit.Value.distance;
            }
            else
            {
                Ground = null;
                GroundRigidbody = null;
                GroundNormal = Vector3.zero;
                lastGroundPosition = Vector3.zero;
                return float.PositiveInfinity;
            }
        }

        public RaycastHit GetBetterHit(RaycastHit a, RaycastHit b)
        {
            var close = a.distance < b.distance ? a : b;

            bool Static(RaycastHit h) => h.rigidbody ? h.rigidbody.isKinematic : true;

            if (Static(a) && Static(b)) return close;

            if (Static(a)) return a;
            if (Static(b)) return b;

            if (a.rigidbody.mass == b.rigidbody.mass) return close;
            else return a.rigidbody.mass > b.rigidbody.mass ? a : b;
        }

        private float GetGroundAngle(Vector3 groundNormal)
        {
            return Mathf.Acos(Mathf.Clamp01(Vector3.Dot(Vector3.up, groundNormal))) * Mathf.Rad2Deg;
        }

        public void Look(Rigidbody rigidbody, Transform head, Vector2 lookRotation)
        {
            rigidbody.rotation = Quaternion.Euler(0.0f, lookRotation.x, 0.0f);
            head.rotation = Quaternion.Euler(lookRotation.y, lookRotation.x, 0.0f);
        }
    }
}
