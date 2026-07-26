using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHPBarUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject uiContainer;
    [SerializeField] private Slider bossSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI bossHealthText;
    [SerializeField] private Image fillImage;

    [Header("Settings")]
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private Color fullColor = new Color(0.85f, 0.15f, 0.15f, 1f); // Red
    [SerializeField] private Color lowColor = new Color(0.5f, 0.05f, 0.05f, 1f); // Dark Crimson
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.15f;

    private Boss_HP activeBoss;
    private float targetValue;
    private float lastHealth;
    private float flashTimer;

    private void Awake()
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }
        else
        {
            uiContainer = gameObject;
            uiContainer.SetActive(false);
        }
    }

    private void Update()
    {
        if (activeBoss == null)
        {
            FindActiveBoss();
            return;
        }

        // Smoothly lerp slider value
        if (bossSlider != null)
        {
            bossSlider.value = Mathf.Lerp(bossSlider.value, targetValue, Time.deltaTime * lerpSpeed);

            // Handle flash and color Lerp
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
                float ratio = bossSlider.value / bossSlider.maxValue;
                fillImage.color = Color.Lerp(lowColor, fullColor, ratio);
            }
        }
    }

    private void FindActiveBoss()
    {
        var boss = Object.FindAnyObjectByType<Boss_HP>();
        if (boss != null && !boss.IsDead)
        {
            BindToBoss(boss);
        }
    }

    private void BindToBoss(Boss_HP boss)
    {
        activeBoss = boss;
        
        if (bossNameText != null)
        {
            string cleanedName = boss.gameObject.name.Replace("(Clone)", "").Trim().ToUpper();
            bossNameText.text = cleanedName;
        }

        if (bossSlider != null)
        {
            bossSlider.maxValue = boss.MaxHealth;
            bossSlider.value = boss.CurrentHealth;
        }

        targetValue = boss.CurrentHealth;
        lastHealth = boss.CurrentHealth;

        if (fillImage == null && bossSlider != null)
        {
            var fillRect = bossSlider.fillRect;
            if (fillRect != null)
            {
                fillImage = fillRect.GetComponent<Image>();
            }
        }

        UpdateText(boss.CurrentHealth, boss.MaxHealth);

        // Bind events
        boss.onHealthChanged.AddListener(OnBossHealthChanged);
        boss.onDeath.AddListener(OnBossDeath);

        // Show UI
        if (uiContainer != null)
        {
            uiContainer.SetActive(true);
        }
    }

    private void UnbindBoss()
    {
        if (activeBoss != null)
        {
            activeBoss.onHealthChanged.RemoveListener(OnBossHealthChanged);
            activeBoss.onDeath.RemoveListener(OnBossDeath);
            activeBoss = null;
        }
    }

    private void OnBossHealthChanged(float currentHealth, float maxHealth)
    {
        targetValue = currentHealth;
        
        if (bossSlider != null)
        {
            bossSlider.maxValue = maxHealth;
        }

        if (currentHealth < lastHealth)
        {
            flashTimer = flashDuration;
        }

        lastHealth = currentHealth;
        UpdateText(currentHealth, maxHealth);
    }

    private void OnBossDeath()
    {
        UnbindBoss();
        
        // Hide UI immediately
        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }
    }

    private void UpdateText(float current, float max)
    {
        if (bossHealthText != null)
        {
            bossHealthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }

    private void OnDestroy()
    {
        UnbindBoss();
    }
}
