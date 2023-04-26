using BoschingMachine.Logic.Scripts.Interactables;
using BoschingMachine.Logic.Scripts.Meta;
using BoschingMachine.Logic.Scripts.Signals;
using BoschingMachine.Logic.Scripts.Vitallity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BoschingMachine.Logic.Scripts.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PlayerBiped : Biped.Biped
    {
        [Space]
        [SerializeField]
        private InputActionAsset inputAsset;

        [Space]
        [SerializeField][Percent]
        private float crouchSpeedPenalty;

        [Space]
        [SerializeField]
        private PlayerPickerUpper pickerUpper;
        [SerializeField] private Transform holdTarget;

        [Space]
        [SerializeField]
        private float lookDeltaSensitivity;
        [SerializeField] private float lookAdditiveSensitivity;
        [SerializeField] private PlayerCameraAnimator camAnimator;

        [Space]
        [SerializeField]
        private Interactor interactor;

        [Space]
        [SerializeField]
        private Rigidbody deadCamera;
        [SerializeField] private float deadCamForce;
        [SerializeField] private float deadCamTorque;

        [Space]
        [SerializeField]
        private Signal winSignal;

        public Health health;

        private Vector2 lookRotation;

        private InputActionMap playerMap;
        private InputActionMap persistantMap;

        private InputAction move;
        private InputAction jump;
        private InputAction crouch;
        private InputAction lookDelta;
        private InputAction lookAdditive;

        private InputAction throwObject;
        private InputAction interact;

        private GameObject normalCollision;
        private GameObject crouchCollision;

        private bool crouched;

        private CursorLock.CursorReservation cursorReservation;

        public override Vector3 MoveDirection
        {
            get
            {
                if (Frozen) return Vector2.zero;

                var input = move.ReadValue<Vector2>();
                return transform.TransformDirection(input.x, 0.0f, input.y) * SpeedPenalty;
            }
        }

        public override bool Jump => Switch(jump) && !Frozen;
        public override Vector2 LookRotation => lookRotation;
        public float FOV { get; set; }
        public float ViewmodelFOV { get; set; }

        public float SpeedPenalty
        {
            get
            {
                var s = 1.0f;
                if (crouched) s *= crouchSpeedPenalty;
                return s;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            playerMap = inputAsset.FindActionMap("Player");
            playerMap.Enable();

            move = playerMap.FindAction("move");
            jump = playerMap.FindAction("jump");
            crouch = playerMap.FindAction("crouch");

            throwObject = playerMap.FindAction("throw");
            interact = playerMap.FindAction("pickupAndDrop");
            
            persistantMap = inputAsset.FindActionMap("Player Persistant");
            persistantMap.Enable();

            lookDelta = persistantMap.FindAction("lookDelta");
            lookAdditive = persistantMap.FindAction("lookAdditive");

            var collisionGroups = transform.Find("Collision Groups");
            normalCollision = collisionGroups.GetChild(0).gameObject;
            crouchCollision = collisionGroups.GetChild(1).gameObject;
        }

        private static bool Switch(InputAction action) => action.ReadValue<float>() > 0.5f;

        protected override void OnEnable()
        {
            base.OnEnable();

            cursorReservation = new CursorLock.CursorReservation(CursorLockMode.Locked);
            cursorReservation.Push();

            interact.performed += Interact;
            throwObject.performed += Throw;

            if (TryGetComponent(out health))
            {
                health.DamageEvent += OnDamage;
                health.DeathEvent += OnDie;
            }

            crouch.performed += Crouch;

            winSignal.RaiseEvent += OnWin;
            camAnimator.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            cursorReservation.Pop();

            interact.performed -= Interact;
            throwObject.performed -= Throw;

            crouch.performed -= Crouch;

            winSignal.RaiseEvent -= OnWin;
            camAnimator.OnDisable();
        }

        private void OnDestroy()
        {
            winSignal.RaiseEvent -= OnWin;
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            interactor.Update(this, Switch(interact));
            
            lookRotation += GetLookDelta();
            lookRotation.y = Mathf.Clamp(lookRotation.y, -90.0f, 90.0f);
            base.Update();

            pickerUpper.Update(holdTarget);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            CrouchAction();

            interactor.FixedUpdate();

            pickerUpper.FixedProcess(this, holdTarget);
        }

        private void CrouchAction()
        {
            if (Jump) crouched = false;

            normalCollision.SetActive(!crouched);
            crouchCollision.SetActive(crouched);
            camAnimator.Crouched = crouched;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            camAnimator.Update(this);
        }

        private Vector2 GetLookDelta()
        {
            var input = Vector2.zero;

            if (Cursor.lockState != CursorLockMode.Locked) return input;

            if (lookDelta != null) input += lookDelta.ReadValue<Vector2>() * lookDeltaSensitivity;
            if (lookAdditive != null) input += lookAdditive.ReadValue<Vector2>() * lookAdditiveSensitivity * Time.deltaTime;

            return input;
        }

        private void Crouch(InputAction.CallbackContext c) => crouched = !crouched;

        public void Interact (InputAction.CallbackContext _)
        {
            if (interactor.TryGetLookingAt(this, out var _)) return;
            if (pickerUpper.TryGrabOrDrop(this)) return;
        }

        public void Throw(InputAction.CallbackContext _)
        {
            pickerUpper.Throw(this);
        }

        public override void Freeze()
        {
            base.Freeze();
            playerMap.Disable();
        }

        public override void Unfreeze()
        {
            base.Unfreeze();
            playerMap.Enable();
        }

        private void OnWin(object sender, System.EventArgs e)
        {
            Cursor.lockState = CursorLockMode.None;
            playerMap.Disable();
            persistantMap.Disable();

            health.Invulnerable = true;
        }

        private void OnDamage(Health.DamageArgs obj)
        {

        }

        private void OnDie(Health.DamageArgs obj)
        {
            var dcam = Instantiate(deadCamera, Head.position, Head.rotation);
            dcam.velocity = Rigidbody.GetPointVelocity(Head.position);

            dcam.velocity += Random.insideUnitSphere * deadCamForce;
            dcam.angularVelocity += Random.insideUnitSphere * deadCamTorque;
        }
    }
}
