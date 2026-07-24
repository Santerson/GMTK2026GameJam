using System.Collections.Generic;
using System.Collections;
using UnityEngine;


public class CameraTrack : MonoBehaviour
{
    [Header("Camera Track Settings")]
    [Tooltip("If true, the camera will start moving along the track when the scene starts.")]
        [SerializeField] bool PlayOnStart = true;
    [Tooltip("The starting point of the camera track")]
        [SerializeField] Vector2 StartPoint;
    [Tooltip("Time before starting the animation")]
        [SerializeField] float StartDelay = 0f;
    [Tooltip("Initial camera zoom level before starting the animation")]
        [SerializeField] float InitialCameraZoom = 9f;
    [Tooltip("The nodes that define the camera track")]
        [SerializeField] List<CameraTrackNode> TrackNodes = new List<CameraTrackNode>();
    [SerializeField] float EndZoom = 9f;
    [SerializeField] bool pullUpTimeBankUIAfterPan = true;
    CameraMovement refCamera;
    Rigidbody2D refRB;

    [System.Serializable]
    public struct CameraTrackNode
    {
        [Tooltip("The position the camera will glide to")]
        public Vector2 Position;
        [Tooltip("The speed the camera will glide to this position. Dont do super high numbers")]
        public float camSpeed;
        [Tooltip("The zoom the camera will glide to this position")]
        public float camZoom;
        [Tooltip("The time the camera will pause at this position")]
        public float camPauseTime;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a cross at the start point
        Debug.DrawLine(StartPoint + Vector2.up * 0.25f, StartPoint + Vector2.down * 0.25f, Color.green);
        Debug.DrawLine(StartPoint + Vector2.left * 0.25f, StartPoint + Vector2.right * 0.25f, Color.green);

        Vector2 LastPoint = StartPoint;
        // Then for each subsequent point
        foreach (CameraTrackNode trackNode in TrackNodes)
        {
            // Draw a cross at the at point
            Debug.DrawLine(trackNode.Position + Vector2.up * 0.25f, trackNode.Position + Vector2.down * 0.25f, Color.green);
            Debug.DrawLine(trackNode.Position + Vector2.left * 0.25f, trackNode.Position + Vector2.right * 0.25f, Color.green);
            // Draw a line from the previous point to this point
            Debug.DrawLine(LastPoint, trackNode.Position, Color.red);
            LastPoint = trackNode.Position;
        }
    }

    private void Start()
    {
        refCamera = FindFirstObjectByType<CameraMovement>();
        refRB = GetComponent<Rigidbody2D>();
        if (PlayOnStart)
        {
            // Look for the timebank data script
            AllocatedTimeStorage allocatedTimeStorage = FindFirstObjectByType<AllocatedTimeStorage>();
            // check if it exists
            if (allocatedTimeStorage != null)
            {
                // Check it's variables and see if they're all 0 to play the intro
                if (allocatedTimeStorage.allocatedTimeLeft != 0 || allocatedTimeStorage.allocatedTimeRight != 0 || allocatedTimeStorage.allocatedTimeJump != 0)
                {
                    PullUpTimeAllocationUI();
                    return;
                }
            }
            StartCoroutine(PlayCameraPan());
        }
    }

    /// <summary>
    /// Starts the camera pan along the track defined by the StartPoint and TrackNodes.
    /// </summary>
    public IEnumerator PlayCameraPan()
    {
        // Set this object's position to the start point
        transform.position = StartPoint;
        // Set the camera to follow this object
        GameObject trackingObj = refCamera.getTrackingObject();
        Vector2 camOffset = refCamera.GetOffset();
        // Set up the camera and player to not be ass
        FindFirstObjectByType<PlayerMovement>().canMove = false;
        refCamera.SetOffset(Vector2.zero);
        refCamera.SetTrackingObject(gameObject);
        refCamera.transform.position = new(StartPoint.x, StartPoint.y, refCamera.transform.position.z);
        refCamera.GetComponent<Camera>().orthographicSize = InitialCameraZoom;
        yield return new WaitForSeconds(StartDelay);
        for (int i = 0; i < TrackNodes.Count; i++)
        {
            CameraTrackNode trackNode = TrackNodes[i];
            // Set the zoom of the camera
            refCamera.ChangeCameraZoom(trackNode.camZoom);
            // Apply a velocity towards the next node at the speed speed
            float speed = trackNode.camSpeed;
            Vector2 direction = (trackNode.Position - (Vector2)transform.position).normalized;
            // After reaching the node, stop it for the set stop time
            while (refCamera.getTrackingObject() == gameObject && Vector2.Distance(transform.position, trackNode.Position) > 0.1f)
            {
                refRB.linearVelocity = direction * speed;
                yield return null;
            }
            refRB.linearVelocity = Vector2.zero;
            // change the camera's zoom if final node
            if (i == TrackNodes.Count - 1)
                refCamera.ChangeCameraZoom(EndZoom);
            // Wait for end time
            yield return new WaitForSeconds(trackNode.camPauseTime);
        }
        // Reset the camera to follow the original object
        refCamera.SetTrackingObject(trackingObj);
        refCamera.SetOffset(camOffset);
        // Pull up ui after last node
        PullUpTimeAllocationUI();
    }

    private void PullUpTimeAllocationUI()
    {
        if (pullUpTimeBankUIAfterPan)
        {
            TimeBank refTimeBank = FindFirstObjectByType<TimeBank>();
            if (refTimeBank != null)
                refTimeBank.ActivateUI();
        }
    }
}
