using System;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Utility
{
    [Serializable]
    public sealed class SeccondOrderDynamicsF
    {
        public SeccondOrderDynamicsSettings settings;
        private SeccondOrderDynamicsState state;

        public float Position { get; set; }
        public float Velocity { get; set; }
        public SeccondOrderDynamics.ProcessOperation ProcessOperation { get; set; } = SeccondOrderDynamics.Process;

        public SeccondOrderDynamicsF() : this(1.0f, 0.5f, 2.0f) { }
        public SeccondOrderDynamicsF(float f, float z, float r) : this(0.0f, f, z, r) { }
        public SeccondOrderDynamicsF(float lastTarget, float f, float z, float r)
        {
            settings = new SeccondOrderDynamicsSettings(f, z, r);
            state = new SeccondOrderDynamicsState(lastTarget);
        }

        public void Loop(float target, float? targetVelocity, float dt)
        {
            Position += Velocity * dt;
            Velocity += ProcessOperation(Position, Velocity, target, targetVelocity, dt, settings, state) * dt;
        }
    }

    [Serializable]
    public sealed class SeccondOrderDynamicsV2
    {
        public SeccondOrderDynamicsSettings settings;
        private SeccondOrderDynamicsState x;
        private SeccondOrderDynamicsState y;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public SeccondOrderDynamics.ProcessOperation ProcessOperation { get; set; } = SeccondOrderDynamics.Process;

        public SeccondOrderDynamicsV2() : this(1.0f, 0.5f, 2.0f) { }
        public SeccondOrderDynamicsV2(float f, float z, float r) : this(Vector3.zero, f, z, r) { }
        public SeccondOrderDynamicsV2(Vector2 lastTarget, float f, float z, float r)
        {
            settings = new SeccondOrderDynamicsSettings(f, z, r);
            x = new SeccondOrderDynamicsState(lastTarget.x);
            y = new SeccondOrderDynamicsState(lastTarget.y);
        }

        public void Process(Vector2 target, Vector2? targetVelocity, float dt)
        {
            Func<Func<Vector2, float>, SeccondOrderDynamicsState, float> process = (f, state) =>
            {
                return ProcessOperation(f(Position), f(Velocity), f(target), targetVelocity.HasValue ? f(targetVelocity.Value) : null, dt, settings, state);
            };

            var fx = process(v => v.x, x);
            var fy = process(v => v.y, y);

            Position += Velocity * dt;
            Velocity += new Vector2(fx, fy) * dt;
        }
    }

    [Serializable]
    public sealed class SeccondOrderDynamicsV3
    {
        public SeccondOrderDynamicsSettings settings;
        private SeccondOrderDynamicsState x;
        private SeccondOrderDynamicsState y;
        private SeccondOrderDynamicsState z;

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public SeccondOrderDynamics.ProcessOperation ProcessOperation { get; set; } = SeccondOrderDynamics.Process;

        public SeccondOrderDynamicsV3() : this(1.0f, 0.5f, 2.0f) { }
        public SeccondOrderDynamicsV3(float f, float z, float r) : this(Vector3.zero, f, z, r) { }
        public SeccondOrderDynamicsV3(Vector3 lastTarget, float f, float z, float r)
        {
            settings = new SeccondOrderDynamicsSettings(f, z, r);
            x = new SeccondOrderDynamicsState(lastTarget.x);
            y = new SeccondOrderDynamicsState(lastTarget.y);
            this.z = new SeccondOrderDynamicsState(lastTarget.z);
        }

        public void Process(Vector3 target, Vector3? targetVelocity, float dt)
        {
            Func<Func<Vector3, float>, SeccondOrderDynamicsState, float> process = (f, state) =>
            {
                return ProcessOperation(f(Position), f(Velocity), f(target), targetVelocity.HasValue ? f(targetVelocity.Value) : null, dt, settings, state);
            };

            var fx = process(v => v.x, x);
            var fy = process(v => v.y, y);
            var fz = process(v => v.z, z);

            Position += Velocity * dt;
            Velocity += new Vector3(fx, fy, fz) * dt;
        }
    }

    public static class SeccondOrderDynamics
    {
        public delegate float ProcessOperation(float position, float velocity, float targetPosition, float? targetVelocity, float dt, SeccondOrderDynamicsSettings settings, SeccondOrderDynamicsState state);

        public static float ProcessRotation(float position, float velocity, float targetPosition, float? targetVelocity, float dt, SeccondOrderDynamicsSettings settings, SeccondOrderDynamicsState state)
        {
            var altTarget = targetPosition < 180.0f ? targetPosition + 360.0f : targetPosition - 360.0f;
            if (Mathf.Abs(targetPosition - position) > Mathf.Abs(altTarget - position)) targetPosition = altTarget;

            if (!targetVelocity.HasValue)
            {
                targetVelocity = Mathf.DeltaAngle(targetPosition, state.lastTargetPosition) / dt;
            }

            return Process(position, velocity, targetPosition, targetVelocity, dt, settings, state);
        }

        public static float Process(float position, float velocity, float targetPosition, float? targetVelocity, float dt, SeccondOrderDynamicsSettings settings, SeccondOrderDynamicsState state)
        {
            if (!targetVelocity.HasValue)
            {
                targetVelocity = (targetPosition - state.lastTargetPosition) / dt;
            }

            var w = 2.0f * Mathf.PI * settings.F;
            var z = settings.Z;
            var d = w * Mathf.Sqrt(Mathf.Abs(z * z - 1));

            float k1Stable, k2Stable;
            if (w * dt < z)
            {
                k1Stable = settings.k1;
                k2Stable = Mathf.Max(settings.k2, dt * dt / 2.0f + dt * settings.k1 / 2.0f, dt * settings.k1);
            }
            else
            {
                var t1 = Mathf.Exp(-z * w * dt);
                var alpha = 2.0f * t1 * (z <= 1.0f ? Mathf.Cos(dt * d) : MathF.Cosh(dt * d));
                var beta = t1 * t1;
                var t2 = dt / (1.0f + beta - alpha);
                k1Stable = (1.0f - beta) * t2;
                k2Stable = dt * t2;
            }

            state.lastTargetPosition = targetPosition;

            return (dt * (targetPosition + settings.k3 * targetVelocity.Value - position - k1Stable * velocity) / k2Stable) / dt;
        }

        public static void GetAbstracts(in float k1, in float k2, in float k3, out float f, out float z, out float r)
        {
            f = GetF(k1, k2, k3);
            z = GetZ(k1, k2, k3);
            r = GetR(k1, k2, k3);
        }

        public static void CalculateAbstracts(in float f, in float z, in float r, out float k1, out float k2, out float k3)
        {
            k1 = z / (Mathf.PI * f);
            k2 = 1.0f / Mathf.Pow(2.0f * Mathf.PI * f, 2);
            k3 = (r * z) / (2.0f * Mathf.PI * f);
        }

        public static float GetF(float k1, float k2, float k3) => 1.0f / (2.0f * Mathf.PI * Mathf.Sqrt(k2));
        public static float GetZ(float k1, float k2, float k3) => k1 / (2.0f * Mathf.Sqrt(k2));
        public static float GetR(float k1, float k2, float k3) => (2.0f * k3) / k1;
    }

    [System.Serializable]
    public class SeccondOrderDynamicsSettings
    {
        public float k1;
        public float k2;
        public float k3;

        public float F
        {
            get => SeccondOrderDynamics.GetF(k1, k2, k3);
            set => SetAbstracts(value, Z, R);
        }

        public float Z
        {
            get => SeccondOrderDynamics.GetZ(k1, k2, k3);
            set => SetAbstracts(Z, value, R);
        }

        public float R
        {
            get => SeccondOrderDynamics.GetR(k1, k2, k3);
            set => SetAbstracts(F, Z, value);
        }

        public SeccondOrderDynamicsSettings()
        {
            SetAbstracts(1.0f, 0.5f, 2.0f);
        }

        public SeccondOrderDynamicsSettings(float f, float z, float r)
        {
            SetAbstracts(f, z, r);
        }

        public SeccondOrderDynamicsSettings SetAbstracts(float f, float z, float r)
        {
            SeccondOrderDynamics.CalculateAbstracts(f, z, r, out k1, out k2, out k3);
            return this;
        }
    }

    [System.Serializable]
    public class SeccondOrderDynamicsState
    {
        public float lastTargetPosition;

        public SeccondOrderDynamicsState(float lastTargetPosition = 0.0f)
        {
            this.lastTargetPosition = lastTargetPosition;
        }
    }
}