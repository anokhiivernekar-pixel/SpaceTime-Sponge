using UnityEngine;
using UnityEngine.InputSystem;

public class UserCameraMovement : MonoBehaviour
{
    // =========================================================
    // MOUSE
    // =========================================================

    [Header("Mouse")]

    public float rotateSpeed = 0.25f;

    public float panSpeed = 0.05f;

    public float zoomSpeed = 1.2f;


    // =========================================================
    // HP PEN
    // =========================================================

    [Header("HP Pen")]

    [Tooltip("Normal pen drag moves the camera.")]
    public float penPanSpeed = 0.04f;

    [Tooltip("Pen button + drag rotates the camera.")]
    public float penRotateSpeed = 0.20f;

    [Range(0f, 1f)]
    public float minimumPenPressure = 0.001f;


    // =========================================================
    // TOUCHSCREEN
    // =========================================================

    [Header("Touchscreen")]

    public bool allowTouchscreen = true;

    public float touchPanSpeed = 0.04f;


    // =========================================================
    // KEYBOARD
    // =========================================================

    [Header("Keyboard")]

    public float moveSpeed = 20f;

    public float fastMultiplier = 3f;


    // =========================================================
    // SMOOTHNESS
    // =========================================================

    [Header("Smoothness")]

    [Range(1f, 50f)]
    public float positionSmoothness = 20f;

    [Range(1f, 50f)]
    public float rotationSmoothness = 20f;


    // =========================================================
    // DEBUG
    // =========================================================

    [Header("Debug")]

    public bool showInputDebug = false;


    // =========================================================
    // CAMERA VALUES
    // =========================================================

    private Vector3 targetPosition;

    private Quaternion targetRotation;

    private Vector3 startPosition;

    private Quaternion startRotation;


    // =========================================================
    // PEN TRACKING
    // =========================================================

    private Vector2 previousPenPosition;

    private bool penWasDown = false;


    // =========================================================
    // TOUCH TRACKING
    // =========================================================

    private Vector2 previousTouchPosition;

    private bool touchWasDown = false;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        targetPosition =
            transform.position;

        targetRotation =
            transform.rotation;

        startPosition =
            transform.position;

        startRotation =
            transform.rotation;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        Mouse mouse =
            Mouse.current;

        Keyboard keyboard =
            Keyboard.current;

        Pen pen =
            Pen.current;

        Touchscreen touchscreen =
            Touchscreen.current;


        // =====================================================
        // RESET CAMERA
        // =====================================================

        if (
            keyboard != null &&
            keyboard.rKey.wasPressedThisFrame
        )
        {
            targetPosition =
                startPosition;

            targetRotation =
                startRotation;
        }


        bool penControlling =
            false;

        bool touchControlling =
            false;


        // =====================================================
        // HP PEN
        // =====================================================

        if (pen != null)
        {
            Vector2 penPosition =
                pen.position.ReadValue();


            // =================================================
            // PEN TIP
            // =================================================

            bool tipPressed =
                false;


            if (pen.tip != null)
            {
                tipPressed =
                    pen.tip.isPressed;
            }


            // =================================================
            // PEN PRESSURE
            // =================================================

            float pressure =
                0f;


            if (pen.pressure != null)
            {
                pressure =
                    pen.pressure.ReadValue();
            }


            bool pressurePressed =
                pressure >
                minimumPenPressure;


            // =================================================
            // PEN IS IN RANGE
            // =================================================

            bool penInRange =
                false;


            if (pen.inRange != null)
            {
                penInRange =
                    pen.inRange.isPressed;
            }


            // =================================================
            // PEN BUTTONS
            // =================================================

            bool firstBarrelButton =
                false;


            bool secondBarrelButton =
                false;


            bool thirdBarrelButton =
                false;


            bool fourthBarrelButton =
                false;


            if (pen.firstBarrelButton != null)
            {
                firstBarrelButton =
                    pen.firstBarrelButton.isPressed;
            }


            if (pen.secondBarrelButton != null)
            {
                secondBarrelButton =
                    pen.secondBarrelButton.isPressed;
            }


            if (pen.thirdBarrelButton != null)
            {
                thirdBarrelButton =
                    pen.thirdBarrelButton.isPressed;
            }


            if (pen.fourthBarrelButton != null)
            {
                fourthBarrelButton =
                    pen.fourthBarrelButton.isPressed;
            }


            // =================================================
            // WINDOWS MOUSE FALLBACKS
            //
            // HP/Windows may convert a pen side button into
            // a mouse button instead of a Pen barrel button.
            // =================================================

            bool windowsRightButton =
                false;


            bool windowsMiddleButton =
                false;


            if (
                mouse != null &&
                penInRange
            )
            {
                windowsRightButton =
                    mouse.rightButton.isPressed;


                windowsMiddleButton =
                    mouse.middleButton.isPressed;
            }


            // =================================================
            // ROTATE MODE
            //
            // We check EVERY possible side-button signal.
            //
            // This makes the HP pen much more likely to work.
            // =================================================

            bool rotateMode =
                firstBarrelButton ||
                secondBarrelButton ||
                thirdBarrelButton ||
                fourthBarrelButton ||
                windowsRightButton ||
                windowsMiddleButton;


            // =================================================
            // IS PEN TOUCHING?
            // =================================================

            bool penDown =
                tipPressed ||
                pressurePressed;


            // =================================================
            // PEN FIRST TOUCH
            // =================================================

            if (
                penDown &&
                !penWasDown
            )
            {
                previousPenPosition =
                    penPosition;


                penWasDown =
                    true;


                penControlling =
                    true;
            }


            // =================================================
            // PEN DRAG
            // =================================================

            else if (
                penDown &&
                penWasDown
            )
            {
                penControlling =
                    true;


                Vector2 delta =
                    penPosition -
                    previousPenPosition;


                // Ignore giant Windows pointer jumps.

                if (
                    delta.magnitude >
                    0.01f &&
                    delta.magnitude <
                    300f
                )
                {
                    // =========================================
                    // BUTTON + PEN = ROTATE
                    // =========================================

                    if (rotateMode)
                    {
                        RotateCamera(
                            delta,
                            penRotateSpeed
                        );


                        if (showInputDebug)
                        {
                            Debug.Log(
                                "PEN MODE = ROTATE"
                            );
                        }
                    }


                    // =========================================
                    // NORMAL PEN = MOVE / PAN
                    // =========================================

                    else
                    {
                        PanCamera(
                            delta,
                            penPanSpeed
                        );


                        if (showInputDebug)
                        {
                            Debug.Log(
                                "PEN MODE = MOVE"
                            );
                        }
                    }
                }


                previousPenPosition =
                    penPosition;
            }


            // =================================================
            // PEN RELEASED
            // =================================================

            else
            {
                penWasDown =
                    false;
            }


            // =================================================
            // DEBUG INFORMATION
            // =================================================

            if (
                showInputDebug &&
                (
                    penDown ||
                    rotateMode
                )
            )
            {
                Debug.Log(
                    "HP PEN | " +
                    "First=" +
                    firstBarrelButton +
                    " | Second=" +
                    secondBarrelButton +
                    " | Third=" +
                    thirdBarrelButton +
                    " | Fourth=" +
                    fourthBarrelButton +
                    " | RightMouse=" +
                    windowsRightButton +
                    " | MiddleMouse=" +
                    windowsMiddleButton +
                    " | Pressure=" +
                    pressure
                );
            }
        }
        else
        {
            penWasDown =
                false;
        }


        // =====================================================
        // TOUCHSCREEN
        // =====================================================

        if (
            !penControlling &&
            allowTouchscreen &&
            touchscreen != null
        )
        {
            bool pressed =
                touchscreen
                    .primaryTouch
                    .press
                    .isPressed;


            Vector2 position =
                touchscreen
                    .primaryTouch
                    .position
                    .ReadValue();


            // =================================================
            // TOUCH START
            // =================================================

            if (
                pressed &&
                !touchWasDown
            )
            {
                previousTouchPosition =
                    position;


                touchWasDown =
                    true;


                touchControlling =
                    true;
            }


            // =================================================
            // TOUCH DRAG
            // =================================================

            else if (
                pressed &&
                touchWasDown
            )
            {
                touchControlling =
                    true;


                Vector2 delta =
                    position -
                    previousTouchPosition;


                if (
                    delta.magnitude >
                    0.01f &&
                    delta.magnitude <
                    300f
                )
                {
                    PanCamera(
                        delta,
                        touchPanSpeed
                    );
                }


                previousTouchPosition =
                    position;
            }


            // =================================================
            // TOUCH RELEASE
            // =================================================

            else
            {
                touchWasDown =
                    false;
            }
        }


        // =====================================================
        // NORMAL MOUSE
        // =====================================================

        if (
            mouse != null &&
            !penControlling &&
            !touchControlling
        )
        {
            // =================================================
            // RIGHT CLICK = ROTATE
            // =================================================

            if (mouse.rightButton.isPressed)
            {
                Vector2 delta =
                    mouse.delta.ReadValue();


                RotateCamera(
                    delta,
                    rotateSpeed
                );
            }


            // =================================================
            // LEFT CLICK = MOVE
            // =================================================

            if (mouse.leftButton.isPressed)
            {
                Vector2 delta =
                    mouse.delta.ReadValue();


                PanCamera(
                    delta,
                    panSpeed
                );
            }


            // =================================================
            // SCROLL = ZOOM
            // =================================================

            float scroll =
                mouse.scroll.ReadValue().y;


            if (
                Mathf.Abs(scroll) >
                0.01f
            )
            {
                targetPosition +=
                    transform.forward *
                    scroll *
                    zoomSpeed;
            }
        }


        // =====================================================
        // KEYBOARD MOVEMENT
        // =====================================================

        if (keyboard != null)
        {
            float speed =
                moveSpeed;


            if (
                keyboard.leftShiftKey.isPressed ||
                keyboard.rightShiftKey.isPressed
            )
            {
                speed *=
                    fastMultiplier;
            }


            // FORWARD

            if (keyboard.upArrowKey.isPressed)
            {
                targetPosition +=
                    transform.forward *
                    speed *
                    Time.deltaTime;
            }


            // BACKWARD

            if (keyboard.downArrowKey.isPressed)
            {
                targetPosition -=
                    transform.forward *
                    speed *
                    Time.deltaTime;
            }


            // LEFT

            if (keyboard.leftArrowKey.isPressed)
            {
                targetPosition -=
                    transform.right *
                    speed *
                    Time.deltaTime;
            }


            // RIGHT

            if (keyboard.rightArrowKey.isPressed)
            {
                targetPosition +=
                    transform.right *
                    speed *
                    Time.deltaTime;
            }


            // UP

            if (keyboard.spaceKey.isPressed)
            {
                targetPosition +=
                    Vector3.up *
                    speed *
                    Time.deltaTime;
            }


            // DOWN

            if (
                keyboard.leftCtrlKey.isPressed ||
                keyboard.rightCtrlKey.isPressed
            )
            {
                targetPosition -=
                    Vector3.up *
                    speed *
                    Time.deltaTime;
            }
        }


        // =====================================================
        // SMOOTH POSITION
        // =====================================================

        float positionAmount =
            1f -
            Mathf.Exp(
                -positionSmoothness *
                Time.deltaTime
            );


        transform.position =
            Vector3.Lerp(
                transform.position,
                targetPosition,
                positionAmount
            );


        // =====================================================
        // SMOOTH ROTATION
        // =====================================================

        float rotationAmount =
            1f -
            Mathf.Exp(
                -rotationSmoothness *
                Time.deltaTime
            );


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationAmount
            );
    }


    // =========================================================
    // MOVE / PAN
    // =========================================================

    void PanCamera(
        Vector2 delta,
        float speed
    )
    {
        targetPosition -=
            transform.right *
            delta.x *
            speed;


        targetPosition -=
            transform.up *
            delta.y *
            speed;
    }


    // =========================================================
    // ROTATE
    // =========================================================

    void RotateCamera(
        Vector2 delta,
        float speed
    )
    {
        Vector3 angles =
            targetRotation.eulerAngles;


        if (angles.x > 180f)
        {
            angles.x -=
                360f;
        }


        // LEFT / RIGHT

        angles.y +=
            delta.x *
            speed;


        // UP / DOWN

        angles.x -=
            delta.y *
            speed;


        // Prevent upside-down flipping.

        angles.x =
            Mathf.Clamp(
                angles.x,
                -89f,
                89f
            );


        targetRotation =
            Quaternion.Euler(
                angles.x,
                angles.y,
                0f
            );
    }
}