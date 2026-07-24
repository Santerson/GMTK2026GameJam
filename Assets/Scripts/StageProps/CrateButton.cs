using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CrateButton : MonoBehaviour
{
    [SerializeField] bool EnableColorSwaps = false;
    [SerializeField] GameObject[] Objects;
    [SerializeField] UnityEvent[] EnableActions;
    [SerializeField] UnityEvent[] DisableActions;
    [SerializeField] AudioSource ButtonDown;
    [SerializeField] AudioSource ButtonUp;

    List<GameObject> ButtonEligibleCollisions = new List<GameObject>();

    SpriteRenderer refRenderer = null;

    private void Start()
    {
        refRenderer = GetComponent<SpriteRenderer>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<ButtonEligible>() != null)
        {
            EnableButton();
            ButtonEligibleCollisions.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<ButtonEligible>() != null)
        {
            ButtonEligibleCollisions.Remove(collision.gameObject);
            if (ButtonEligibleCollisions.Count == 0)
                DisableButton();
        }
    }

    void EnableButton()
    {
        if (EnableColorSwaps) refRenderer.color = Color.green;
        // Disable all gameobjects
        foreach (GameObject obj in Objects)
        {
            obj.SetActive(false);
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
        if (EnableColorSwaps) refRenderer.color = Color.red;
        // Enable all gameobjects
        foreach (GameObject obj in Objects)
        {
            if (obj != null)
                obj.SetActive(true);
        }
        // Invoke all disable actions
        foreach (UnityEvent action in DisableActions)
        {
            action.Invoke();
        }
        ButtonUp.Play();
    }
}
