using UnityEngine;

public class ChangeMusicToMenuOnLoad : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        GlobalMusicCaller obj = FindFirstObjectByType<GlobalMusicCaller>();
        if (obj != null)
        {
            obj.TryPlayMainMenuMusic();
        }
        else
        {
            Debug.LogError("Wont reset main menu music, if started on this scene, this ok!");
        }
    }
}
