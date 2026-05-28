using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainCamera : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float zoomSpeed = 50f;

    [SerializeField]
    private float ortographicSizeBoundLower = 2f;
    [SerializeField]
    private float ortographicSizeBoundUpper = 10f;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private BoxCollider2D mapBounds;

    private MainCameraInput input;

    private Vector3 currentMoveDelta;
    private float currentZoomDelta;

    private Vector3 nextPosition;
    private float nextZoom;
    private float zoomBasedSpeedModifier;

    private void Awake()
    {
        input = new MainCameraInput();
        nextPosition = transform.position;
        nextZoom = camera.orthographicSize;

        ortographicSizeBoundUpper = Mathf.Min(ortographicSizeBoundUpper, mapBounds.bounds.extents.y);
        ortographicSizeBoundUpper = Mathf.Min(ortographicSizeBoundUpper, mapBounds.bounds.extents.x / camera.aspect);
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
        nextZoom = nextZoom - currentZoomDelta * zoomSpeed * Time.unscaledDeltaTime;
        nextZoom = Mathf.Clamp(nextZoom, ortographicSizeBoundLower, ortographicSizeBoundUpper);

        
        zoomBasedSpeedModifier = nextZoom / ortographicSizeBoundLower;

        nextPosition = nextPosition + currentMoveDelta * moveSpeed * zoomBasedSpeedModifier * Time.unscaledDeltaTime;
        nextPosition.x = Mathf.Clamp(nextPosition.x, mapBounds.bounds.min.x + nextZoom * camera.aspect, mapBounds.bounds.max.x - nextZoom * camera.aspect);
        nextPosition.y = Mathf.Clamp(nextPosition.y, mapBounds.bounds.min.y + nextZoom, mapBounds.bounds.max.y - nextZoom);
    }

    private void LateUpdate()
    {
        float zoomLerpT = 1f - Mathf.Exp(-15f * Time.unscaledDeltaTime);
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, nextZoom, zoomLerpT);

        float moveLerpT = 1f - Mathf.Exp(-15f * Time.unscaledDeltaTime);
        transform.position = Vector3.Lerp(transform.position, nextPosition, moveLerpT);
    }
}