using UnityEngine;
using TMPro;

public class ElementalHUD : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI elementText;
    [SerializeField] private TextMeshProUGUI chanceText;
    [SerializeField] private TextMeshProUGUI dpsText;

    private PlayerElementalManager.ElementType lastElement = PlayerElementalManager.ElementType.None;
    private float lastChance = -1f;

    private void Start()
    {
        UpdateHUD();
    }

    private void Update()
    {
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        PlayerElementalManager manager = PlayerElementalManager.Instance;
        if (manager == null) return;

        PlayerElementalManager.ElementType element = manager.ActiveElement;
        float chance = manager.ActivationChance;

        if (element != lastElement || chance != lastChance)
        {
            lastElement = element;
            lastChance = chance;

            // 1. Update Element Text and Color
            if (elementText != null)
            {
                switch (element)
                {
                    case PlayerElementalManager.ElementType.None:
                        elementText.text = "ELEMENT: NONE";
                        elementText.color = Color.white;
                        break;
                    case PlayerElementalManager.ElementType.Poison:
                        elementText.text = "ELEMENT: POISON";
                        elementText.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Vibrant Green
                        break;
                    case PlayerElementalManager.ElementType.Dark:
                        elementText.text = "ELEMENT: DARK";
                        elementText.color = new Color(0.6f, 0.2f, 0.8f, 1f); // Vibrant Purple
                        break;
                    case PlayerElementalManager.ElementType.Lightning:
                        elementText.text = "ELEMENT: LIGHTNING";
                        elementText.color = new Color(1f, 0.8f, 0.1f, 1f); // Vibrant Amber/Yellow
                        break;
                }
            }

            // 2. Update Chance Text
            if (chanceText != null)
            {
                if (element == PlayerElementalManager.ElementType.None)
                {
                    chanceText.text = "CHANCE: 0%";
                }
                else
                {
                    chanceText.text = $"CHANCE: {Mathf.RoundToInt(chance * 100f)}%";
                }
            }

            // 3. Update DPS Text
            if (dpsText != null)
            {
                if (element == PlayerElementalManager.ElementType.None)
                {
                    dpsText.text = "DAMAGE: 0/s";
                }
                else
                {
                    dpsText.text = $"DAMAGE: {Mathf.RoundToInt(manager.TickDamage)}/s";
                }
            }
        }
    }
}
