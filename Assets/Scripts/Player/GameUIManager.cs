using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Pause & Settings UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Tutorial UI Buttons")]
    [SerializeField] private Button tutorialOkButton;

    [Header("Death UI Buttons")]
    [SerializeField] private Button deathTryAgainButton;
    [SerializeField] private Button deathMainMenuButton;

    [Header("Win UI Buttons")]
    [SerializeField] private Button winTryAgainButton;
    [SerializeField] private Button winMainMenuButton;

    [Header("Pause UI Buttons")]
    [SerializeField] private Button pauseResumeButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button pauseExitButton;

    [Header("Settings UI Controls")]
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button settingsMuteButton;
    [SerializeField] private TextMeshProUGUI muteButtonText;

    [Header("BGM Settings")]
    [SerializeField] private AudioClip mainThemeSound;
    private AudioSource bgmSource;

    private UnityEngine.InputSystem.InputAction pauseAction;

    private const string MasterVolumeKey = "MasterVolume";
    private const string IsMutedKey = "IsMuted";

    private bool isMuted = false;
    private float preMuteVolume = 0.75f;
    private bool isPaused = false;

    private Health playerHealth;
    private WaveManager waveManager;

    private void Start()
    {
        // Load saved values for audio settings
        float masterVol = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);
        isMuted = PlayerPrefs.GetInt(IsMutedKey, 0) == 1;

        // Initialize escape pause action in code as a Button type to avoid press/release double-triggering
        pauseAction = new UnityEngine.InputSystem.InputAction("Pause", type: UnityEngine.InputSystem.InputActionType.Button, binding: "<Keyboard>/escape");
        pauseAction.performed += OnPauseAction;
        pauseAction.Enable();

        // 1. Play main theme background music
        if (mainThemeSound != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = mainThemeSound;
            bgmSource.loop = true;
            bgmSource.playOnAwake = true;
            bgmSource.ignoreListenerPause = true;
            bgmSource.Play();
            Debug.Log("GameUIManager: Started playing main theme background music.");
        }

        // Apply saved volume settings on startup
        ApplyVolumes();

        // 2. Subscribe to Player's Death Event
        playerHealth = Object.FindAnyObjectByType<Health>();
        if (playerHealth != null)
        {
            playerHealth.onDeath.AddListener(OnPlayerDeath);
        }
        else
        {
            Debug.LogWarning("GameUIManager: Player Health component not found in scene.");
        }

        // 3. Subscribe to WaveManager's Win Event
        waveManager = Object.FindAnyObjectByType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.onGameWon.AddListener(OnGameWon);
        }
        else
        {
            Debug.LogWarning("GameUIManager: WaveManager not found in scene.");
        }

        // 4. Setup Button Listeners
        if (tutorialOkButton != null) tutorialOkButton.onClick.AddListener(DismissTutorial);
        if (deathTryAgainButton != null) deathTryAgainButton.onClick.AddListener(RestartGame);
        if (deathMainMenuButton != null) deathMainMenuButton.onClick.AddListener(LoadMainMenu);
        if (winTryAgainButton != null) winTryAgainButton.onClick.AddListener(RestartGame);
        if (winMainMenuButton != null) winMainMenuButton.onClick.AddListener(LoadMainMenu);

        // Pause / Settings listeners
        if (pauseResumeButton != null) pauseResumeButton.onClick.AddListener(ResumeGame);
        if (pauseSettingsButton != null) pauseSettingsButton.onClick.AddListener(OpenSettings);
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(CloseSettings);
        if (pauseExitButton != null) pauseExitButton.onClick.AddListener(LoadMainMenu);

        if (volumeSlider != null)
        {
            volumeSlider.value = masterVol;
            volumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (settingsMuteButton != null)
        {
            settingsMuteButton.onClick.AddListener(ToggleMute);
        }

        // 5. Initial UI State: Show Tutorial & Pause Game
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game for tutorial
        }
        else
        {
            Time.timeScale = 1f;
        }

        if (deathPanel != null) deathPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        UpdateMuteButtonUI();
    }

    private void OnDestroy()
    {
        if (pauseAction != null)
        {
            pauseAction.Disable();
            pauseAction.Dispose();
        }

        if (playerHealth != null)
        {
            playerHealth.onDeath.RemoveListener(OnPlayerDeath);
        }
        if (waveManager != null)
        {
            waveManager.onGameWon.RemoveListener(OnGameWon);
        }

        if (volumeSlider != null) volumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (settingsMuteButton != null) settingsMuteButton.onClick.RemoveListener(ToggleMute);
        if (pauseResumeButton != null) pauseResumeButton.onClick.RemoveListener(ResumeGame);
        if (pauseSettingsButton != null) pauseSettingsButton.onClick.RemoveListener(OpenSettings);
        if (settingsBackButton != null) settingsBackButton.onClick.RemoveListener(CloseSettings);
        if (pauseExitButton != null) pauseExitButton.onClick.RemoveListener(LoadMainMenu);
    }

    private void OnPauseAction(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        // Do not allow pausing if other overlay panels are active
        if (tutorialPanel != null && tutorialPanel.activeSelf) return;
        if (deathPanel != null && deathPanel.activeSelf) return;
        if (winPanel != null && winPanel.activeSelf) return;

        // Don't pause if an elemental selection is currently active
        ElementalSelectionUI selectionUI = UnityEngine.Object.FindAnyObjectByType<ElementalSelectionUI>();
        if (selectionUI != null && selectionUI.gameObject.activeSelf) return;

        BossUpgradeUI upgradeUI = UnityEngine.Object.FindAnyObjectByType<BossUpgradeUI>();
        if (upgradeUI != null && upgradeUI.gameObject.activeSelf) return;

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void Update()
    {
        // Direct hardware key fallback if the action map is blocked/disabled
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Only toggle if the action didn't already fire in the same frame to prevent double-toggling
            if (pauseAction == null || !pauseAction.enabled || !pauseAction.triggered)
            {
                TogglePause();
            }
        }
    }

    // --- Pause & Settings Callbacks ---

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        Debug.Log("Game paused.");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        Debug.Log("Game resumed.");
    }

    public void OpenSettings()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (isMuted && value > 0f)
        {
            isMuted = false;
            PlayerPrefs.SetInt(IsMutedKey, 0);
            UpdateMuteButtonUI();
        }

        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();

        ApplyVolumes();
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt(IsMutedKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        if (isMuted)
        {
            if (volumeSlider != null)
            {
                preMuteVolume = volumeSlider.value;
                volumeSlider.value = 0f;
            }
        }
        else
        {
            if (volumeSlider != null)
            {
                volumeSlider.value = preMuteVolume > 0.01f ? preMuteVolume : 0.75f;
            }
        }

        UpdateMuteButtonUI();
        ApplyVolumes();
    }

    private void UpdateMuteButtonUI()
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = isMuted ? "UNMUTE" : "MUTE";
        }
    }

    public void ApplyVolumes()
    {
        float master = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);
        bool muted = PlayerPrefs.GetInt(IsMutedKey, 0) == 1;
        if (muted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = master;
        }
    }

    // --- Button Callbacks ---

    public void DismissTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        Time.timeScale = 1f; // Resume/start gameplay
        Debug.Log("Tutorial dismissed. Gameplay started.");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainGame");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // --- Event Listeners ---

    private void OnPlayerDeath()
    {
        Time.timeScale = 0f; // Pause the game on death
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
        Debug.Log("Player died. Death UI activated.");
    }

    private void OnGameWon()
    {
        Time.timeScale = 0f; // Pause the game on win
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        Debug.Log("Game won. Win UI activated.");
    }
}
