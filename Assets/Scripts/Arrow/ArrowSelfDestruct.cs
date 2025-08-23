using UnityEngine;

public class ArrowSelfDestruct : MonoBehaviour
{
    public float lifeTime = 4f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Called by ArrowDamage after a hit to shorten remaining life
    public void Shorten(float newLifeTime)
    {
        // cancel any pending destroy and schedule a new one
        CancelInvoke();
        Destroy(gameObject, newLifeTime);
    }
}
