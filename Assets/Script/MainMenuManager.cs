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
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle muteToggle;

    [Header("Options & Reset")]
    [SerializeField] private Button resetProgressButton;

    [Header("Main Menu UI Objects (Hide/Show System)")]
    [SerializeField] private GameObject[] hideOnOptionsOpen;
    [SerializeField] private GameObject[] showOnOptionsClose;

    [Header("Reset Confirmation Panel")]
    [SerializeField] private GameObject resetConfirmPanel;
    [SerializeField] private Button yesResetButton;
    [SerializeField] private Button noResetButton;

    [Header("Scene Names")]
    [SerializeField] private string characterCustomizeScene = "Customize Character";
    [SerializeField] private string mainGameScene = "02_GameScene";

    private void Start()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // PLAY MAIN MENU MUSIC
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuMusic();

            // LOAD CURRENT SETTINGS
            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.Instance.musicSource.volume;

                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.sfxSource.volume;

                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            }

            if (muteToggle != null)
            {
                muteToggle.isOn = AudioManager.Instance.IsMuted();

                muteToggle.onValueChanged.RemoveAllListeners();
                muteToggle.onValueChanged.AddListener(AudioManager.Instance.SetMuteAll);
            }
        }

        // RESET CONFIRM PANEL
        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(false);

        // YES BUTTON
        if (yesResetButton != null)
        {
            yesResetButton.onClick.RemoveAllListeners();
            yesResetButton.onClick.AddListener(ConfirmResetGame);
        }

        // NO BUTTON
        if (noResetButton != null)
        {
            noResetButton.onClick.RemoveAllListeners();
            noResetButton.onClick.AddListener(CloseResetConfirm);
        }

        // RESET BUTTON
        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.RemoveAllListeners();
            resetProgressButton.onClick.AddListener(ResetGameProgress);
        }

        UpdatePlayButtonState();
    }

    private void UpdatePlayButtonState()
    {
        bool hasSave = File.Exists(Application.persistentDataPath + "/savedata.json");

        if (playButtonImage != null)
        {
            playButtonImage.sprite = hasSave ? loadGameSprite : playSprite;

            Button btn = playButtonImage.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            if (hasSave)
                btn.onClick.AddListener(ContinueGame);
            else
                btn.onClick.AddListener(NewGame);
        }
    }

    public void NewGame()
    {
        if (File.Exists(Application.persistentDataPath + "/savedata.json"))
            File.Delete(Application.persistentDataPath + "/savedata.json");

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
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        // HIDE OBJECTS
        if (hideOnOptionsOpen != null)
        {
            foreach (GameObject obj in hideOnOptionsOpen)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // SHOW OBJECTS
        if (showOnOptionsClose != null)
        {
            foreach (GameObject obj in showOnOptionsClose)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }

    public void ResetGameProgress()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(true);
    }

    public void ConfirmResetGame()
    {
        if (File.Exists(Application.persistentDataPath + "/savedata.json"))
            File.Delete(Application.persistentDataPath + "/savedata.json");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("<color=red>Progress Reset!</color>");

        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(false);

        UpdatePlayButtonState();
        CloseOptions();
    }

    public void CloseResetConfirm()
    {
        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    private void OnEnable()
    {
        UpdatePlayButtonState();
    }

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}