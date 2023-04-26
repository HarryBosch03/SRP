using BoschingMachine.Logic.Scripts.Utility;
using TMPro;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Player
{
    [System.Serializable]
    public class PlayerPickerUpperUI
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private SeccondOrderDynamicsF spring;

        public void Update (PlayerPickerUpper pickerUpper)
        {
            var scaleTarget = 0.0f;

            if (!pickerUpper.HeldObject && pickerUpper.LookingAt)
            {
                text.text = $"Grab {pickerUpper.LookingAt.name}";
                scaleTarget = 1.0f;
            }

            spring.Loop(scaleTarget, null, Time.deltaTime);
            group.transform.localScale = Vector3.one * Mathf.Max(spring.Position, 0.0f);
        }
    }
}
