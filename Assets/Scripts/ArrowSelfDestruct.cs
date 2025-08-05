using UnityEngine;
public class ArrowSelfDestruct : MonoBehaviour
{
    public float lifeTime = 4f;
    void Start() { Destroy(gameObject, lifeTime); }
}
