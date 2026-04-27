using UnityEngine;
using UnityEngine.UI;

public class TutorialTrustSlide : MonoBehaviour
{
    [Header("Tutorial Controller")]
    public TutorialController tutorial;

    [Header("Slides")]
    public Image previewImage;
    public Sprite[] slides;   // 3 to 4 images

    [Header("Buttons")]
    public Button nextButton;
    public Button backButton; // ✅ BACK BUTTON

    private int currentIndex = 0;

    void OnEnable()
    {
        currentIndex = 0;
        RefreshSlide();

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextSlide);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(PreviousSlide);
        }

        UpdateBackButtonState();
    }

    void RefreshSlide()
    {
        if (previewImage == null) return;
        if (slides == null || slides.Length == 0) return;

        previewImage.sprite = slides[currentIndex];
        UpdateBackButtonState();
    }

    public void NextSlide()
    {
        if (slides == null || slides.Length == 0) return;

        currentIndex++;

        // 👉 kung may next pa
        if (currentIndex < slides.Length)
        {
            RefreshSlide();
            return;
        }

        // 👉 last slide → DONE
        if (tutorial != null)
            tutorial.Continue();
    }

    void PreviousSlide()
    {
        if (slides == null || slides.Length == 0) return;

        currentIndex--;

        // clamp para di mag negative
        if (currentIndex < 0)
            currentIndex = 0;

        RefreshSlide();
    }

    void UpdateBackButtonState()
    {
        if (backButton != null)
            backButton.interactable = currentIndex > 0; // disable pag first slide
    }
}