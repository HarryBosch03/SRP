using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Biped
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class Biped : MonoBehaviour
    {
        [SerializeField] private BipedalMovement movement;
        [SerializeField] private Transform head;

        public Rigidbody Rigidbody { get; private set; }
        public BipedalMovement Movement => movement;

        public virtual Vector3 MoveDirection { get; }
        public virtual bool Jump { get; }
        public virtual Vector2 LookRotation { get; }
        public Transform Head => head;
        public bool Frozen { get; private set; }

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable ()
        {

        }

        protected virtual void OnDisable ()
        {

        }

        protected virtual void Start ()
        {

        }

        protected virtual void FixedUpdate ()
        {
            Movement.Move(Rigidbody, Frozen ? Vector3.zero : MoveDirection, Jump);
        }

        protected virtual void Update ()
        {
            Movement.Look(Rigidbody, head, LookRotation);
        }

        protected virtual void LateUpdate ()
        {

        }

        public virtual void Freeze()
        {
            Frozen = true;
        }

        public virtual void Unfreeze()
        {
            Frozen = false;
        }
    }
}
