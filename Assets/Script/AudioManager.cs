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
    [Range(0f, 1f)] public float defaultMusicVolume = 0.75f;
    [Range(0f, 1f)] public float defaultSFXVolume = 0.75f;

    private const string MUSIC_VOL = "MusicVolume";
    private const string SFX_VOL   = "SFXVolume";
    private const string MUTE_ALL  = "MuteAll";

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupSources();
        LoadSettings();
    }

    private void SetupSources()
    {
        // MUSIC SOURCE
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;

        // SFX SOURCE
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }

    // ==================================================
    // MUSIC
    // ==================================================
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
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

    public void PlayButtonClick()
    {
        if (buttonClickClip == null) return;
        sfxSource.PlayOneShot(buttonClickClip);
    }

    // ==================================================
    // SFX
    // ==================================================
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    // ==================================================
    // VOLUME SETTINGS
    // ==================================================
    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
        PlayerPrefs.SetFloat(MUSIC_VOL, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
        PlayerPrefs.SetFloat(SFX_VOL, value);
        PlayerPrefs.Save();
    }

    // ==================================================
    // MUTE ALL (Music + SFX)
    // ==================================================
    public void SetMuteAll(bool muted)
    {
        musicSource.mute = muted;
        sfxSource.mute = muted;

        PlayerPrefs.SetInt(MUTE_ALL, muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsMuted()
    {
        return PlayerPrefs.GetInt(MUTE_ALL, 0) == 1;
    }

    // ==================================================
    // LOAD SAVED SETTINGS
    // ==================================================
    public void LoadSettings()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL, defaultMusicVolume);
        float sfxVol   = PlayerPrefs.GetFloat(SFX_VOL, defaultSFXVolume);
        bool muted     = PlayerPrefs.GetInt(MUTE_ALL, 0) == 1;

        musicSource.volume = musicVol;
        sfxSource.volume = sfxVol;

        musicSource.mute = muted;
        sfxSource.mute = muted;
    }
}