using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveUIDisplay : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite waveWordSprite;
    [SerializeField] private Sprite[] digitSprites = new Sprite[10]; // 0 to 9

    [Header("UI Components")]
    [SerializeField] private Image waveWordImage;
    [SerializeField] private Image tensDigitImage;
    [SerializeField] private Image onesDigitImage;
    [SerializeField] private TextMeshProUGUI enemyCounterText;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Awake()
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    public void UpdateWave(int waveNumber)
    {
        // Clamp wave to 0-99 range for display
        int clampedWave = Mathf.Clamp(waveNumber, 0, 99);
        int tens = clampedWave / 10;
        int ones = clampedWave % 10;

        if (waveWordImage != null && waveWordSprite != null)
        {
            waveWordImage.sprite = waveWordSprite;
            waveWordImage.gameObject.SetActive(true);
        }

        if (tensDigitImage != null && digitSprites != null && tens < digitSprites.Length)
        {
            tensDigitImage.sprite = digitSprites[tens];
            tensDigitImage.gameObject.SetActive(true);
        }

        if (onesDigitImage != null && digitSprites != null && ones < digitSprites.Length)
        {
            onesDigitImage.sprite = digitSprites[ones];
            onesDigitImage.gameObject.SetActive(true);
        }
    }

    public void UpdateEnemyCount(int count)
    {
        if (enemyCounterText != null)
        {
            enemyCounterText.text = $"ENEMIES REMAINING: {count}";
        }
    }

    public void UpdateTimer(float secondsLeft, bool active)
    {
        if (timerText != null)
        {
            if (active && secondsLeft > 0f)
            {
                timerText.text = $"NEXT WAVE IN: {Mathf.CeilToInt(secondsLeft)}s";
                timerText.gameObject.SetActive(true);
            }
            else
            {
                timerText.gameObject.SetActive(false);
            }
        }
    }
}
