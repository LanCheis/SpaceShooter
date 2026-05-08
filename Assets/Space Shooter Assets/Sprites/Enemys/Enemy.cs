using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Combat")]
    public int contactDamage = 1;
    public GameObject bulletPrefab;
    public float minFireInterval = 3f;
    public float maxFireInterval = 7f;

    [Header("Health Bar")]
    public EnemyHealthBar healthBar;

    private int health;
    private int columnIndex;
    private float nextFireTime;
    private EnemySpawner spawner;

    void Start()
    {
        health = maxHealth;
        nextFireTime = Time.time + Random.Range(minFireInterval, maxFireInterval);
        if (healthBar != null)
            healthBar.UpdateBar(1f);
    }

    // Called by EnemySpawner immediately after Instantiate, before Start
    public void Init(int columnIndex, EnemySpawner spawner)
    {
        this.columnIndex = columnIndex;
        this.spawner = spawner;
    }

    void Update()
    {
        // X is fully driven by the spawner's formation — no individual movement logic
        if (spawner != null)
            transform.position = new Vector3(spawner.GetColumnX(columnIndex), transform.position.y, 0f);

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + Random.Range(minFireInterval, maxFireInterval);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;
        Instantiate(bulletPrefab, transform.position, Quaternion.identity);
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (healthBar != null)
            healthBar.UpdateBar((float)health / maxHealth);
        if (health <= 0)
            Die();
    }

    void Die()
    {
        if (spawner != null)
            spawner.OnEnemyDied();
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(contactDamage);
    }
}
