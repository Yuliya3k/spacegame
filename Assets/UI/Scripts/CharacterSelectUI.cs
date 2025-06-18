using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterSelectUI : MonoBehaviour
{


    [Header("UI References")]
    public GameObject characterSelectPanel;
    // The parent panel that holds the selection UI (buttons, text, etc.)

    public Button leftButton;
    public Button rightButton;
    public Button confirmButton;
    public TextMeshProUGUI characterNameText;

    [Header("Character Model References")]
    public SkinnedMeshRenderer characterRenderer;
    // The same one you’ll be using for base blend shapes
    // (or a special preview mesh if you prefer)

    [Header("Character Stats Reference")]
    [Tooltip("Assign the CharacterStats from your Player object in the Inspector.")]
    public CharacterStats characterStats;

    [Header("Profiles")]
    public List<CharacterProfile> characterProfiles;
    private int currentIndex = 0;

    [Header("Weight Gain Distribution Toggles")]
    public Toggle boobsToggle;
    public Toggle torsoToggle;
    public Toggle thighsToggle;
    public Toggle shinsToggle;
    public Toggle armsToggle;
    public Toggle wholeBodyToggle;
    public Toggle glutesToggle;

    private CharacterProfile lastProfile = null;

    public GameObject inGameUI;

    private void Start()
    {
        // Hide this UI by default (unless you want to see it in the editor).
        if (characterSelectPanel != null)
            characterSelectPanel.SetActive(false);

        // Hook up the buttons:
        leftButton.onClick.AddListener(OnLeftClicked);
        rightButton.onClick.AddListener(OnRightClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    public void OpenCharacterSelectUI()
    {
        if (characterSelectPanel != null)
            characterSelectPanel.SetActive(true);

        currentIndex = 0;
        ApplyProfile(characterProfiles[currentIndex]);


    }

    public void CloseCharacterSelectUI()
    {
        if (characterSelectPanel != null)
            characterSelectPanel.SetActive(false);

        UIManager.instance.EnableCharacterUIOpening();

        inGameUI.SetActive(true);
        Time.timeScale = 1f; // Resume the game

        inGameUI.SetActive(true);
        // Lock and hide the cursor during gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnLeftClicked()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = characterProfiles.Count - 1; // wrap around

        ApplyProfile(characterProfiles[currentIndex]);
    }

    private void OnRightClicked()
    {
        currentIndex++;
        if (currentIndex >= characterProfiles.Count) currentIndex = 0;

        ApplyProfile(characterProfiles[currentIndex]);
    }

    private void OnConfirmClicked()
    {
        // 1) Store the chosen character profile somewhere
        var chosenProfile = characterProfiles[currentIndex];

        // Write the toggles into that profile
        chosenProfile.enableBoobGain = boobsToggle.isOn;
        chosenProfile.enableTorsoGain = torsoToggle.isOn;
        chosenProfile.enableThighsGain = thighsToggle.isOn;
        chosenProfile.enableShinsGain = shinsToggle.isOn;
        chosenProfile.enableArmsGain = armsToggle.isOn;
        chosenProfile.enableWholeBodyGain = wholeBodyToggle.isOn;
        chosenProfile.enableGlutesGain = glutesToggle.isOn;

        // e.g. store in a GameManager or a static var:
        CharacterProfileManager.instance.SetChosenCharacter(chosenProfile);

        if (characterStats != null)
            characterStats.ApplyBaseCharacterProfile(chosenProfile);
        else
            Debug.LogWarning("characterStats is not assigned in the inspector.");

        CloseCharacterSelectUI();


        // 2) Hide the selection UI
        

        // 3) Resume the game or open In-Game UI, etc.
        //  Possibly call StartMenuController's "ActuallyStartGame()" or something.
        // ...
    }

    private void ApplyProfile(CharacterProfile newProfile)
    {
        // 1) If we do have a previously applied profile, zero out those old shapes only.
        if (lastProfile != null && characterStats != null)
        {
            foreach (var blendShapeSetting in lastProfile.baseBlendShapes)
            {
                // We set the old shape to zero so we don't "stack" shapes from multiple profiles.
                characterStats.SetBlendShapeWeightOnAllRenderers(blendShapeSetting.blendShapeName, 0f);
            }
        }

        // 2) Now apply the new profile: 
        //    (Set UI name, then call characterStats to apply base shapes.)
        if (characterNameText != null)
            characterNameText.text = newProfile.characterName;

        boobsToggle.isOn = newProfile.enableBoobGain;
        torsoToggle.isOn = newProfile.enableTorsoGain;
        thighsToggle.isOn = newProfile.enableThighsGain;
        shinsToggle.isOn = newProfile.enableShinsGain;
        armsToggle.isOn = newProfile.enableArmsGain;
        wholeBodyToggle.isOn = newProfile.enableWholeBodyGain;
        glutesToggle.isOn = newProfile.enableGlutesGain;


        if (characterStats != null)
        {
            // This sets each blend shape weight from the new profile.
            characterStats.ApplyBaseCharacterProfile(newProfile);
        }

        // 3) Update lastProfile so that next time we know which shapes to zero out.
        lastProfile = newProfile;
    }
}
