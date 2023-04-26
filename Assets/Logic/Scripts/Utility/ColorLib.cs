using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Utility
{
    public static class ColorLib
    {
        public static Color Gray(int v) => new Color(v / 255.0f, v / 255.0f, v / 255.0f);
    }
}
