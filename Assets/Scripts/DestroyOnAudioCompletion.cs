using UnityEngine;

public class DestroyOnAudioCompletion : MonoBehaviour
{
    AudioSource refAudioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        refAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!refAudioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
