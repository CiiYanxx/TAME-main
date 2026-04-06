using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO; 

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Play/Load Button Logic")]
    [SerializeField] private Image playButtonImage; 
    [SerializeField] private Sprite playSprite;      
    [SerializeField] private Sprite loadGameSprite;  

    [Header("Audio Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private AudioSource backgroundMusic; // I-drag dito ang AudioSource mo

    [Header("Options & Reset")]
    [SerializeField] private Button resetProgressButton; 

    [Header("Scene Names")]
    [SerializeField] private string characterCustomizeScene = "Customize Character";
    [SerializeField] private string mainGameScene = "02_GameScene";

    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
        UpdatePlayButtonState();
        LoadAudioSettings(); // I-load ang volume settings

        // Setup Audio Listeners
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetVolume);
        if (muteToggle != null) muteToggle.onValueChanged.AddListener(SetMute);

        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.RemoveAllListeners();
            resetProgressButton.onClick.AddListener(ResetGameProgress);
        }
    }

    private void UpdatePlayButtonState()
    {
        bool hasSave = File.Exists(Application.persistentDataPath + "/savedata.json");
        
        if (playButtonImage != null)
        {
            playButtonImage.sprite = hasSave ? loadGameSprite : playSprite;
            
            Button btn = playButtonImage.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            
            if (hasSave) btn.onClick.AddListener(ContinueGame);
            else btn.onClick.AddListener(NewGame);
        }
    }

    // --- AUDIO LOGIC ---
    public void SetVolume(float volume)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = volume;
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }
    }

    public void SetMute(bool isMuted)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = isMuted;
            PlayerPrefs.SetInt("MusicMuted", isMuted ? 1 : 0);
        }
    }

    private void LoadAudioSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        bool savedMute = PlayerPrefs.GetInt("MusicMuted", 0) == 1;

        if (musicSlider != null) musicSlider.value = savedVolume;
        if (muteToggle != null) muteToggle.isOn = savedMute;

        if (backgroundMusic != null)
        {
            backgroundMusic.volume = savedVolume;
            backgroundMusic.mute = savedMute;
        }
    }

    // --- NAVIGATION ---
    public void NewGame()
    {
        if (File.Exists(Application.persistentDataPath + "/savedata.json"))
            File.Delete(Application.persistentDataPath + "/savedata.json");

        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();
        
        PlayerPrefs.SetString("TargetScene", characterCustomizeScene);
        SceneManager.LoadScene("Customize Character"); 
    }

    public void ContinueGame()
    {
        PlayerPrefs.SetString("TargetScene", mainGameScene);
        SceneManager.LoadScene("3LoadingScreen"); 
    }

    public void OpenOptions()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void ResetGameProgress()
    {
        if (File.Exists(Application.persistentDataPath + "/savedata.json"))
            File.Delete(Application.persistentDataPath + "/savedata.json");

        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();
        
        Debug.Log("<color=red>Progress Reset!</color>");
        UpdatePlayButtonState();
        CloseOptions();
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}