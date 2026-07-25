using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CrateButton : MonoBehaviour
{
    [SerializeField] GameObject[] Objects;
    [SerializeField] UnityEvent[] EnableActions;
    [SerializeField] UnityEvent[] DisableActions;
    [SerializeField] AudioSource ButtonDown;
    [SerializeField] AudioSource ButtonUp;
    bool buttonEnabled = false;
    Animator refAnimator;

    List<GameObject> ButtonEligibleCollisions = new List<GameObject>();

    SpriteRenderer refRenderer = null;

    private void Start()
    {
        refRenderer = GetComponent<SpriteRenderer>();
        refAnimator = GetComponent<Animator>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<ButtonEligible>() != null)
        {
            if (!buttonEnabled)
            {
                EnableButton();
                buttonEnabled = true;
            }
            ButtonEligibleCollisions.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<ButtonEligible>() != null && buttonEnabled)
        {
            ButtonEligibleCollisions.Remove(collision.gameObject);
            if (ButtonEligibleCollisions.Count == 0)
            {
                DisableButton();
                buttonEnabled = false;
            }
        }
    }

    void EnableButton()
    {
        refAnimator.SetBool("Active", true);
        // Disable all gameobjects
        foreach (GameObject obj in Objects)
        {
            if (obj != null)
            {
                DisableWall(obj);
            }
        }
        // Invoke all enable actions
        foreach (UnityEvent action in EnableActions)
        {
            action.Invoke();
        }
        ButtonDown.Play();
    }

    void DisableButton()
    {
        refAnimator.SetBool("Active", false);
        // Enable all gameobjects
        foreach (GameObject obj in Objects)
        {
            if (obj != null)
                EnableWall(obj);
        }
        // Invoke all disable actions
        foreach (UnityEvent action in DisableActions)
        {
            action.Invoke();
        }
        ButtonUp?.Play();
    }

    void EnableWall(GameObject obj)
    {
        Animator refAnimator = obj.GetComponent<Animator>();
        Collider2D collider2D = obj.GetComponent<Collider2D>();
        if (refAnimator != null && collider2D != null)
        {
            refAnimator.SetBool("Active", false);
            collider2D.enabled = true;
        }
        else
        {
            Debug.LogError("Make sure to only assign wall objects to buttons!");
            obj.SetActive(true);
        }
    }

    void DisableWall(GameObject obj)
    {
        Animator refAnimator = obj.GetComponent<Animator>();
        Collider2D collider2D = obj.GetComponent<Collider2D>();
        if (refAnimator != null && collider2D != null)
        {
            refAnimator.SetBool("Active", true);
            collider2D.enabled = false;
        }
        else
        {
            Debug.LogError("Make sure to only assign wall objects to buttons!");
            obj.SetActive(false);
        }
    }
}
