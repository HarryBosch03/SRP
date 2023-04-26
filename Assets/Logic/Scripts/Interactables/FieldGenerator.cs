using BoschingMachine.Logic.Scripts.Level.Signal_Groups;
using BoschingMachine.Logic.Scripts.Utility;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Interactables
{
    public class FieldGenerator : MonoBehaviour
    {
        [SerializeField] private float smoothTime;

        private Transform fieldTransform;
        private SignalGroup signalGroup;
        private new Collider collider;

        private float openPercent;
        private float velocity;

        public bool State => signalGroup.StateB;

        private void Awake()
        {
            signalGroup = SignalGroup.GetOrCreate(gameObject);
            collider = GetComponentInChildren<Collider>();

            fieldTransform = transform.DeepFind("field");
        }

        private void OnEnable()
        {
            openPercent = State ? 1 : 0;
        }

        private void Update()
        {
            openPercent = Mathf.SmoothDamp(openPercent, State ? 1 : 0, ref velocity, smoothTime);
            collider.enabled = State;

            fieldTransform.localScale = new Vector3(1.0f, openPercent, 1.0f);
        }
    }
}
