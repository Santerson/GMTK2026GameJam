using UnityEngine;

public class DestroyObjectOnAnimationFinish : MonoBehaviour
{
    public void KillItWithFire()
    {
        Destroy(transform.parent.gameObject);
    }
}
