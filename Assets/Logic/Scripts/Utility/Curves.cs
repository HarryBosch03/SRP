using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Utility
{
    public static class Curves
    {
        public static float EaseOutElastic(float x)
        {
            if (x < 0.0f) return 0.0f;
            if (x > 1.0f) return 1.0f;

            var c4 = (2 * Mathf.PI) / 3;
            return Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
        }

        public static float EaseOutBack (float x)
        {
            var c1 = 1.70158f;
            var c3 = c1 + 1;

            return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
        }

        public static float Smootherstep(float x) => x * x * x * (x * (x * 6.0f - 15.0f) + 10.0f);
    }
}
