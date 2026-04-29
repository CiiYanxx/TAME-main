using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;

    [Header("SFX Clips")]
    public AudioClip buttonClickClip;

    [Header("Default Volume")]
    [Range(0f, 1f)] public float defaultMusicVolume = 0.5f;
    [Range(0f, 1f)] public float defaultSFXVolume = 0.5f;

    private const string MUSIC_VOL = "MusicVolume";
    private const string SFX_VOL   = "SFXVolume";
    private const string MUTE_ALL  = "MuteAll";

    private void Awake()
    {
        // SAFE SINGLETON
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();
        LoadSettings();
    }

    private void SetupSources()
    {
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }

    // ========================= MUSIC =========================

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    // ========================= SFX =========================

    public void PlayButtonClick()
    {
        if (Instance == null) return;
        if (sfxSource == null) return;
        if (buttonClickClip == null) return;

        sfxSource.PlayOneShot(buttonClickClip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
    }

    // ========================= VOLUME =========================

    public void SetMusicVolume(float value)
    {
        if (musicSource == null) return;

        musicSource.volume = value;
        PlayerPrefs.SetFloat(MUSIC_VOL, value);
    }

    public void SetSFXVolume(float value)
    {
        if (sfxSource == null) return;

        sfxSource.volume = value;
        PlayerPrefs.SetFloat(SFX_VOL, value);
    }

    // ========================= MUTE =========================

    public void SetMuteAll(bool muted)
    {
        if (musicSource != null) musicSource.mute = muted;
        if (sfxSource != null) sfxSource.mute = muted;

        PlayerPrefs.SetInt(MUTE_ALL, muted ? 1 : 0);
    }

    public bool IsMuted()
    {
        return PlayerPrefs.GetInt(MUTE_ALL, 0) == 1;
    }

    // ========================= LOAD =========================

    public void LoadSettings()
    {
        if (musicSource == null || sfxSource == null) return;

        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL, defaultMusicVolume);
        float sfxVol   = PlayerPrefs.GetFloat(SFX_VOL, defaultSFXVolume);
        bool muted     = PlayerPrefs.GetInt(MUTE_ALL, 0) == 1;

        musicSource.volume = musicVol;
        sfxSource.volume = sfxVol;

        musicSource.mute = muted;
        sfxSource.mute = muted;
    }
}