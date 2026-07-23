using UnityEngine;
using TMPro;

public class TimeBank : MonoBehaviour
{
    [Header("Time Bank")]
    [Tooltip("The time the player has on this stage")]
        [SerializeField] float MaxTime = 12f;
    [Header("Camera Movement")]
    [Tooltip("The offset the camera will be at while this menu is open")]
        [SerializeField] Vector2 CameraOffsetPosition = new(0, 3);
    [Tooltip("The camera zoom in scale for the camera while this menu is open")]
        [SerializeField] float CameraZoomInScale = 3f;

    [Header("References")]
    [Tooltip("Reference to the player movement script")]
        [SerializeField] PlayerMovement refPlayer;
    [Tooltip("Reference to the camera movement script")]
        [SerializeField] CameraMovement refCameraMovement;
    [Tooltip("The UI for the time bank (this will be set inactive)")]
        [SerializeField] GameObject[] UI;
    [Tooltip("The text for the time bank")]
        [SerializeField] TMP_Text timeBankText;
    [Header("Text References")]
    [Tooltip("The text for the left movement time")]
        [SerializeField] TMP_Text LeftText;
    [Tooltip("The text for the right movement time")]
        [SerializeField] TMP_Text RightText;
    [Tooltip("The text for the jump movement time")]
    [   SerializeField] TMP_Text JumpText;

    Vector2 CurrentCameraOffset = Vector2.zero;

    private void Awake()
    {
        ActivateUI();
    }

    private void Update()
    {
        timeBankText.text = "Time Bank: " + MaxTime + "/s";
    }

    /// <summary>
    /// Enables the UI for selecting the time bank and sets the camera offset to the specified position
    /// </summary>
    public void ActivateUI()
    {
        // Get a reference to the reference camera
        if (refCameraMovement == null)
            refCameraMovement = Camera.main.GetComponent<CameraMovement>();
        // Set the offset properly
        CurrentCameraOffset = refCameraMovement.GetOffset();
        refCameraMovement.SetOffset(CameraOffsetPosition);
        // Enable the ui
        foreach (GameObject ui in UI)
            ui.SetActive(true);
        // Set the zoom for the camera
        refCameraMovement.StartCameraZoomin(CameraZoomInScale);
        // Stop the player from moving
        refPlayer.canMove = false;
    }

    public void LeftButtonTimeAdd()
    {
        if (MaxTime == 0f)
            return;
        refPlayer.LeftMovementTimeLeft += 1f;
        LeftText.text = "" + refPlayer.LeftMovementTimeLeft;
        MaxTime -= 1f;
    }

    public void LeftButtonTimeMinus()
    {
        if (MaxTime == 12)
            return;
        refPlayer.LeftMovementTimeLeft -= 1f;
        LeftText.text = "" + refPlayer.LeftMovementTimeLeft;
        MaxTime += 1f;
    }

    public void RightButtonTimeAdd()
    {
        if (MaxTime == 0f)
            return;
        refPlayer.RightMovementTimeLeft += 1f;
        RightText.text = "" + refPlayer.RightMovementTimeLeft;
        MaxTime -= 1f;
    }

    public void RightButtonTimeMinus()
    {
        if (MaxTime == 12)
            return;
        refPlayer.RightMovementTimeLeft -= 1f;
        RightText.text = "" + refPlayer.RightMovementTimeLeft;
        MaxTime += 1f;
    }

    public void JumpButtonTimeAdd()
    {
        if (MaxTime == 0f)
            return;
        refPlayer.JumpMovementTimeLeft += 1f;
        RightText.text = "" + refPlayer.JumpMovementTimeLeft;
        MaxTime -= 1f;
    }

    public void JumpButtonTimeMinus()
    {
        if(MaxTime == 12)
            return;
        RightText.text = "" + refPlayer.JumpMovementTimeLeft;
        MaxTime += 1f;
    }

    public void StartButton()
    {
        if (MaxTime == 0f)
        {
            // Disable the ui
            foreach (GameObject ui in UI)
                ui.SetActive(false);
            // Reset the camera
            refCameraMovement.SetOffset(CurrentCameraOffset);
            refCameraMovement.StartCameraZoomout();
            // Allow the player to move
            refPlayer.canMove = true;
        }
    }
}
