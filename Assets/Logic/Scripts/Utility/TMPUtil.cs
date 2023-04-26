using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Utility
{
    public static class TMPUtil
    {
        public static string Color(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }
    }
}
