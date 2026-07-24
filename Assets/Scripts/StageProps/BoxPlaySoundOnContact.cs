using UnityEngine;

public class BoxPlaySoundOnContact : MonoBehaviour
{
    [SerializeField] AudioSource BumpSound;

    // This is to prevent instant sounds
    float timeUntilPlay = 1f;

    private void Update()
    {
        timeUntilPlay -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (timeUntilPlay > 0f) return;
        BumpSound.Play();
    }
}
