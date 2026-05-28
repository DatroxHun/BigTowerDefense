using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("BGM")]
    [SerializeField] private AudioSource[] bgmSources = new AudioSource[2];
    private LTDescr bgmFade = null;
    private LTDescr bgmPitch = null;
    private int activeBgmIndex = 0;

    [Header("SFX Pooling")]
    [SerializeField, Tooltip("How many overlapping sounds can play at once?")]
    private int sfxPoolSize = 10;

    // pool of generated audio sources
    private AudioSource[] sfxPool;
    private int sfxPoolIndex = 0;

    [Header("Clips")]
    [SerializeField] private List<ClipElement> clips;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // keeps audio playing between scene loads
            InitializePool();
            SetupBGMSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBGM(Clip.CalmBGM);
    }

    private void InitializePool()
    {
        sfxPool = new AudioSource[sfxPoolSize];

        for (int i = 0; i < sfxPoolSize; i++)
        {
            // add AudioSources to this GameObject
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = sfxMixerGroup;
            newSource.playOnAwake = false;

            sfxPool[i] = newSource;
        }
    }

    private void SetupBGMSources()
    {
        foreach (var source in bgmSources)
        {
            source.outputAudioMixerGroup = musicMixerGroup;
            source.loop = true;
            source.playOnAwake = false;
        }
    }


    // --- SOUND EFFECTS ---

    /// <summary>
    /// Plays an SFX. Optionally pass pitch constraints to randomize it!
    /// </summary>
    public static void PlaySFX(Clip clip, float volume = 1f, float minPitch = 1f, float maxPitch = 1f)
    {
        if (instance.clips.Any(x => x.key == clip))
        {
            PlaySFX(instance.clips.First(x => x.key == clip).value, volume, minPitch, maxPitch);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Could not play SFX. '{clip}' is missing from the clips list or has no AudioClip assigned!");
        }
    }

    /// <summary>
    /// Plays an SFX. Optionally pass pitch constraints to randomize it!
    /// </summary>
    public static void PlaySFX(AudioClip clip, float volume = 1f, float minPitch = 1f, float maxPitch = 1f)
    {
        if (clip == null || instance == null) return;

        AudioSource source = instance.sfxPool[instance.sfxPoolIndex];
        instance.sfxPoolIndex = (instance.sfxPoolIndex + 1) % instance.sfxPoolSize;

        source.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        source.volume = volume;
        source.clip = clip;
        source.Play();
    }

    // --- BACKGROUND MUSIC ---
    public static void PlayBGM(Clip clip, float fadeDuration = 2.5f)
    {
        if (instance.clips.Any(x => x.key == clip))
        {
            PlayBGM(instance.clips.First(x => x.key == clip).value, fadeDuration);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Could not play BGM. '{clip}' is missing from the clips list or has no AudioClip assigned!");
        }
    }

    public static void PlayBGM(AudioClip newClip, float fadeDuration = 2.5f)
    {
        if (newClip == null || instance == null) return;

        int activeSource = instance.activeBgmIndex;
        int nextSource = 1 - instance.activeBgmIndex;

        // don't restart the track if it's already playing
        if (instance.bgmSources[activeSource].clip == newClip) return;

        AudioSource fadeOutSource = instance.bgmSources[activeSource];
        AudioSource fadeInSource = instance.bgmSources[nextSource];
        instance.activeBgmIndex = nextSource;

        if (instance.bgmFade != null)
            LeanTween.cancel(instance.gameObject, instance.bgmFade.uniqueId);

        if (fadeInSource.clip != newClip)
        {
            fadeInSource.clip = newClip;
            fadeInSource.volume = 0f;
            fadeInSource.Play();
        }
        else if (!fadeInSource.isPlaying)
        {
            fadeInSource.Play();
        }

        float startVolIn = fadeInSource.volume;
        float startVolOut = fadeOutSource.volume;

        instance.bgmFade = 
        LeanTween.value(instance.gameObject, (float val) =>
        {
            fadeInSource.volume = Mathf.Lerp(startVolIn, 1f, val);
            fadeOutSource.volume = Mathf.Lerp(startVolOut, 0f, val);
        }, 0f, 1f, fadeDuration)
        .setEase(LeanTweenType.easeInOutSine)
        .setOnComplete(() =>
        {
            fadeInSource.volume = 1f;
            fadeOutSource.volume = 0f;

            fadeOutSource.Stop();
            instance.bgmFade = null;
        })
        .setIgnoreTimeScale(true);
    }

    public static void SetBGMPitch(float to, float fadeDuration = 1f)
    {
        if (instance == null) return;

        if (instance.bgmPitch != null)
            LeanTween.cancel(instance.gameObject, instance.bgmPitch.uniqueId);

        float currentPitch = instance.bgmSources[instance.activeBgmIndex].pitch;

        instance.bgmPitch = 
        LeanTween.value(instance.gameObject, (float val) =>
        {
            foreach (var source in instance.bgmSources)
            {
                source.pitch = Mathf.Lerp(currentPitch, to, val);
            }
        }, 0f, 1f, fadeDuration)
        .setEase(LeanTweenType.easeInOutSine)
        .setOnComplete(() =>
        {
            foreach (var source in instance.bgmSources)
            {
                source.pitch = to;
            }

            instance.bgmPitch = null;
        })
        .setIgnoreTimeScale(true);
    }

    // --- VOLUME ---
    public static void SetMasterVolume(float v) => SetVolume("MasterVol", v);
    public static void SetMusicVolume(float v) => SetVolume("MusicVol", v);
    public static void SetSFXVolume(float v) => SetVolume("SFXVol", v);

    public static bool GetMasterVolume(out float v) => GetVolume("MasterVol", out v);
    public static bool GetMusicVolume(out float v) => GetVolume("MusicVol", out v);
    public static bool GetSFXVolume(out float v) => GetVolume("SFXVol", out v);

    private static void SetVolume(string name, float v)
    {
        v = Mathf.Clamp(v, 0.0001f, 1f);
        instance.mainMixer.SetFloat(name, Mathf.Log10(v) * 20f);
    }

    private static bool GetVolume(string name, out float v)
    {
        bool result = instance.mainMixer.GetFloat(name, out v);
        v = Mathf.Pow(10f, v / 20f);

        return result;
    }
}

public enum Clip
{
    CalmBGM,
    BattleBGM,
    BasicAttack,
    Electro,
    Heal,
    Shot,
    Buy,
    Place,
    Warning,
    Echo,
    Boom
}

[Serializable]
public class ClipElement
{
    public Clip key;
    public AudioClip value;
}