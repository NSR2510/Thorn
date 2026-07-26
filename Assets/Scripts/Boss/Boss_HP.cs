using UnityEngine;
using UnityEngine.Events;

public class Boss_HP : MonoBehaviour
{
    [Header("Boss Health")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float currentHealth;

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onDeath;

    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Set the boss's max health and fully heal them.
    /// </summary>
    /// <param name="amount">New max health amount.</param>
    public void SetMaxHealth(float amount)
    {
        maxHealth = amount;
        currentHealth = amount;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} max health configured to {maxHealth}");
    }

    private void Die()
    {
        isDead = true;
        onDeath?.Invoke();
    }
}
