using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CustomizationUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterCustomizer customizer;
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Next Buttons (Right)")]
    [SerializeField] private Button hairNextBtn;
    [SerializeField] private Button shirtNextBtn;
    [SerializeField] private Button shortsNextBtn;
    [SerializeField] private Button shoesNextBtn;

    [Header("Previous Buttons (Left)")]
    [SerializeField] private Button hairPrevBtn;
    [SerializeField] private Button shirtPrevBtn;
    [SerializeField] private Button shortsPrevBtn;
    [SerializeField] private Button shoesPrevBtn;

    [Header("Action Buttons")]
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button backBtn;

    [Header("Scene Names")]
    [SerializeField] private string nextSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        // NEXT BUTTONS
        hairNextBtn.onClick.AddListener(() =>
            customizer.ChangePart(CharacterCustomizer.BodyPartType.Hair, 1));

        shirtNextBtn.onClick.AddListener(() =>
            customizer.ChangePart(CharacterCustomizer.BodyPartType.Shirt, 1));

        shortsNextBtn.onClick.AddListener(() =>
            customizer.ChangePart(CharacterCustomizer.BodyPartType.Shorts, 1));

        shoesNextBtn.onClick.AddListener(() =>
            customizer.ChangePart(CharacterCustomizer.BodyPartType.Shoes, 1));

        // PREVIOUS BUTTONS
        hairPrevBtn.onClick.AddListener(() =>
            customizer.ChangePart(CharacterCustomizer.BodyPartType.Hair, -1));

        shirtPrevBtn.onClick.AddListener(() =>
            customizer.ChangePart(CharacterCustomizer.BodyPartType.Shirt, -1));

        shortsPrevBtn.onClick.AddListener(() =>
            customizer.ChangePart(CharacterCustomizer.BodyPartType.Shorts, -1));

        shoesPrevBtn.onClick.AddListener(() =>
            customizer.ChangePart(CharacterCustomizer.BodyPartType.Shoes, -1));

        // SAVE BUTTON
        saveBtn.onClick.AddListener(HandleSaveAndNextScene);

        // BACK BUTTON
        if (backBtn != null)
            backBtn.onClick.AddListener(ReturnToMainMenu);

        customizer.LoadCharacter();

        if (PlayerPrefs.HasKey("CharacterName"))
            nameInputField.text = PlayerPrefs.GetString("CharacterName");
    }

    private void HandleSaveAndNextScene()
    {
        PlayerPrefs.SetString("CharacterName", nameInputField.text);
        customizer.SaveCharacter();
        SceneManager.LoadScene(nextSceneName);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}