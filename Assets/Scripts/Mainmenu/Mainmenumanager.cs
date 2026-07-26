using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names (must match names in Build Settings)")]
    [SerializeField] private string gameSceneName = "Main Game";
    [SerializeField] private string settingsSceneName = "SettingsScene";

    [Header("Optional: Settings Panel Instead of a Scene")]
    [Tooltip("If you use an in-menu Settings panel instead of a separate scene, assign it here and leave settingsSceneName blank.")]
    [SerializeField] private GameObject settingsPanel;

    // --- PLAY GAME ---
    public void PlayGame()
    {
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