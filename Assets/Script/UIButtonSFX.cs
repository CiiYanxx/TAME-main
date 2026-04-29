using UnityEngine;
using UnityEngine.UI;

public class UIButtonSFX : MonoBehaviour
{
    [Header("Buttons With SFX")]
    public Button[] buttons;

    private void Start()
    {
        foreach (Button btn in buttons)
        {
            if (btn != null)
                btn.onClick.AddListener(PlaySound);
        }
    }

    private void PlaySound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
}