using UnityEngine;

public class PlayUIClickSFX : MonoBehaviour
{
    private void Start()
    {
        PlayUIClickSFX[] playUIClickSFXes = FindObjectsByType<PlayUIClickSFX>(FindObjectsSortMode.None);
        if (playUIClickSFXes.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void PlayUIClick()
    {
        GetComponent<AudioSource>().Play();
    }
}
