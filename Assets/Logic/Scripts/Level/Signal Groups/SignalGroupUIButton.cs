using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoschingMachine.Logic.Scripts.Level.Signal_Groups
{
    public class SignalGroupUIButton : Button
    {
        public bool toggle;
        public int callData;

        private SignalGroup signalGroup;

        protected override void Awake()
        {
            base.Awake();

            signalGroup = SignalGroup.GetOrCreate(gameObject);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            signalGroup.Call(GetCallData());
        }

        public int GetCallData()
        {
            if (toggle) return signalGroup.StateB ? 0 : 1;
            else return callData;
        }
    }
}
