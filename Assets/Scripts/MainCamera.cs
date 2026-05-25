using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainCamera : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private float zoomSpeed = 50f;

    [SerializeField]
    private float ortographicSizeBoundLower = 2f;
    [SerializeField]
    private float ortographicSizeBoundUpper = 10f;

    [SerializeField]
    private Camera camera;

    private MainCameraInput input;

    private Vector3 currentMoveDelta;
    private float currentZoomDelta;

    private Vector3 nextPosition;
    private float nextZoom;

    private void Awake()
    {
        input = new MainCameraInput();
        nextPosition = transform.position;
        nextZoom = camera.orthographicSize;
    }

    private void OnEnable()
    {
        input.Enable();
        input.Camera.Move.performed += OnMove;
        input.Camera.Move.canceled += OnMove;
        input.Camera.Zoom.performed += OnZoom;
        input.Camera.Zoom.canceled += OnZoom;
    }
    private void OnDisable()
    {
        input.Camera.Move.performed -= OnMove;
        input.Camera.Move.canceled -= OnMove;
        input.Camera.Zoom.performed -= OnZoom;
        input.Camera.Zoom.canceled -= OnZoom;
        input.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        currentMoveDelta = context.ReadValue<Vector2>();
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        currentZoomDelta = context.ReadValue<float>();
    }

    void Update()
    {
        nextPosition = nextPosition + currentMoveDelta * moveSpeed * Time.deltaTime;

        nextZoom = nextZoom - currentZoomDelta * zoomSpeed * Time.deltaTime;
        nextZoom = Mathf.Clamp(nextZoom, ortographicSizeBoundLower, ortographicSizeBoundUpper);

        transform.position = nextPosition;
        camera.orthographicSize = nextZoom;
    }
}
