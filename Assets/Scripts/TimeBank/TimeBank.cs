using UnityEngine;
using TMPro;

public class TimeBank : MonoBehaviour
{
    [Header("Time Bank")]
    [Tooltip("The time the player has on this stage")]
        [SerializeField] uint MaxTime = 12;
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
        [SerializeField] TMP_Text JumpText;

    [Header("Audio References")]
    [Tooltip("The audio source for the time bank")]
        [SerializeField] AudioSource SFX_Allocate;
    [Tooltip("The audio source for the time bank")]
        [SerializeField] AudioSource SFX_Deallocate;
    [SerializeField] AudioSource FadeInSFX;
    [SerializeField] AudioSource FadeOutSFX;

    Vector2 CurrentCameraOffset = Vector2.zero;

    float TimeLeftToAllocate = 0f;
    float allocatedTimeLeft = 0;
    float allocatedTimeRight = 0;
    float allocatedTimeJump = 0;
    public bool IsUIActive { get; private set; } = false;

    AllocatedTimeStorage refTimeStorage;

    private void Awake()
    {
        refTimeStorage = FindFirstObjectByType<AllocatedTimeStorage>();
        ActivateUI();
    }

    /// <summary>
    /// Enables the UI for selecting the time bank and sets the camera offset to the specified position
    /// </summary>
    public void ActivateUI()
    {
        // Pull the selected times from the global object for this stage
        PullSelectedTimes();
        // Calculate the time left to allocate and reallocate the given applied tiems
        CalculateTimeToAllocateLeft();
        ReapplySelectedTimes();
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
        FadeInSFX.Play();
        refCameraMovement.StartCameraZoomin(CameraZoomInScale);
        // Stop the player from moving
        refPlayer.canMove = false;
        IsUIActive = true;
    }

    public void LeftButtonTimeAdd()
    {
        if (TimeLeftToAllocate <= 0f)
            return;
        refPlayer.LeftMovementTimeLeft += 1f;
        SFX_Allocate.Play();
        allocatedTimeLeft += 1f;
        LeftText.text = "" + refPlayer.LeftMovementTimeLeft;
        CalculateTimeToAllocateLeft();
    }

    public void LeftButtonTimeMinus()
    {
        if (TimeLeftToAllocate >= MaxTime || allocatedTimeLeft <= 0)
            return;
        refPlayer.LeftMovementTimeLeft -= 1f;
        SFX_Deallocate.Play();
        allocatedTimeLeft -= 1f;
        LeftText.text = "" + refPlayer.LeftMovementTimeLeft;
        CalculateTimeToAllocateLeft();
    }

    public void RightButtonTimeAdd()
    {
        if (TimeLeftToAllocate <= 0f)
            return;
        refPlayer.RightMovementTimeLeft += 1f;
        SFX_Allocate.Play();
        allocatedTimeRight += 1f;
        RightText.text = "" + refPlayer.RightMovementTimeLeft;
        CalculateTimeToAllocateLeft();
    }

    public void RightButtonTimeMinus()
    {
        if (TimeLeftToAllocate >= MaxTime || allocatedTimeRight <= 0)
            return;
        refPlayer.RightMovementTimeLeft -= 1f;
        SFX_Deallocate.Play();
        allocatedTimeRight -= 1f;
        RightText.text = "" + refPlayer.RightMovementTimeLeft;
        CalculateTimeToAllocateLeft();
    }

    public void JumpButtonTimeAdd()
    {
        if (TimeLeftToAllocate <= 0f)
            return;
        refPlayer.JumpMovementTimeLeft += 1f;
        SFX_Allocate.Play();
        allocatedTimeJump += 1f;
        JumpText.text = "" + refPlayer.JumpMovementTimeLeft;
        CalculateTimeToAllocateLeft();
    }

    public void JumpButtonTimeMinus()
    {
        if (TimeLeftToAllocate >= MaxTime || allocatedTimeJump <= 0)
            return;
        refPlayer.JumpMovementTimeLeft -= 1f;
        SFX_Deallocate.Play();
        allocatedTimeJump -= 1f;
        JumpText.text = "" + refPlayer.JumpMovementTimeLeft;
        CalculateTimeToAllocateLeft();
    }

    /// <summary>
    /// Starts the level if all time has been allocated
    /// </summary>
    public void StartButton()
    {
        if (TimeLeftToAllocate <= 0f)
        {
            // SFXs
            FadeOutSFX.Play();
            FindFirstObjectByType<PlayUIClickSFX>()?.PlayUIClick();
            // Disable the ui
            foreach (GameObject ui in UI)
                ui.SetActive(false);
            // Save the selected times
            SaveSelectedTimes();
            // Reset the camera
            refCameraMovement.SetOffset(CurrentCameraOffset);
            refCameraMovement.StartCameraZoomout();
            // Allow the player to move
            refPlayer.canMove = true;
            IsUIActive = false;
        }
    }

    void CalculateTimeToAllocateLeft()
    {
        TimeLeftToAllocate = MaxTime - allocatedTimeJump - allocatedTimeLeft - allocatedTimeRight;
        timeBankText.text = "Time Bank: " + TimeLeftToAllocate + "/s";
    }

    public void ReapplySelectedTimes()
    {
        refPlayer.JumpMovementTimeLeft = allocatedTimeJump;
        refPlayer.LeftMovementTimeLeft = allocatedTimeLeft;
        refPlayer.RightMovementTimeLeft = allocatedTimeRight;
    }

    /// <summary>
    /// Saves the selected times to the global object for this stage
    /// </summary>
    void SaveSelectedTimes()
    {
        if (refTimeStorage == null)
        {
            Debug.LogWarning("No Time Storage Gameobject found on this scene! This might be because you forgot to put the empty on this scene!");
            return;
        }
        refTimeStorage.allocatedTimeJump = allocatedTimeJump;
        refTimeStorage.allocatedTimeLeft = allocatedTimeLeft;
        refTimeStorage.allocatedTimeRight = allocatedTimeRight;
        refPlayer.MaxJumpMovementTime = allocatedTimeJump;
        refPlayer.MaxLeftMovementTime = allocatedTimeLeft;
        refPlayer.MaxRightMovementTime = allocatedTimeRight;
    }

    /// <summary>
    /// Pulls selected save times from the global object for this stage
    /// </summary>
    void PullSelectedTimes()
    {
        if (refTimeStorage == null)
        {
            Debug.LogWarning("No Time Storage Gameobject found on this scene! This might be because you forgot to put the empty on this scene!");
            return;
        }
        allocatedTimeJump = refTimeStorage.allocatedTimeJump;
        allocatedTimeLeft = refTimeStorage.allocatedTimeLeft;
        allocatedTimeRight = refTimeStorage.allocatedTimeRight;
        RightText.text = "" + allocatedTimeRight;
        LeftText.text = "" + allocatedTimeLeft;
        JumpText.text = "" + allocatedTimeJump;
    }
}
