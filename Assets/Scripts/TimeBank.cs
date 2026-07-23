using UnityEngine;
using TMPro;

public class TimeBank : MonoBehaviour
{
    [SerializeField] PlayerMovement pl;

    [SerializeField] float MaxTime = 12f;
    [SerializeField] GameObject UI;
    [SerializeField] TMP_Text text;

    private void Awake()
    {
        Time.timeScale = 0f;
    }
    private void Update()
    {
        text.text = "Time Bank: " + MaxTime + "/s";
    }

    public void LeftButtonTimeAdd()
    {
        if (MaxTime == 0f)
            return;
        pl.LeftMovementTimeLeft += 1f;
        MaxTime -= 1f;
    }

    public void LeftButtonTimeMinus()
    {
        if (MaxTime == 12)
            return;
        pl.LeftMovementTimeLeft -= 1f;
        MaxTime += 1f;
    }

    public void RightButtonTimeAdd()
    {
        if (MaxTime == 0f)
            return;
        pl.RightMovementTimeLeft += 1f;
        MaxTime -= 1f;
    }

    public void RightButtonTimeMinus()
    {
        if (MaxTime == 12)
            return;
        pl.RightMovementTimeLeft -= 1f;
        MaxTime += 1f;
    }

    public void JumpButtonTimeAdd()
    {
        if (MaxTime == 0f)
            return;
        pl.JumpMovementTimeLeft += 1f;
        MaxTime -= 1f;
    }

    public void JumpButtonTimeMinus()
    {
        if(MaxTime == 12)
            return;
        pl.JumpMovementTimeLeft -= 1f;
        MaxTime += 1f;
    }

    public void StartButton()
    {
        if (MaxTime == 0f)
        {
            Time.timeScale = 1.0f;
            UI.SetActive(false);
        }
    }
}
