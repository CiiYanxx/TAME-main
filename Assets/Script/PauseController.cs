using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Panel Reference")]
    public GameObject pauseMenuPanel;

    [Header("Audio UI References")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle muteToggle;

    private void Start()
    {
        // =========================
        // UI LISTENERS
        // =========================
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(SetMuteAll);

        // =========================
        // LOAD CURRENT AUDIO VALUES
        // =========================
        if (AudioManager.Instance != null)
        {
            if (musicSlider != null)
                musicSlider.value = AudioManager.Instance.musicSource.volume;

            if (sfxSlider != null)
                sfxSlider.value = AudioManager.Instance.sfxSource.volume;

            if (muteToggle != null)
                muteToggle.isOn = AudioManager.Instance.IsMuted();
        }

        // Start closed
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    // =========================
    // PAUSE SYSTEM
    // =========================
    public void TogglePause()
    {
        if (pauseMenuPanel == null) return;

        bool paused = pauseMenuPanel.activeSelf;

        pauseMenuPanel.SetActive(!paused);
        Time.timeScale = !paused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        TogglePause();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("01_MainMenu");
    }

    // =========================
    // AUDIO SETTINGS
    // =========================
    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    public void SetMuteAll(bool muted)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMuteAll(muted);
    }
}