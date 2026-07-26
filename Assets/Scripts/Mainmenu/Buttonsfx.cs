using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this directly to each Button GameObject (PlayButton, ExitButton,
/// SettingsButton, etc.). Assign a hover clip and/or click clip in the
/// Inspector. Uses PlayOneShot so sounds can overlap/retrigger cleanly
/// and won't cut off if clicked rapidly.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sound Clips")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Make sure this AudioSource doesn't auto-play or loop on its own;
        // it's only used to fire one-shot SFX on demand.
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, volume);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
}