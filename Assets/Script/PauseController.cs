using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Audio Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle muteToggle;

    [Header("Optional UI Hide When Pause")]
    [SerializeField] private GameObject[] hideWhenPaused;

    private void Start()
    {
        // START CLOSED
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // LOAD CURRENT AUDIO SETTINGS
        if (AudioManager.Instance != null)
        {
            // MUSIC
            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.Instance.musicSource.volume;

                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            }

            // SFX
            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.sfxSource.volume;

                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            }

            // MUTE
            if (muteToggle != null)
            {
                muteToggle.isOn = AudioManager.Instance.IsMuted();

                muteToggle.onValueChanged.RemoveAllListeners();
                muteToggle.onValueChanged.AddListener(AudioManager.Instance.SetMuteAll);
            }
        }
    }

    // ==================================================
    // TOGGLE PAUSE
    // ==================================================
    public void TogglePause()
    {
        if (pauseMenuPanel == null)
            return;

        bool isPaused = pauseMenuPanel.activeSelf;

        pauseMenuPanel.SetActive(!isPaused);

        // TIME SCALE
        Time.timeScale = !isPaused ? 0f : 1f;

        // OPTIONAL HIDE OBJECTS
        if (hideWhenPaused != null)
        {
            foreach (GameObject obj in hideWhenPaused)
            {
                if (obj != null)
                    obj.SetActive(isPaused);
            }
        }
    }

    // ==================================================
    // RESUME
    // ==================================================
    public void ResumeGame()
    {
        TogglePause();
    }

    // ==================================================
    // MAIN MENU
    // ==================================================
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("01_MainMenu");
    }
}