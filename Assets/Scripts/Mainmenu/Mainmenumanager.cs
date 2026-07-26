using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this script to an empty GameObject in your Main Menu scene
/// (e.g. name it "MenuManager"). Then hook up the button OnClick()
/// events in the Inspector to the public methods below.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names (must match names in Build Settings)")]
    [SerializeField] private string gameSceneName = "MainGame";
    [SerializeField] private string settingsSceneName = "SettingsScene";

    [Header("Optional: Settings Panel Instead of a Scene")]
    [Tooltip("If you use an in-menu Settings panel instead of a separate scene, assign it here and leave settingsSceneName blank.")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Menu Music")]
    [Tooltip("Drag your MusicPlayer's AudioSource here so it can be stopped when Play Game is clicked.")]
    [SerializeField] private AudioSource menuMusic;

    // --- PLAY GAME ---
    public void PlayGame()
    {
        if (menuMusic != null)
        {
            menuMusic.Stop();
        }

        Debug.Log("Loading game scene: " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }

    // --- SETTINGS ---
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            // In-menu panel approach
            settingsPanel.SetActive(true);
        }
        else if (!string.IsNullOrEmpty(settingsSceneName))
        {
            // Separate scene approach
            SceneManager.LoadScene(settingsSceneName);
        }
        else
        {
            Debug.LogWarning("No settings panel or scene assigned!");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // --- EXIT GAME ---
    public void ExitGame()
    {
        Debug.Log("Exiting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}