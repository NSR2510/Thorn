using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsController : MonoBehaviour
{
    [Header("UI Controls")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Button muteButton;

    [Header("UI Value Texts")]
    [SerializeField] private TextMeshProUGUI masterValueText;
    [SerializeField] private TextMeshProUGUI muteButtonText;

    private const string MasterVolumeKey = "MasterVolume";
    private const string IsMutedKey = "IsMuted";

    private bool isMuted = false;
    private float preMuteVolume = 0.75f;

    private void Start()
    {
        // Load saved values
        float masterVol = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);
        isMuted = PlayerPrefs.GetInt(IsMutedKey, 0) == 1;

        // Initialize slider
        if (masterSlider != null)
        {
            masterSlider.value = masterVol;
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        // Initialize mute button
        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleMute);
        }

        // Apply volumes on start
        ApplyVolumes();
        UpdateMuteButtonUI();
    }

    private void OnDestroy()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (muteButton != null) muteButton.onClick.RemoveListener(ToggleMute);
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (isMuted && value > 0f)
        {
            // Unmute if user drags slider above 0
            isMuted = false;
            PlayerPrefs.SetInt(IsMutedKey, 0);
            UpdateMuteButtonUI();
        }

        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();

        if (masterValueText != null)
        {
            masterValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }

        ApplyVolumes();
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt(IsMutedKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        if (isMuted)
        {
            if (masterSlider != null)
            {
                preMuteVolume = masterSlider.value;
                masterSlider.value = 0f;
            }
        }
        else
        {
            if (masterSlider != null)
            {
                // Restore pre-mute volume, default to 0.75f if it was 0
                masterSlider.value = preMuteVolume > 0.01f ? preMuteVolume : 0.75f;
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

        if (isMuted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = master;
        }
    }
}