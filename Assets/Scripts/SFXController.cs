using System.Collections;
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
    [Tooltip("Dedicated source for per-mask music. Auto-created if missing.")]
    [SerializeField] private AudioSource maskMusicSource;
    [Tooltip("Main music volume multiplier when mask music is playing (0–1).")]
    [SerializeField, Range(0f, 1f)] private float mainMusicDuckVolume = 0.25f;
    [Tooltip("Duration to fade main music volume when ducking or restoring (seconds).")]
    [SerializeField] private float musicVolumeFadeDuration = 0.3f;

    [Header("Optional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool persistAcrossScenes;

    private float savedMainMusicVolume = 1f;
    private bool isMainMusicDucked;
    private Coroutine mainMusicFadeRoutine;

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

        if (maskMusicSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != audioSource && sources[i] != musicSource)
                {
                    maskMusicSource = sources[i];
                    break;
                }
            }
        }

        if (maskMusicSource == null)
        {
            maskMusicSource = gameObject.AddComponent<AudioSource>();
            maskMusicSource.playOnAwake = false;
            maskMusicSource.loop = true;
        }
        else
        {
            maskMusicSource.loop = true;
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

    /// <summary>Set main music volume (0–1). Also used as restore value when mask music stops.</summary>
    public void SetMusicVolume(float volume)
    {
        float v = Mathf.Clamp01(volume);
        savedMainMusicVolume = v;
        if (musicSource != null && !isMainMusicDucked)
        {
            musicSource.volume = v;
        }
    }

    /// <summary>Whether main music is currently playing.</summary>
    public bool IsMainMusicPlaying => musicSource != null && musicSource.isPlaying;

    /// <summary>Play music for the current mask (from UI assets). Lowers main music volume; loops until stopped.</summary>
    public void PlayMaskMusic(AudioClip clip)
    {
        if (maskMusicSource == null)
        {
            return;
        }

        if (clip == null)
        {
            maskMusicSource.Stop();
            RestoreMainMusicVolume();
            return;
        }

        if (musicSource != null)
        {
            if (!isMainMusicDucked)
            {
                savedMainMusicVolume = musicSource.volume;
                isMainMusicDucked = true;
            }
            float targetVolume = savedMainMusicVolume * mainMusicDuckVolume;
            StartFadeMainMusicVolume(targetVolume);
        }

        maskMusicSource.clip = clip;
        maskMusicSource.loop = true;
        maskMusicSource.Play();
    }

    /// <summary>Stop mask music and restore main music volume.</summary>
    public void StopMaskMusic()
    {
        if (maskMusicSource != null)
        {
            maskMusicSource.Stop();
        }

        RestoreMainMusicVolume();
    }

    private void RestoreMainMusicVolume()
    {
        StartFadeMainMusicVolume(savedMainMusicVolume);
        isMainMusicDucked = false;
    }

    private void StartFadeMainMusicVolume(float targetVolume)
    {
        if (mainMusicFadeRoutine != null)
        {
            StopCoroutine(mainMusicFadeRoutine);
            mainMusicFadeRoutine = null;
        }

        if (musicSource == null)
        {
            return;
        }

        mainMusicFadeRoutine = StartCoroutine(FadeMainMusicVolumeRoutine(targetVolume));
    }

    private IEnumerator FadeMainMusicVolumeRoutine(float targetVolume)
    {
        if (musicSource == null)
        {
            mainMusicFadeRoutine = null;
            yield break;
        }

        float from = musicSource.volume;
        float duration = Mathf.Max(0.001f, musicVolumeFadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            musicSource.volume = Mathf.Lerp(from, targetVolume, t);
            yield return null;
        }

        musicSource.volume = targetVolume;
        mainMusicFadeRoutine = null;
    }
}
