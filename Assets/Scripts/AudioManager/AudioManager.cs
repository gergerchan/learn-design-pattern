using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sound
{
    [Tooltip("Unique name used to play this sound via AudioManager.")]
    public string name;

    [Tooltip("AudioClip to play for this sound.")]
    public AudioClip clip;

    [Range(0f, 1f)]
    [Tooltip("Per-sound volume multiplier (combined with global Sfx/Bgm volume).")]
    public float volume = 1f;

    [Range(0.5f, 1.5f)]
    [Tooltip("Pitch applied when this sound is played.")]
    public float pitch = 1f;
}

/// <summary>
/// Singleton audio manager. Plays named SFX (one-shot) and BGM (looped),
/// and exposes global SFX/BGM volumes persisted in PlayerPrefs.
/// </summary>
[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    /// <summary>Raised whenever <see cref="SfxVolume"/> or <see cref="BgmVolume"/> changes.</summary>
    public event Action OnVolumeChanged;

    [SerializeField] private bool dontDestroyOnLoad = true;

    [Tooltip("If true, SFX/BGM volume is saved to PlayerPrefs and loaded on Awake. If false, defaults to startup volumes and no persistence occurs.")]
    [SerializeField] private bool persistVolumeSettings = true;

    [Range(0f, 1f)]
    [Tooltip("Initial SFX volume used when persistence is disabled or no saved value exists.")]
    [SerializeField] private float defaultSfxVolume = 1f;

    [Range(0f, 1f)]
    [Tooltip("Initial BGM volume used when persistence is disabled or no saved value exists.")]
    [SerializeField] private float defaultBgmVolume = 1f;

    [Header("Libraries")]
    [SerializeField] private Sound[] soundEffects;
    [SerializeField] private Sound[] backgroundMusic;

    private const string SfxVolumeKey = "SfxVolume";
    private const string BgmVolumeKey = "BgmVolume";

    private readonly Dictionary<string, Sound> sfxLookup = new();
    private readonly Dictionary<string, Sound> bgmLookup = new();

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    private string currentBgmName;
    private float currentBgmBaseVolume = 1f;

    public float SfxVolume { get; private set; } = 1f;
    public float BgmVolume { get; private set; } = 1f;

    /// <summary>If true, volume changes are persisted to PlayerPrefs.</summary>
    public bool PersistVolumeSettings
    {
        get => persistVolumeSettings;
        set => persistVolumeSettings = value;
    }

    // -----------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        BuildLookup(soundEffects, sfxLookup);
        BuildLookup(backgroundMusic, bgmLookup);

        SfxVolume = Mathf.Clamp01(persistVolumeSettings
            ? PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume)
            : defaultSfxVolume);
        BgmVolume = Mathf.Clamp01(persistVolumeSettings
            ? PlayerPrefs.GetFloat(BgmVolumeKey, defaultBgmVolume)
            : defaultBgmVolume);
        ApplyBgmVolume();

        if (dontDestroyOnLoad)
        {
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
    }

    private static void BuildLookup(Sound[] sounds, Dictionary<string, Sound> map)
    {
        map.Clear();
        if (sounds == null) return;
        foreach (var s in sounds)
        {
            if (s != null && !string.IsNullOrEmpty(s.name))
                map[s.name] = s;
        }
    }

    // -----------------------------------------------------------------
    // Volume
    // -----------------------------------------------------------------

    public void SetSfxVolume(float value, bool save = true)
    {
        SfxVolume = Mathf.Clamp01(value);
        if (save && persistVolumeSettings) Persist(SfxVolumeKey, SfxVolume);
        OnVolumeChanged?.Invoke();
    }

    public void SetBgmVolume(float value, bool save = true)
    {
        BgmVolume = Mathf.Clamp01(value);
        ApplyBgmVolume();
        if (save && persistVolumeSettings) Persist(BgmVolumeKey, BgmVolume);
        OnVolumeChanged?.Invoke();
    }

    public void SetVolume(float sfx, float bgm, bool save = true)
    {
        SfxVolume = Mathf.Clamp01(sfx);
        BgmVolume = Mathf.Clamp01(bgm);
        ApplyBgmVolume();

        if (save && persistVolumeSettings)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
            PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
            PlayerPrefs.Save();
        }

        OnVolumeChanged?.Invoke();
    }

    private static void Persist(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    private void ApplyBgmVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = BgmVolume * currentBgmBaseVolume;
    }

    // -----------------------------------------------------------------
    // Playback
    // -----------------------------------------------------------------

    /// <summary>Plays a one-shot sound effect by name.</summary>
    public void PlaySfx(string name)
    {
        if (!TryGet(sfxLookup, name, "SFX", out var sound)) return;

        sfxSource.pitch = sound.pitch;
        sfxSource.PlayOneShot(sound.clip, SfxVolume * sound.volume);
    }

    /// <summary>Plays a looped BGM by name. Ignores call if the track is already playing.</summary>
    public void PlayBgm(string name)
    {
        if (currentBgmName == name && bgmSource.isPlaying) return;
        if (!TryGet(bgmLookup, name, "BGM", out var sound)) return;

        currentBgmBaseVolume = sound.volume <= 0f ? 1f : Mathf.Clamp01(sound.volume);

        bgmSource.Stop();
        bgmSource.clip = sound.clip;
        bgmSource.pitch = sound.pitch;
        ApplyBgmVolume();
        Debug.Log($"BgmVolume={BgmVolume}, baseVol={currentBgmBaseVolume}, src.volume={bgmSource.volume}, src.mute={bgmSource.mute}, listener.volume={AudioListener.volume}, listener.pause={AudioListener.pause}");

        bgmSource.Play();
        currentBgmName = name;
    }

    public void StopBgm()
    {
        if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
        currentBgmName = null;
    }

    private static bool TryGet(Dictionary<string, Sound> map, string name, string kind, out Sound sound)
    {
        sound = null;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning($"[AudioManager] Play{kind} called with null/empty name.");
            return false;
        }
        if (!map.TryGetValue(name, out sound) || sound.clip == null)
        {
            Debug.LogWarning($"[AudioManager] {kind} '{name}' not found or has no clip.");
            return false;
        }
        return true;
    }

    [ContextMenu("Test / Play BGM")]
    public void TestPlayBgm()
    {
        PlayBgm("bgm2");
    }
    [ContextMenu("Test / Play SFX")]
    public void TestPlaySfx()
    {
        var randomSound = soundEffects[UnityEngine.Random.Range(0, soundEffects.Length)];
        PlaySfx(randomSound.name);
    }

    [ContextMenu("Test / Stop BGM")]
    public void TestStopBgm()
    {
        StopBgm();
    }
}
