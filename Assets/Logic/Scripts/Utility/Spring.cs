using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Utility
{
    [System.Serializable]
    public class Spring
    {
        public float constant = 1000.0f;
        public float damper = 100.0f;
        public float maxForce = 600.0f;

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float Mass { get; set; } = 1.0f;

        public void Update(float target, float dt) => Update(Vector3.right * target, dt);
        public void Update (Vector3 target, float dt)
        {
            var diff = target - Position;
            var force = GetForce(diff);
            Integrate(force, dt);
        }

        private Vector3 GetForce(Vector3 diff)
        {
            var force = diff * constant - Velocity * damper;
            force = Vector3.ClampMagnitude(force * Mass, maxForce);
            return force;
        }

        private void Integrate (Vector3 force, float dt)
        {
            Position += Velocity * dt;
            Velocity += force * (dt / Mass);
        }

        public void Drive(Rigidbody self, Vector3 target) => Drive(self, null, target);
        public void Drive (Rigidbody self, Rigidbody other, Vector3 target)
        {
            var oldPos = Position;
            var oldVel = Velocity;
            var oldMass = Mass;

            Position = self.position;
            Velocity = self.velocity;
            Mass = self.mass;

            var diff = target - Position;
            var force = GetForce(diff);

            self.AddForce(force);
            if (other) other.AddForce(-force);

            Position = oldPos;
            Velocity = oldVel;
            Mass = oldMass;
        }
    }
}
