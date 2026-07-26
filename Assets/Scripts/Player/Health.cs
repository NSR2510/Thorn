using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Immunity Settings")]
    [SerializeField] private float immunityDuration = 1.0f;
    private float immunityTimer = 0f;
    private SpriteRenderer spriteRenderer;

    [Header("Audio")]
    [SerializeField] private AudioClip gruntSound;
    private AudioSource audioSource;

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged; // passes currentHealth, maxHealth
    public UnityEvent onDeath;

    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool IsImmune => immunityTimer > 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (immunityTimer > 0f)
        {
            immunityTimer -= Time.deltaTime;

            // Flash effect to visualize immunity period
            if (spriteRenderer != null)
            {
                float flashInterval = 0.1f;
                bool isVisible = (Mathf.FloorToInt(Time.time / flashInterval) % 2 == 0);
                spriteRenderer.enabled = isVisible;
            }
        }
        else
        {
            // Ensure sprite is visible when immunity ends
            if (spriteRenderer != null && !spriteRenderer.enabled)
            {
                spriteRenderer.enabled = true;
            }
        }
    }


    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    /// <param name="damage">Amount of damage to take.</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        if (immunityTimer > 0f) return; // Ignore damage during immunity window

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (audioSource != null && gruntSound != null)
        {
            audioSource.PlayOneShot(gruntSound);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            // Activate 1-second immunity period
            immunityTimer = immunityDuration;
        }
    }

    /// <summary>
    /// Restore health to this entity.
    /// </summary>
    /// <param name="amount">Amount of health to restore.</param>
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Increase max health and heal the player by the same amount.
    /// </summary>
    /// <param name="amount">Amount of max health to add.</param>
    public void IncreaseMaxHealth(float amount)
    {
        if (isDead) return;

        maxHealth += amount;
        currentHealth += amount;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} max health increased to {maxHealth}. Current health: {currentHealth}");
    }

    private void Die()
    {
        isDead = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        onDeath?.Invoke();
        Debug.Log($"{gameObject.name} has died.");
    }
}
