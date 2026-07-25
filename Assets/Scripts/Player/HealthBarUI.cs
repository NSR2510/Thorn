using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Health playerHealth;

    [Header("UI Components")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Start()
    {
        // If not manually assigned in Inspector, try to find the Player (Warrior) in the scene
        if (playerHealth == null)
        {
            var warrior = GameObject.Find("Warrior");
            if (warrior != null)
            {
                playerHealth = warrior.GetComponent<Health>();
            }

            if (playerHealth == null)
            {
                playerHealth = Object.FindAnyObjectByType<Health>();
            }
        }

        if (playerHealth != null)
        {
            // Set initial UI state
            UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);

            // Subscribe to health change events
            playerHealth.onHealthChanged.AddListener(UpdateHealthUI);
        }
        else
        {
            Debug.LogWarning("HealthBarUI: No Health component found to track.");
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.RemoveListener(UpdateHealthUI);
        }
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}
