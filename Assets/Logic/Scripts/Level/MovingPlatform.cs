using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Level
{
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Rigidbody platform;
        [SerializeField] private Transform pathParent;
        [SerializeField] private float moveSpeed = 2.0f;
        [SerializeField] private float smoothTime = 0.2f;
        [SerializeField] private float holdTime = 3.0f;

        private float timer = 0.0f;
        private float distance = 0.0f;
        private int index = 0;
        private Vector3 velocity;
        private Vector3 target;

        public Rigidbody Platform => platform;
        public Transform PathParent => pathParent;

        private void Start()
        {
            platform.position = GetTargetPosition();
        }

        private void FixedUpdate()
        {
            if (pathParent.childCount == 1) pathParent.transform.position = pathParent.GetChild(0).position;
            if (pathParent.childCount < 2)
            {
                platform.isKinematic = true;
                return;
            }

            var next = GetTargetPosition();
            var last = GetLastTargetPosition();
            var direction = (next - last).normalized;
            var travelDistance = (next - last).magnitude;

            if (distance >= travelDistance)
            {
                timer += Time.deltaTime;
                distance = travelDistance;

                if (timer > holdTime)
                {
                    index = (index + 1) % PathParent.childCount;
                    distance = 0.0f;
                    timer = 0.0f;
                    return;
                }
            }
            else
            {
                distance += moveSpeed * Time.deltaTime;
                timer = 0.0f;
            }

            target = last + direction * distance;
            var newPosition = Vector3.SmoothDamp(platform.position, target, ref velocity, smoothTime);
            platform.MovePosition(newPosition);
        }

        public Vector3 GetTargetPosition()
        {
            return pathParent.GetChild(index).position;
        }

        public Vector3 GetLastTargetPosition()
        {
            return pathParent.GetChild(index != 0 ? index - 1 : pathParent.childCount - 1).position;
        }

        private void OnValidate()
        {
            if (PathParent)
            {
                var i = 0;
                foreach (Transform child in PathParent)
                {
                    child.name = $"Path Point.{++i}";
                }
            }
        }

        private void Reset()
        {
            if (transform.childCount == 2)
            {
                platform = transform.GetChild(0).GetComponent<Rigidbody>();
                pathParent = transform.GetChild(1);
            }

            if (transform.childCount == 0)
            {
                platform = new GameObject("Platform").AddComponent<Rigidbody>();
                pathParent = new GameObject("Path").transform;

                pathParent.SetAsLastSibling();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(target, 0.1f);
            }
        }
    }
}
