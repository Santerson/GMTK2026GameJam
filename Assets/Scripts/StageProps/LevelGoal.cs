using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    [Tooltip("The index of the CURRENT level. completing this level will load this level index + 1")]
    public uint LevelIndex = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Epic gamer dub
            collision.GetComponent<PlayerMovement>().GamerWin();
        }
    }
}
