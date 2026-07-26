using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsController : MonoBehaviour
{
    [Header("UI Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("UI Value Texts")]
    [SerializeField] private TextMeshProUGUI masterValueText;
    [SerializeField] private TextMeshProUGUI musicValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SFXVolume";

    private void Start()
    {
        // Load saved values or set defaults (0.75f is a nice default)
        float masterVol = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);
        float musicVol = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
        float sfxVol = PlayerPrefs.GetFloat(SfxVolumeKey, 0.75f);

        // Initialize sliders
        if (masterSlider != null)
        {
            masterSlider.value = masterVol;
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        if (musicSlider != null)
        {
            musicSlider.value = musicVol;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVol;
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        // Initialize text and audio
        UpdateSliderTexts();
        ApplyVolumes();
    }

    private void OnDestroy()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
    }

    public void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();
        if (masterValueText != null)
        {
            masterValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
        ApplyVolumes();
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
        if (musicValueText != null)
        {
            musicValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    public void OnSfxVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        PlayerPrefs.Save();
        if (sfxValueText != null)
        {
            sfxValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    private void UpdateSliderTexts()
    {
        if (masterSlider != null && masterValueText != null)
            masterValueText.text = Mathf.RoundToInt(masterSlider.value * 100f) + "%";
        if (musicSlider != null && musicValueText != null)
            musicValueText.text = Mathf.RoundToInt(musicSlider.value * 100f) + "%";
        if (sfxSlider != null && sfxValueText != null)
            sfxValueText.text = Mathf.RoundToInt(sfxSlider.value * 100f) + "%";
    }

    public void ApplyVolumes()
    {
        float master = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);

        // Control standard AudioListener volume for Master
        AudioListener.volume = master;
    }
}