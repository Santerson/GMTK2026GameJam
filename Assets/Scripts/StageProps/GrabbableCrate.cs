using UnityEngine;

public class GrabbableCrate : MonoBehaviour
{
    [SerializeField] GameObject refGrabTooltip;
    [SerializeField] AudioSource grabSound;
    [SerializeField] AudioSource dropSound;
    bool grabbable = false;
    Collider2D PlayerCollider;
    PlayerMovement refPlayer;

    private void Start()
    {
        refPlayer = FindFirstObjectByType<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Dis shet unoptimized as shit its 9pm im eepy
            if (refPlayer.IsHoldingObject)
            {
                return;
            }
            else if (refPlayer.canMove == false)
            {
                return;
            }
            grabbable = true;
            PlayerCollider = collision;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            grabbable = false;
            PlayerCollider = null;
        }
    }

    void Update()
    {
        if (refPlayer.canMove == false)
        {
            grabbable = false;
        }
        if (grabbable)
        {
            refGrabTooltip.SetActive(true);
        }
        else
        {
            refGrabTooltip.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E) && grabbable)
        {
            if (refPlayer.IsHoldingObject)
            {
                return;
            }
            else if (refPlayer.canMove == false)
            {
                return;
            }
            GrabObject(PlayerCollider);
            grabSound.Play();
        }
    }

    private void GrabObject(Collider2D collision)
    {
        refGrabTooltip.SetActive(false);
        // Get the PlayerController component from the player
        PlayerMovement playerController = collision.GetComponent<PlayerMovement>();
        // Check if the player is not already holding an object
        if (playerController != null && !playerController.IsHoldingObject)
        {
            // Call the GrabObject method on the player controller and pass this crate as the object to grab
            playerController.GrabObject(this.gameObject);
        }
    }

    public void PlayDropSound()
    {
        dropSound.Play();
    }
}
