using UnityEngine;

public class AllocatedTimeStorage : MonoBehaviour
{
    public float allocatedTimeLeft = 0f;
    public float allocatedTimeRight = 0f;
    public float allocatedTimeJump = 0f;

    // Dont destory on load stuff
    private void Awake()
    {
        AllocatedTimeStorage[] objs = FindObjectsByType<AllocatedTimeStorage>(FindObjectsSortMode.None);
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
