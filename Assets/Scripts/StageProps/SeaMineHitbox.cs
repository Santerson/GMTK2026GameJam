using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SeaMineHitbox : MonoBehaviour
{
    [Tooltip("The speed the player gets yeeted at if they touch it")]
    [SerializeField] float PlayerYeetSpeed = 10f;
    [SerializeField] float PlayerRotationSpeed = 50f;
    [SerializeField] AudioSource IdleSound;
    [SerializeField] AudioSource kablamoSound;
    PlayerMovement refPlayer;

    private void Start()
    {
        refPlayer = FindFirstObjectByType<PlayerMovement>();
    }
    private void Update()
    {
        if (refPlayer.canMove && !IdleSound.isPlaying)
        {
            IdleSound.Play();
        }
    }

    Vector2 playerVelocity = Vector2.zero;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<ButtonEligible>() != null)
        {
            StartCoroutine(YeetPlayer(collision.gameObject));
        }
    }

    IEnumerator YeetPlayer(GameObject player)
    {
        yield return new WaitForEndOfFrame();
        // Yeet the player
        Collider2D collision = player.GetComponent<Collider2D>();
        Vector2 delta = transform.position - collision.transform.position;
        playerVelocity = delta.normalized * PlayerYeetSpeed * -1;
        collision.GetComponent<Rigidbody2D>().linearVelocity = playerVelocity;
        // make them go spinny spin
        player.GetComponent<Rigidbody2D>().AddTorque(PlayerRotationSpeed * delta.magnitude, ForceMode2D.Impulse);


        // Play kablamo sound
        Instantiate(kablamoSound, transform.position, Quaternion.identity);
        // Destroy mine
        Destroy(transform.parent.gameObject);
    }
}
