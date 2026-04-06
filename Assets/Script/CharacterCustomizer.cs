using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class CharacterCustomizer : MonoBehaviour
{
    public enum CustomizationType { ToggleObject, MaterialSwap, MeshSwap }
    // INALIS ANG SKIN DITO
    public enum BodyPartType { Hair, Eyes, Shirt, Shorts, Shoes }

    [Serializable]
    public class CustomizationData
    {
        public BodyPartType partType;
        public CustomizationType changeMethod;
        
        [Header("Toggling Objects")]
        public GameObject[] objectOptions; 

        [Header("Swapping Materials")]
        public SkinnedMeshRenderer targetRenderer;
        public Material[] materialOptions;

        [Header("Swapping Meshes")]
        public Mesh[] meshOptions;
        
        [HideInInspector] public int currentIndex = 0;
    }

    [SerializeField] private List<CustomizationData> customizationParts = new List<CustomizationData>();
    public TMP_InputField nameInputField;

    public List<CustomizationData> GetCustomizationParts() { return customizationParts; }

    private void Awake()
    {
        LoadCharacter(); 
    }

    // NAG-ADD NG INT DIRECTION PARAMETER DITO
    public void ChangePart(BodyPartType type, int direction = 1)
    {
        CustomizationData data = customizationParts.Find(x => x.partType == type);
        if (data == null) return;

        int maxOptions = GetOptionCount(data);
        if (maxOptions == 0) return;
        
        // DITO ANG LOGIC PARA SA NEXT (1) AT PREVIOUS (-1)
        data.currentIndex += direction;

        // Loop pabalik sa dulo kung lumagpas sa zero pa-left
        if (data.currentIndex < 0) 
            data.currentIndex = maxOptions - 1;
        
        // Loop pabalik sa simula kung lumagpas sa max pa-right
        else if (data.currentIndex >= maxOptions) 
            data.currentIndex = 0;

        ApplyVisuals(data);
    }

    private int GetOptionCount(CustomizationData data)
    {
        if (data.changeMethod == CustomizationType.ToggleObject) return data.objectOptions.Length;
        if (data.changeMethod == CustomizationType.MaterialSwap) return data.materialOptions.Length;
        return data.meshOptions.Length;
    }

    private void ApplyVisuals(CustomizationData data)
    {
        switch (data.changeMethod)
        {
            case CustomizationType.ToggleObject:
                for (int i = 0; i < data.objectOptions.Length; i++)
                    data.objectOptions[i].SetActive(i == data.currentIndex);
                break;

            case CustomizationType.MaterialSwap:
                if (data.targetRenderer != null)
                    data.targetRenderer.material = data.materialOptions[data.currentIndex];
                break;

            case CustomizationType.MeshSwap:
                if (data.targetRenderer != null)
                    data.targetRenderer.sharedMesh = data.meshOptions[data.currentIndex];
                break;
        }
    }

    public void SaveCharacter()
    {
        // 1. I-process ang Appearance Data
        string appearanceString = "";
        foreach (var part in customizationParts) appearanceString += part.currentIndex + ",";
        
        // 2. I-process ang Pangalan
        string rawName = (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text)) 
                         ? nameInputField.text 
                         : "Rescue Hero";

        // --- GLOBAL COLOR TAG ---
        string coloredName = $"<color=#0000FF>{rawName}</color>";
        
        PlayerPrefs.SetString("Character_Name", coloredName);

        // 3. I-load ang existing progress
        GameData currentData = SaveSystem.Load();
        
        int missions = 0;
        int points = 0;
        Vector3 pos = new Vector3(165.94f, 0.021f, 142.877f); 

        if (currentData != null)
        {
            missions = currentData.completedMissions;
            points = currentData.playerPoints;
            
            if(currentData.playerPos[0] != 0 || currentData.playerPos[2] != 0)
            {
                pos = new Vector3(currentData.playerPos[0], currentData.playerPos[1], currentData.playerPos[2]);
            }
        }

        // FLAG BILANG NEW GAME: Para lumitaw ang Bus Intro sa susunod na scene load
        PlayerPrefs.SetInt("IsNewGame", 1); 
        PlayerPrefs.Save();

        // 4. Final Save (ito yung gumagawa ng savedata.json)
        SaveSystem.Save(missions, points, pos, rawName, appearanceString);
        Debug.Log($"<color=green>[Save]</color> Character {rawName} saved. Flagging as New Game.");
    }

    public void LoadCharacter()
    {
        GameData data = SaveSystem.Load();
        if (data == null || string.IsNullOrEmpty(data.customizationData))
        {
            foreach (var part in customizationParts) { part.currentIndex = 0; ApplyVisuals(part); }
            return;
        }

        string[] savedIndices = data.customizationData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < customizationParts.Count; i++)
        {
            if (i < savedIndices.Length && int.TryParse(savedIndices[i], out int index))
            {
                customizationParts[i].currentIndex = index;
                ApplyVisuals(customizationParts[i]);
            }
        }
        
        if (nameInputField != null)
        {
            nameInputField.text = data.charName;
        }
    }
}