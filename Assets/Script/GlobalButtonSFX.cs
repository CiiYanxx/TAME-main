using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalButtonSFX : MonoBehaviour
{
    [Header("Default Button Sounds")]
    [SerializeField] private AudioClip defaultClickSound;

    [Header("Optional Special Sounds")]
    [SerializeField] private AudioClip backSound;
    [SerializeField] private AudioClip confirmSound;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterButtons();
    }

    private void RegisterButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (Button btn in buttons)
        {
            btn.onClick.RemoveListener(PlayDefaultSound);
            btn.onClick.AddListener(PlayDefaultSound);
        }
    }

    private void PlayDefaultSound()
    {
        if (AudioManager.Instance != null && defaultClickSound != null)
        {
            AudioManager.Instance.PlaySFX(defaultClickSound);
        }
    }

    // OPTIONAL MANUAL CALLS
    public void PlayBackSound()
    {
        if (AudioManager.Instance != null && backSound != null)
        {
            AudioManager.Instance.PlaySFX(backSound);
        }
    }

    public void PlayConfirmSound()
    {
        if (AudioManager.Instance != null && confirmSound != null)
        {
            AudioManager.Instance.PlaySFX(confirmSound);
        }
    }
}