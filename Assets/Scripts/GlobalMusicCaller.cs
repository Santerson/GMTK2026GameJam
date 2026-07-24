using UnityEngine;

public class GlobalMusicCaller : MonoBehaviour
{
    public void TryPlayGameMusic()
    {
        GlobalMusicHandlerer objs = FindFirstObjectByType<GlobalMusicHandlerer>();
        if (objs != null)
        {
            objs.PlayGameMusic();
        }
        else
        {
            Debug.LogWarning("GlobalMusicHandlerer not found in the scene. If you did not run through the main menu, this is ok! No music will play.");
        }
    }

    public void TryPlayMainMenuMusic()
    {
        GlobalMusicHandlerer objs = FindFirstObjectByType<GlobalMusicHandlerer>();
        if (objs != null)
        {
            objs.PlayMainMenuMusic();
        }
        else
        {
            Debug.LogWarning("GlobalMusicHandlerer not found in the scene. If you did not run through the main menu, this is ok! No music will play.");
        }
    }
}
