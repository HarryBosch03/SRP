using System;
using BoschingMachine.Logic.Scripts.Utility;
using Cinemachine;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Player
{
    [System.Serializable]
    public class PlayerCameraAnimator
    {
        [SerializeField] private Transform animationContainer;

        [Space]
        [SerializeField]
        private float defaultFov;

        [Space]
        [SerializeField]
        private float moveTilt;

        [Space]
        [SerializeField]
        private float smoothTime;

        [Space]
        [SerializeField]
        private float crouchTime;
        [SerializeField] private float crouchCamOffset;
        [SerializeField] private float crouchZoom;

        private PlayerBiped biped;

        private Vector3 offset;
        private Vector2 target;
        private Vector2 position;
        private Vector2 velocity;

        private float rotationTarget;
        private float rotation;
        private float rotationalVelocity;

        private Vector3 relativeVelocity;

        private float crouchPercent;
        
        public bool Crouched { get; set; }
        public float Zoom { get; set; }

        private static event Action<float> FovChangedEvent;

        public void OnEnable()
        {
            FovChangedEvent += ChangeFov;
        }

        public void OnDisable()
        {
            FovChangedEvent -= ChangeFov;
        }
        
        public void Update(PlayerBiped biped)
        {
            Setup(biped);

            CameraMoveTilt(biped);
            CrouchCam();

            Apply(biped);
        }

        private void CrouchCam()
        {
            crouchPercent = Mathf.MoveTowards(crouchPercent, Crouched ? 1.0f : 0.0f, Time.deltaTime / crouchTime);
            var smoothed = Curves.Smootherstep(crouchPercent);

            offset += Vector3.up * crouchCamOffset * smoothed;
            Zoom *= Mathf.Lerp(1.0f, crouchZoom, smoothed);
        }

        private void CameraMoveTilt(PlayerBiped biped)
        {
            var dot = Vector3.Dot(-biped.Head.transform.right, relativeVelocity);
            rotationTarget += dot * moveTilt;
        }

        private void Setup(PlayerBiped biped)
        {
            this.biped = biped;
            var movement = biped.Movement;
            target = Vector2.zero;
            rotationTarget = 0.0f;
            offset = Vector3.zero;
            relativeVelocity = movement.RelativeVelocity(biped.Rigidbody);
        }

        private void Apply(PlayerBiped biped)
        {
            position = Vector2.SmoothDamp(position, target, ref velocity, smoothTime);
            animationContainer.localPosition = position;
            animationContainer.position += biped.transform.rotation * offset;

            rotation = Mathf.SmoothDamp(rotation, rotationTarget, ref rotationalVelocity, smoothTime);
            animationContainer.localRotation = Quaternion.Euler(Vector3.forward * rotation);

            var vcam = biped.GetComponentInChildren<CinemachineVirtualCamera>();
            if (vcam)
            {
                var fovRad = defaultFov * Mathf.Deg2Rad;
                vcam.m_Lens.FieldOfView = 2.0f * Mathf.Atan(Mathf.Tan(0.5f * fovRad) / Zoom) * Mathf.Rad2Deg;
            }
            Zoom = 1.0f;
        }

        public void ChangeFov(float newFov)
        {
            defaultFov = newFov;
        }
        
        public static void SetFov(float newFov)
        {
            FovChangedEvent?.Invoke(newFov);
        }
    }
}
