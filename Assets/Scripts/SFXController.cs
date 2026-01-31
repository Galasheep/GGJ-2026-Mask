using UnityEngine;

/// <summary>
/// Central controller for UI and gameplay sound effects.
/// Assign clips in the Inspector; leave null to skip that sound.
/// </summary>
public class SFXController : MonoBehaviour
{
    public static SFXController Instance { get; private set; }

    [Header("Clips (assign in Inspector; leave null to skip)")]
    [Tooltip("Clicking on UI buttons (e.g. back, play, menu)")]
    [SerializeField] private AudioClip uiButtonClick;

    [Tooltip("Moving from room to room using the mask feature")]
    [SerializeField] private AudioClip maskRoomSwitch;

    [Tooltip("Removing / closing the mask")]
    [SerializeField] private AudioClip maskRemove;

    [Tooltip("Walking from one switchlist target to another (one chosen at random)")]
    [SerializeField] private AudioClip[] switchListWalkVariations;

    [Tooltip("Clicking on riddle buttons (one chosen at random)")]
    [SerializeField] private AudioClip[] riddleButtonVariations;

    [Tooltip("Picking up items to inventory")]
    [SerializeField] private AudioClip itemPickup;

    [Header("Music")]
    [Tooltip("Main background music (loops)")]
    [SerializeField] private AudioClip mainMusic;
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField] private AudioSource musicSource;

    [Header("Optional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool persistAcrossScenes;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (persistAcrossScenes)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            return;
        }

        Instance = this;
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        if (musicSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != audioSource)
                {
                    musicSource = sources[i];
                    break;
                }
            }
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
        else
        {
            musicSource.loop = true;
        }

        if (playMusicOnStart && mainMusic != null)
        {
            PlayMainMusic();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }

    private static AudioClip PickRandomVariation(AudioClip[] variations)
    {
        if (variations == null || variations.Length == 0)
        {
            return null;
        }

        int attempts = variations.Length;
        while (attempts-- > 0)
        {
            int i = Random.Range(0, variations.Length);
            if (variations[i] != null)
            {
                return variations[i];
            }
        }

        return null;
    }

    /// <summary>Use for generic UI button clicks (back, play, menu).</summary>
    public void PlayUIButtonClick()
    {
        PlayOneShot(uiButtonClick);
    }

    /// <summary>Use when opening the mask / moving to a room via mask.</summary>
    public void PlayMaskRoomSwitch()
    {
        PlayOneShot(maskRoomSwitch);
    }

    /// <summary>Use when closing / removing the mask.</summary>
    public void PlayMaskRemove()
    {
        PlayOneShot(maskRemove);
    }

    /// <summary>Use when navigating from one switchlist target to another (including back). Picks a random variation.</summary>
    public void PlaySwitchListWalk()
    {
        PlayOneShot(PickRandomVariation(switchListWalkVariations));
    }

    /// <summary>Use when the player clicks a riddle button. Picks a random variation.</summary>
    public void PlayRiddleButton()
    {
        PlayOneShot(PickRandomVariation(riddleButtonVariations));
    }

    /// <summary>Use when picking up an item into inventory.</summary>
    public void PlayItemPickup()
    {
        PlayOneShot(itemPickup);
    }

    /// <summary>Start or restart main background music (loops).</summary>
    public void PlayMainMusic()
    {
        if (mainMusic == null || musicSource == null)
        {
            return;
        }

        musicSource.clip = mainMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>Stop main music.</summary>
    public void StopMainMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>Set main music volume (0–1).</summary>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>Whether main music is currently playing.</summary>
    public bool IsMainMusicPlaying => musicSource != null && musicSource.isPlaying;
}
