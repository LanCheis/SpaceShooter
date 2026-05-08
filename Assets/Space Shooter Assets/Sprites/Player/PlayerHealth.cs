using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;
    public float blinkInterval = 0.1f;

    private int currentHealth;
    private bool isInvincible;
    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        StartCoroutine(InvincibilityFrames());
    }

    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        if (sr != null) sr.enabled = true;
        isInvincible = false;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
