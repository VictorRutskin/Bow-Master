using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArrowDamage : MonoBehaviour
{
    public int damage = 10;
    public float knockbackMultiplier = 1f;
    public LayerMask enemyMask;          // set to your “Enemy” layer

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<Collider2D>().isTrigger = true;   // arrows trigger enemies
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyMask) == 0) return;

        var goblin = other.GetComponent<Goblin>();
        if (goblin != null)
        {
            goblin.TakeDamage(damage, transform.position, knockbackMultiplier);
        }

        // Destroy arrow on hit (or keep flying if you want piercing)
        Destroy(gameObject);
    }
}
