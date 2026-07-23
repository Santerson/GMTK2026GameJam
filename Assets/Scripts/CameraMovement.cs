using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraMovement : MonoBehaviour
{
    [Header("following settings")]
    [SerializeField] GameObject TargetObject;
    [SerializeField] float FollowSpeed = 0.1f;
    [SerializeField] Vector2 offset = new Vector2(0, 4f);

    [Header("Zooming settings")]
    [Tooltip("The minimum step the camera zooms in by per fixed update")]
    [SerializeField] float zoomInRate = 0.02f;
    [Tooltip("The easing factor for the zooming. The camera will slow down as it approaches the target zoom level")]
    [SerializeField] float zoomInDivider = 0.15f;
    Camera refCamera;
    cameraStates currentCameraState = cameraStates.normal;
    float zoomInScale = 5f;
    float cameraDefaultScale = 9f;

    /// <summary>
    /// Contains states for the camera depending on what it is doing regarding to zooming in and out
    /// </summary>
    enum cameraStates
    {
        normal,
        zoomingIn,
        zoomedIn,
        zoomingOut
    }

    /// <summary>
    /// Draws the line between the target object and the camera to visualize the offset in the editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (TargetObject != null)
        {
            Vector2 targetPos = (Vector2)TargetObject.transform.position + offset;
            Debug.DrawLine(TargetObject.transform.position, targetPos, Color.red);
            Debug.DrawLine(targetPos, transform.position, Color.green);
        }
    }

    private void Start()
    {
        refCamera = GetComponent<Camera>();
        if (refCamera == null)
            Debug.LogError("CameraMovement: No camera component found on this object.");
        cameraDefaultScale = refCamera.orthographicSize;
    }

    private void FixedUpdate()
    {
        // slowly move towards the target position
        if (TargetObject != null)
        {
            Vector2 targetPos = (Vector2)TargetObject.transform.position + offset;
            Vector2 newPos = Vector2.Lerp(transform.position, targetPos, FollowSpeed);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
        // Zooms out and in the camera depending on the state
        if (currentCameraState == cameraStates.zoomingIn)
            ZoomInCamera();
        if (currentCameraState == cameraStates.zoomingOut)
            ZoomOutCamera();
    }

    /// <summary>
    /// Set a new offset for the camera
    /// </summary>
    /// <param name="newOffset">The new offset</param>
    public void SetOffset(Vector2 newOffset)
    {
        offset = newOffset;
    }

    /// <summary>
    /// Get the current offset of the camera
    /// </summary>
    /// <returns>The current offset of the camera</returns>
    public Vector2 GetOffset()
    {
        return offset;
    }

    public void StartCameraZoomin(float zoominAmount)
    {
        zoomInScale = zoominAmount;
        currentCameraState = cameraStates.zoomingIn;
    }

    void ZoomInCamera()
    {
        // Calculate the difference between current and target scale
        float distance = refCamera.orthographicSize - zoomInScale;

        // Use a fraction of the distance to slow down as it approaches the target
        float step = Mathf.Max(distance * zoomInDivider, zoomInRate); // 0.15f is the easing factor, 0.02f is the minimum step

        refCamera.orthographicSize -= step;

        if (refCamera.orthographicSize < zoomInScale)
        {
            refCamera.orthographicSize = zoomInScale;
            currentCameraState = cameraStates.zoomedIn;
        }
    }

    public void StartCameraZoomout()
    {
        currentCameraState = cameraStates.zoomingOut;
    }

    void ZoomOutCamera()
    {
        // Calculate the difference between current and target scale
        float distance = cameraDefaultScale - refCamera.orthographicSize;

        // Use a fraction of the distance to slow down as it approaches the target
        float step = Mathf.Max(Mathf.Abs(distance) * zoomInDivider, zoomInRate);

        refCamera.orthographicSize += step;

        if (refCamera.orthographicSize > cameraDefaultScale)
        {
            refCamera.orthographicSize = cameraDefaultScale;
            currentCameraState = cameraStates.normal;
        }
    }
}
