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

    [Header("Tutorial UI Buttons")]
    [SerializeField] private Button tutorialOkButton;

    [Header("Death UI Buttons")]
    [SerializeField] private Button deathTryAgainButton;
    [SerializeField] private Button deathMainMenuButton;

    [Header("Win UI Buttons")]
    [SerializeField] private Button winTryAgainButton;
    [SerializeField] private Button winMainMenuButton;

    [Header("BGM Settings")]
    [SerializeField] private AudioClip mainThemeSound;
    private AudioSource bgmSource;

    private const string MasterVolumeKey = "MasterVolume";
    private const string IsMutedKey = "IsMuted";

    private Health playerHealth;
    private WaveManager waveManager;

    private void Start()
    {
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
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.onDeath.RemoveListener(OnPlayerDeath);
        }
        if (waveManager != null)
        {
            waveManager.onGameWon.RemoveListener(OnGameWon);
        }
    }

    public void ApplyVolumes()
    {
        float master = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);
        bool isMuted = PlayerPrefs.GetInt(IsMutedKey, 0) == 1;
        if (isMuted)
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
