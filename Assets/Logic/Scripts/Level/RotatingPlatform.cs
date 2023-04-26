using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level
{
    [RequireComponent(typeof(Rigidbody))]
    public class RotatingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 axis;
        [SerializeField] private float revolutionTime;
        
        private new Rigidbody rigidbody;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            var degPerSeccond = 360.0f / revolutionTime;
            rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(axis * degPerSeccond * Time.deltaTime));
        }
    }
}
