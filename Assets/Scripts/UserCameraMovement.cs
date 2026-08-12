using UnityEngine;
using UnityEngine.InputSystem;

public class UserCameraMovement : MonoBehaviour
{
    [Header("Mouse")]
    public float rotateSpeed = 0.25f;
    public float panSpeed = 0.05f;
    public float zoomSpeed = 1.2f;

    [Header("Keyboard")]
    public float moveSpeed = 20f;
    public float fastMultiplier = 3f;

    [Header("Smoothness")]
    [Range(1f, 50f)]
    public float positionSmoothness = 20f;

    [Range(1f, 50f)]
    public float rotationSmoothness = 20f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        if (mouse == null || keyboard == null)
            return;

        // ==========================
        // RESET CAMERA (R)
        // ==========================
        if (keyboard.rKey.wasPressedThisFrame)
        {
            targetPosition = startPosition;
            targetRotation = startRotation;
        }

        // ==========================
        // ROTATE (Right Click)
        // ==========================
        if (mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();

            Vector3 angles = targetRotation.eulerAngles;

            angles.y += delta.x * rotateSpeed;
            angles.x -= delta.y * rotateSpeed;

            if (angles.x > 180f)
                angles.x -= 360f;

            angles.x = Mathf.Clamp(angles.x, -89f, 89f);

            targetRotation = Quaternion.Euler(angles);
        }

        // ==========================
        // PAN (Left Click)
        // ==========================
        if (mouse.leftButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();

            targetPosition -= transform.right * delta.x * panSpeed;
            targetPosition -= transform.up * delta.y * panSpeed;
        }

        // ==========================
        // ZOOM (Mouse Wheel)
        // ==========================
        float scroll = mouse.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetPosition += transform.forward * scroll * zoomSpeed;
        }

        // ==========================
        // ARROW KEY MOVEMENT
        // ==========================
        float speed = moveSpeed;

        if (keyboard.leftShiftKey.isPressed)
            speed *= fastMultiplier;

        if (keyboard.upArrowKey.isPressed)
            targetPosition += transform.forward * speed * Time.deltaTime;

        if (keyboard.downArrowKey.isPressed)
            targetPosition -= transform.forward * speed * Time.deltaTime;

        if (keyboard.leftArrowKey.isPressed)
            targetPosition -= transform.right * speed * Time.deltaTime;

        if (keyboard.rightArrowKey.isPressed)
            targetPosition += transform.right * speed * Time.deltaTime;

        if (keyboard.spaceKey.isPressed)
            targetPosition += Vector3.up * speed * Time.deltaTime;

        if (keyboard.leftCtrlKey.isPressed)
            targetPosition -= Vector3.up * speed * Time.deltaTime;

        // ==========================
        // SMOOTH MOVEMENT
        // ==========================
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmoothness * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothness * Time.deltaTime);
    }
}