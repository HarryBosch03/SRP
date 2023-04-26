using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Freecam : MonoBehaviour
{
    public float panSpeed, panAcceleration;

    private Vector2 rotation;

    private Vector3 velocity;
    private Vector3 acceleration;

    private Vector3 moveInput;

    private Camera camera;

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    private void FixedUpdate()
    {
        acceleration = (moveInput * panSpeed - velocity) / panAcceleration;

        transform.position += velocity * Time.deltaTime;
        velocity += acceleration * Time.deltaTime;
        acceleration = Vector3.zero;
    }

    void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            rotation += Mouse.current.delta.ReadValue() * 0.3f;

            var input = new Vector3
            {
                x = Keyboard.current.dKey.ReadValue() - Keyboard.current.aKey.ReadValue(),
                z = Keyboard.current.wKey.ReadValue() - Keyboard.current.sKey.ReadValue(),
                y = Keyboard.current.eKey.ReadValue() - Keyboard.current.qKey.ReadValue(),
            };
            input = Vector3.ClampMagnitude(input, 1.0f);
            moveInput = transform.TransformDirection(input);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            moveInput = Vector3.zero;
        }

        rotation.y = Mathf.Clamp(rotation.y, -90.0f, 90.0f);
        transform.rotation = Quaternion.Euler(-rotation.y, rotation.x, 0.0f);
    }
}