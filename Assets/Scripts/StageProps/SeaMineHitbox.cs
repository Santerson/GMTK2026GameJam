using System.Collections;
using UnityEngine;

public class SeaMineHitbox : MonoBehaviour
{
    [Tooltip("The speed the player gets yeeted at if they touch it")]
    [SerializeField] float PlayerYeetSpeed = 10f;

    Vector2 playerVelocity = Vector2.zero;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Kill the player
            //yield return new WaitForEndOfFrame();
            // Do something cool here eventaully
            //Destroy(transform.parent.gameObject);
            // Yeet the player the other way
            StartCoroutine(YeetPlayer(collision));
        }

    }

    IEnumerator YeetPlayer(Collider2D collision)
    {
        yield return new WaitForEndOfFrame();
        // Yeet the player
        Vector2 delta = transform.position - collision.transform.position;
        playerVelocity = delta.normalized * PlayerYeetSpeed * -1;
        collision.GetComponent<Rigidbody2D>().linearVelocity = playerVelocity;
        // Hold the camera still
        CameraMovement cam = FindFirstObjectByType<CameraMovement>();
        if (cam != null)
        {
            cam.SetCamTracking(false);
        }
        // Destroy mine
        Destroy(transform.parent.gameObject);
    }
}
