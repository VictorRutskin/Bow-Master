using UnityEngine;
public class AutoDestroy : MonoBehaviour
{
    public float maxLife = 2f;
    void Start() { Destroy(gameObject, maxLife); }
}
