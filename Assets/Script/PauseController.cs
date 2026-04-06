using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class PauseController : MonoBehaviour
{
    [Header("Panel Reference")]
    [Tooltip("The main Pause Menu Panel GameObject.")]
    public GameObject pauseMenuPanel;

    [Header("Audio UI References")]
    [Tooltip("The Slider used to control music volume.")]
    public Slider volumeSlider;
    [Tooltip("The Toggle used to mute/unmute music.")]
    public Toggle muteToggle;

    private void Start()
    {
        // Link UI controls to their functions
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        
        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(ToggleMusicMute);

        // Initialize UI values using the AudioManager's current settings
        if (AudioManager.Instance != null && AudioManager.Instance.musicSource != null)
        {
            if (volumeSlider != null)
                volumeSlider.value = AudioManager.Instance.musicSource.volume;
            
            if (muteToggle != null)
                muteToggle.isOn = AudioManager.Instance.musicSource.mute;
        }
        
        // Ensure the panel starts closed
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    // 1. Pause/Unpause Logic (Called by the Pause Button and Resume Button)
    public void TogglePause()
    {
        // Check for the critical reference to avoid NullReferenceException
        if (pauseMenuPanel == null)
        {
            Debug.LogError("PauseController: pauseMenuPanel is not assigned! Cannot toggle pause state.");
            return;
        }
        
        bool isPaused = pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(!isPaused);

        // Pause time (0) or Resume time (1)
        Time.timeScale = !isPaused ? 0f : 1f;
        Debug.Log($"Game {(isPaused ? "Resumed" : "Paused")}");
    }

    // 2. Button Functions 
    
    // Called by the Resume Button inside the Pause Menu Panel
    public void ResumeGame()
    {
        TogglePause();
    }

    // Called by the Main Menu Button inside the Pause Menu Panel
    public void GoToMainMenu()
    {
        // Ensure time resumes before loading a new scene
        Time.timeScale = 1f; 
        
        // IMPORTANT: Change "MainMenu" to your actual main menu scene name!
        SceneManager.LoadScene("01_MainMenu"); 
    }
    
    // 3. Audio UI Callbacks (Calls the central AudioManager)
    
    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
            
            // Unmute the toggle if volume is raised from 0
            if (volume > 0 && muteToggle != null && muteToggle.isOn)
            {
                muteToggle.isOn = false;
            }
        }
    }

    public void ToggleMusicMute(bool isMuted)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusicMute(isMuted);
        }
    }
}