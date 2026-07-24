using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RestoreAnchor : MonoBehaviour
{
    [SerializeField] bool BringUpUI = false;
    [SerializeField] bool repeatable = false;
    [SerializeField] Color disableColorTint = Color.gray;
    [SerializeField] GameObject popupUI;
    bool activatable = false;
    bool deactivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !deactivated)
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
        GetComponent<SpriteRenderer>().color *= disableColorTint;
        deactivated = true;
    }

    void RestoreAnchorFunction()
    {
        // Logic to restore the anchor goes here
        if (BringUpUI)
        {
            FindFirstObjectByType<TimeBank>().ActivateUI();
            if (!repeatable)
            {
                Disable();
            }
        }
        else
        {
            FindFirstObjectByType<TimeBank>().ReapplySelectedTimes();
            StartCoroutine(waitForPlayerToFinish());
        }
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
