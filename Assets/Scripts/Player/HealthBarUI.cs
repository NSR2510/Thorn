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
    [SerializeField] private Image fillImage;

    [Header("Visual Settings")]
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private Color normalColor = new Color(0.85f, 0.15f, 0.15f, 1f); // Red
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.15f;

    private float targetValue;
    private float lastHealth;
    private float flashTimer;

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
            targetValue = playerHealth.CurrentHealth;
            lastHealth = playerHealth.CurrentHealth;

            if (healthSlider != null)
            {
                healthSlider.maxValue = playerHealth.MaxHealth;
                healthSlider.value = playerHealth.CurrentHealth;
            }

            if (fillImage == null && healthSlider != null)
            {
                var fillRect = healthSlider.fillRect;
                if (fillRect != null)
                {
                    fillImage = fillRect.GetComponent<Image>();
                }
            }

            if (fillImage != null)
            {
                fillImage.color = normalColor;
            }

            UpdateHealthUIValues(playerHealth.CurrentHealth, playerHealth.MaxHealth);

            // Subscribe to health change events
            playerHealth.onHealthChanged.AddListener(UpdateHealthUI);
        }
        else
        {
            Debug.LogWarning("HealthBarUI: No Health component found to track.");
        }
    }

    private void Update()
    {
        if (healthSlider != null)
        {
            // Smoothly lerp slider value
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * lerpSpeed);

            // Handle flash and color lerp
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (fillImage != null)
                {
                    fillImage.color = flashColor;
                }
            }
            else if (fillImage != null)
            {
                fillImage.color = normalColor;
            }
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
        targetValue = currentHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
        }

        // Trigger flash when losing HP
        if (currentHealth < lastHealth)
        {
            flashTimer = flashDuration;
        }

        lastHealth = currentHealth;

        UpdateHealthUIValues(currentHealth, maxHealth);
    }

    private void UpdateHealthUIValues(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}


