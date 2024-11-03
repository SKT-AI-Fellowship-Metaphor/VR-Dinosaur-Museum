using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public Vector3 cameraOffset = new Vector3(0f, 0f, 0f);
    
    public float zoomSpeed = 1.5f;
    public float minZoomDistance = 1f;
    public float maxZoomDistance = 10f;

    private float xRotation = 0f;
    private float currentZoom;
    private float targetZoom;

    public LayerMask collisionLayers;
    public float collisionOffset = 0.2f;

    private bool isAltPressed = false;

    void Start()
    {
        SetCursorState(true);

        if (playerBody == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerBody = player.transform;
            }
            else
            {
                Debug.LogError("Player not found. Make sure the player has the 'Player' tag.");
            }
        }

        currentZoom = targetZoom = cameraOffset.magnitude;
    }

    void LateUpdate()
    {
        if (playerBody == null) return;

        // Check for Alt key press/release
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            isAltPressed = true;
            SetCursorState(false);
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
        {
            isAltPressed = false;
            SetCursorState(true);
        }

        // Only process camera movement if Alt is not pressed
        if (!isAltPressed)
        {
            // Mouse look
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Control rotation around x axis (Look up and down)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -60f, 60f);

            // Calculate the desired rotation
            Quaternion rotation = playerBody.rotation * Quaternion.Euler(xRotation, 0f, 0f);

            // Set the camera's rotation
            transform.rotation = rotation;

            // Rotate the player body horizontally
            playerBody.Rotate(Vector3.up * mouseX);
        }

        // Zoom (allowed even when Alt is pressed)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Zoom(scroll);

        // Smoothly interpolate current zoom towards target zoom
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * 10f);

        // Calculate the desired camera position
        Vector3 zoomedOffset = cameraOffset.normalized * currentZoom;
        Vector3 desiredPosition = playerBody.position + transform.rotation * zoomedOffset;

        // Check for collision and adjust camera position
        RaycastHit hit;
        if (Physics.Raycast(playerBody.position, transform.rotation * zoomedOffset.normalized, out hit, currentZoom, collisionLayers))
        {
            desiredPosition = hit.point + hit.normal * collisionOffset;
        }

        // Set the camera's position
        transform.position = desiredPosition;
    }

    void Zoom(float scrollInput)
    {
        if (scrollInput != 0)
        {
            targetZoom -= scrollInput * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoomDistance, maxZoomDistance);
        }
    }

    void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}