using UnityEngine;



public class AudioManager : MonoBehaviour

{

    public static AudioManager Instance;



    [Header("Audio Sources")]

    public AudioSource musicSource;

   

    private void Awake()

    {

        // Enforce Singleton Pattern

        if (Instance == null)

        {

            Instance = this;

            DontDestroyOnLoad(gameObject); // Keeps music playing across scenes

        }

        else

        {

            Destroy(gameObject);

            return;

        }



        if (musicSource == null)

        {

            musicSource = gameObject.AddComponent<AudioSource>();

        }

    }



    public void SetMusicVolume(float volume)

    {

        if (musicSource != null)

        {

            musicSource.volume = volume;

        }

    }



    public void ToggleMusicMute(bool isMuted)

    {

        if (musicSource != null)

        {

            musicSource.mute = isMuted;

        }

    }

}