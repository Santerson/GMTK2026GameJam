using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RestoreAnchor : MonoBehaviour
{
    [SerializeField] bool BringUpUI = false;
    [SerializeField] bool repeatable = false;
    [SerializeField] GameObject popupUI;
    [SerializeField] AudioSource sfx;
    bool activatable = false;
    bool deactivated = false;
    Animator RefAnimator;
    PlayerMovement refPlayer;

    private void Start()
    {
        RefAnimator = GetComponent<Animator>();
        refPlayer = FindFirstObjectByType<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !deactivated && refPlayer.canMove)
        {
            activatable = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !deactivated)
        {
            activatable = false;
        }
    }

    private void Update()
    {
        if (activatable && popupUI != null)
        {
            popupUI.SetActive(true);
        }
        else if (popupUI != null)
        {
            popupUI.SetActive(false);
        }
        if (activatable && Input.GetKeyDown(KeyCode.F))
        {
            activatable = false;
            RestoreAnchorFunction();
        }
    }

    void Disable()
    {
        deactivated = true;
        RefAnimator.SetBool("Dead", true);
    }

    void RestoreAnchorFunction()
    {
        sfx.Play();
        // Logic to restore the anchor goes here
        if (BringUpUI)
        {
            FindFirstObjectByType<TimeBank>().ActivateUI();
            FindFirstObjectByType<PlayerMovement>().GetComponent<Rigidbody2D>().linearVelocityX = 0;
            if (!repeatable)
            {
                Disable();
            }
            else
            {
                StartCoroutine(justWaitForPlayerToFinish());
            }
        }
        else
        {
            FindFirstObjectByType<TimeBank>().ReapplySelectedTimes();
            StartCoroutine(waitForPlayerToFinish());
        }
    }

    IEnumerator justWaitForPlayerToFinish()
    {
        yield return new WaitUntil(() => FindFirstObjectByType<TimeBank>().IsUIActive == false);
        activatable = true;
    }

    IEnumerator waitForPlayerToFinish()
    {
        yield return new WaitUntil(() => FindFirstObjectByType<TimeBank>().IsUIActive == false);
        if (!repeatable)
        {
            Disable();
        }
        else
        {
            activatable = true;
        }
    }
}
