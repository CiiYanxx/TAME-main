using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class PlayerCharacterCustomizer : MonoBehaviour
{
    [Serializable]
    public class BodyPartGroup {
        public BodyPartType bodyPartType;
        public GameObject[] variants; // Drag your child objects (HAIR_1_L, etc.) here
        [HideInInspector] public int currentIndex = 0;
    }

    public enum BodyPartType { Hair, ClothesTop, ClothesBottom, SkinColor }

    [SerializeField] private List<BodyPartGroup> bodyPartGroups;
    [SerializeField] private TMP_InputField nameInputField;

    // Public function that accepts an int to fix the conversion error
    public void ChangeBodyPart(int typeIndex) 
    {
        BodyPartType type = (BodyPartType)typeIndex;
        BodyPartGroup group = bodyPartGroups.Find(x => x.bodyPartType == type);
        
        if (group == null || group.variants.Length == 0) return;

        // Disable current variant
        group.variants[group.currentIndex].SetActive(false);

        // Move to next index using modulo
        group.currentIndex = (group.currentIndex + 1) % group.variants.Length;

        // Enable new variant
        group.variants[group.currentIndex].SetActive(true);
    }

    public void SaveCharacter() 
    {
        string pName = string.IsNullOrEmpty(nameInputField.text) ? "Player" : nameInputField.text;
        PlayerPrefs.SetString("PlayerName", pName);

        foreach (var group in bodyPartGroups) {
            PlayerPrefs.SetInt("Saved_" + group.bodyPartType.ToString(), group.currentIndex);
        }
        PlayerPrefs.Save(); //
    }
}