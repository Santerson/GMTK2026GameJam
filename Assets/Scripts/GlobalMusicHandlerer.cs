using UnityEngine;

public class GlobalMusicHandlerer : MonoBehaviour
{
    [SerializeField] private AudioSource mainMenuMusicSource;
    [SerializeField] private AudioSource gameMusic;
    [SerializeField] private AudioSource gameAmbientMusic;
    private void Start()
    {
        GlobalMusicHandlerer[] objs = FindObjectsByType<GlobalMusicHandlerer>(FindObjectsSortMode.None);
        if (objs.Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlayMainMenuMusic()
    {
        gameAmbientMusic.Stop();
    }

    public void PlayGameMusic()
    {
        gameAmbientMusic.Play();
    }
}
