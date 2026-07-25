using UnityEngine;
using UnityEngine.UI;

public class ChangeSPriteOnMouseover : MonoBehaviour
{
    [SerializeField] Sprite mouseOffSprite;
    [SerializeField] Sprite mouseOnSprite;

    Image refRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        refRenderer = GetComponent<Image>();
    }

    private void OnMouseEnter()
    {
        if (refRenderer != null)
        {
            refRenderer.sprite = mouseOnSprite;
        }
    }

    private void OnMouseExit()
    {
        if (refRenderer != null)
        {
            refRenderer.sprite = mouseOffSprite;
        }
    }
}
