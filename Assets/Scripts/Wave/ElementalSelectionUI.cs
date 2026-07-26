using UnityEngine;
using UnityEngine.UI;
using System;

public class ElementalSelectionUI : MonoBehaviour
{
    [Header("UI Panel GameObject")]
    [SerializeField] private GameObject selectionPanel;

    [Header("Buttons")]
    [SerializeField] private Button poisonButton;
    [SerializeField] private Button darkButton;
    [SerializeField] private Button lightningButton;

    private Action onSelectionComplete;

    private void Start()
    {
        // Bind button listeners
        if (poisonButton != null)
        {
            poisonButton.onClick.AddListener(() => SelectElement(PlayerElementalManager.ElementType.Poison));
        }
        if (darkButton != null)
        {
            darkButton.onClick.AddListener(() => SelectElement(PlayerElementalManager.ElementType.Dark));
        }
        if (lightningButton != null)
        {
            lightningButton.onClick.AddListener(() => SelectElement(PlayerElementalManager.ElementType.Lightning));
        }
    }

    public void ShowSelectionScreen(Action callback)
    {
        onSelectionComplete = callback;

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
        }

        // Pause the game
        Time.timeScale = 0f;
    }

    private void SelectElement(PlayerElementalManager.ElementType element)
    {
        if (PlayerElementalManager.Instance != null)
        {
            PlayerElementalManager.Instance.ActiveElement = element;
            Debug.Log($"ElementalSelectionUI: Player selected element: {element}");
        }
        else
        {
            Debug.LogError("ElementalSelectionUI: PlayerElementalManager not found!");
        }

        // Hide panel
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }

        // Resume time
        Time.timeScale = 1f;

        // Trigger callback to continue (e.g. show boss upgrades or start cooldown)
        onSelectionComplete?.Invoke();
    }
}
