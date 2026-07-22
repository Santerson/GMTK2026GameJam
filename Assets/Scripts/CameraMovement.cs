using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] GameObject TargetObject;
    [SerializeField] float FollowSpeed = 0.1f;
    [SerializeField] Vector2 offset = new Vector2(0, 4f);

    private void OnDrawGizmos()
    {
        if (TargetObject != null)
        {
            Vector2 targetPos = (Vector2)TargetObject.transform.position + offset;
            Debug.DrawLine(TargetObject.transform.position, targetPos, Color.red);
            Debug.DrawLine(targetPos, transform.position, Color.green);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }
}
