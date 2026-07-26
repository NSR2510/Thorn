using UnityEngine;
using UnityEngine.UI;
using System;

public class BossUpgradeUI : MonoBehaviour
{
    [Header("UI Panel GameObject")]
    [SerializeField] private GameObject upgradePanel;

    [Header("Buttons")]
    [SerializeField] private Button damageButton;
    [SerializeField] private Button healthButton;
    [SerializeField] private Button testYourWillButton;

    private PlayerCombat playerCombat;
    private Health playerHealth;
    private Action onUpgradeSelected;

    private void Start()
    {
        // Find player components dynamically if not assigned
        playerCombat = UnityEngine.Object.FindAnyObjectByType<PlayerCombat>();
        playerHealth = UnityEngine.Object.FindAnyObjectByType<Health>();

        // Bind button listeners
        if (damageButton != null)
        {
            damageButton.onClick.AddListener(SelectDamageUpgrade);
        }
        if (healthButton != null)
        {
            healthButton.onClick.AddListener(SelectHealthUpgrade);
        }
        if (testYourWillButton != null)
        {
            testYourWillButton.onClick.AddListener(SelectTestYourWill);
        }

        // Keep panel disabled initially
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    public void ShowUpgradeScreen(Action callback)
    {
        onUpgradeSelected = callback;

        // Ensure we have reference to player components
        if (playerCombat == null) playerCombat = UnityEngine.Object.FindAnyObjectByType<PlayerCombat>();
        if (playerHealth == null) playerHealth = UnityEngine.Object.FindAnyObjectByType<Health>();

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
        }

        // Pause the game
        Time.timeScale = 0f;
    }

    private void SelectDamageUpgrade()
    {
        if (playerCombat != null)
        {
            playerCombat.IncreaseDamage(5f);
        }
        else
        {
            Debug.LogWarning("BossUpgradeUI: PlayerCombat not found to apply damage upgrade.");
        }

        CompleteUpgrade();
    }

    private void SelectHealthUpgrade()
    {
        if (playerHealth != null)
        {
            playerHealth.IncreaseMaxHealth(50f);
        }
        else
        {
            Debug.LogWarning("BossUpgradeUI: Player Health not found to apply HP upgrade.");
        }

        CompleteUpgrade();
    }

    private void SelectTestYourWill()
    {
        WaveManager waveMgr = UnityEngine.Object.FindAnyObjectByType<WaveManager>();
        if (waveMgr != null)
        {
            // Close panel
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }

            // Resume gameplay time
            Time.timeScale = 1f;

            // Trigger Special Infinite Mode (Wave 99)
            waveMgr.StartSpecialMode();
        }
        else
        {
            Debug.LogError("BossUpgradeUI: WaveManager not found to start Special Mode!");
        }
    }

    private void CompleteUpgrade()
    {
        // Hide panel
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        // Resume time
        Time.timeScale = 1f;

        // Trigger callback to resume next wave
        onUpgradeSelected?.Invoke();
    }
}
