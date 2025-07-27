using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager instance; // Singleton pattern for easy access

    private PlayerInputActions inputActions;

    private bool canOpenCharacterUI = true;


    [Header("UI Elements")]
    public GameObject characterUI;   // Reference to the Character UI panel
    public GameObject skillsUI;      // Reference to the Skills UI panel
    public GameObject craftingUI; // Add this line for the new crafting UI
    public GameObject cookUI;     // Add this line for the new cooking UI
    public GameObject optionsUI;     // Reference to the Options UI panel

    [Header("Navigation Buttons in UIs")]
    public List<Button> characterButtons;  // Buttons for the Character UI
    public List<Button> skillsButtons;     // Buttons for the Skills UI
    public List<Button> cookButtons;     // For UI_Cook
    public List<Button> craftingButtons; // For UI_Craft
    public List<Button> optionsButtons;    // Buttons for the Options UI

    [Header("Game Screen UI Buttons")]
    public Button gameCharacterButton;   // Button to open Character UI from the game screen
    public Button gameSkillsButton;      // Button to open Skills UI from the game screen
    public Button gameCookButton;     // Add this
    public Button gameCraftingButton; // Add this
    public Button gameOptionsButton;     // Button to open Options UI from the game screen

    [Header("Close Buttons in Each UI")]
    public Button characterCloseButton;  // Button to close Character UI
    public Button skillsCloseButton;     // Button to close Skills UI
    public Button cookCloseButton;     // Add this
    public Button craftingCloseButton; // Add this
    public Button optionsCloseButton;    // Button to close Options UI

    [Header("Audio")]
    public AudioClip hoverSound;   // Sound when hovering over a button
    public AudioClip defaultOpenSound;    // Default sound when opening UI
    public AudioClip closeSound;   // Sound when closing UI
    public AudioSource audioSource; // Audio source to play UI sounds
    public AudioClip clickSound;




    [Header("Background Music")]
    public AudioSource backgroundMusicSource; // Audio source to play background music

    [Header("Click Sound Audio Source")]
    public AudioSource clickAudioSource; // Assign via Inspector

    [Header("UI Audio Settings")]
    public List<UIAudioSettings> uiAudioSettingsList = new List<UIAudioSettings>();


    [Header("Button Images")]
    public Sprite activeButtonImage;    // Image for the active button
    public Sprite inactiveButtonImage;  // Image for inactive buttons

    [Header("Game Screen UI Panels")]
    public List<GameObject> gameScreenUIs; // Register UIs to be hidden

    [Header("Android-Only Game Screen UI Panels")]
    public List<GameObject> androidOnlyUIs;


    private GameObject activeUI;  // Tracks the currently active UI panel
    private List<Button> activeButtons;  // Tracks the currently active set of buttons

    public UI_CraftWindow craftWindow;

    [Header("Post Processing")]
    public Volume postProcessVolume; // Reference to PostProcessVolume

    private UnityEngine.Rendering.Universal.DepthOfField depthOfField; // Depth of Field effect for blur

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure there is only one UIManager
        }

        inputActions = new PlayerInputActions();

        inputActions.UI.OpenInventory.performed += ctx =>
        {
            if (canOpenCharacterUI)
                OpenCharacterUI();
        };
        //inputActions.UI.OpenSkills.performed += ctx => OpenSkillsUI();
        //inputActions.UI.OpenOptions.performed += ctx => OpenOptionsUI();

        // Hide all UIs initially
        characterUI.SetActive(false);
        skillsUI.SetActive(false);
        cookUI.SetActive(false); 
        craftingUI.SetActive(false);
        optionsUI.SetActive(false);
        activeUI = null; // No active UI at start
    }

    private void Start()
    {
        // Add listeners to all UI navigation buttons
        AddButtonListeners(characterButtons, OpenCharacterUI);
        AddButtonListeners(skillsButtons, OpenSkillsUI);
        AddButtonListeners(cookButtons, OpenCookUI);
        AddButtonListeners(craftingButtons, OpenCraftingUI);
        AddButtonListeners(optionsButtons, OpenOptionsUI);

        // Add hover effects to UI navigation buttons
        AddHoverEffects(characterButtons);
        AddHoverEffects(skillsButtons);
        AddHoverEffects(cookButtons);
        AddHoverEffects(craftingButtons);
        AddHoverEffects(optionsButtons);

        // Add listeners to game screen buttons to open UIs
        AddButtonClickListener(gameCharacterButton, OpenCharacterUI);
        AddButtonClickListener(gameSkillsButton, OpenSkillsUI);
        AddButtonClickListener(gameCookButton, OpenCookUI);
        AddButtonClickListener(gameCraftingButton, OpenCraftingUI);
        AddButtonClickListener(gameOptionsButton, OpenOptionsUI);


        // Add hover effects to game screen buttons
        AddHoverEffect(gameCharacterButton);
        AddHoverEffect(gameSkillsButton);
        AddHoverEffect(gameCookButton);           // Add this
        AddHoverEffect(gameCraftingButton);       // Add this
        AddHoverEffect(gameOptionsButton);

        // Add listeners to the close buttons in each UI
        AddButtonClickListener(characterCloseButton, () => CloseSpecificUI(characterUI));
        AddButtonClickListener(skillsCloseButton, () => CloseSpecificUI(skillsUI));
        AddButtonClickListener(cookCloseButton, () => CloseSpecificUI(cookUI));
        AddButtonClickListener(craftingCloseButton, () => CloseSpecificUI(craftingUI));
        AddButtonClickListener(optionsCloseButton, () => CloseSpecificUI(optionsUI));



        // Add hover effects to the close buttons in each UI
        AddHoverEffect(characterCloseButton);
        AddHoverEffect(skillsCloseButton);
        AddHoverEffect(cookCloseButton);     // Add this
        AddHoverEffect(craftingCloseButton); // Add this
        AddHoverEffect(optionsCloseButton);

        // Initialize button opacities (set them to unhovered state at the start)
        SetInitialButtonOpacities(characterButtons);
        SetInitialButtonOpacities(skillsButtons);
        SetInitialButtonOpacities(cookButtons);
        SetInitialButtonOpacities(craftingButtons);
        SetInitialButtonOpacities(optionsButtons);

        SetInitialButtonOpacity(gameCharacterButton);
        SetInitialButtonOpacity(gameSkillsButton);
        SetInitialButtonOpacity(gameCookButton);
        SetInitialButtonOpacity(gameCraftingButton);
        SetInitialButtonOpacity(gameOptionsButton);

        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            backgroundMusicSource.loop = true;
            backgroundMusicSource.playOnAwake = false;
        }

        if (clickAudioSource == null)
        {
            clickAudioSource = gameObject.AddComponent<AudioSource>();
            clickAudioSource.playOnAwake = false;
        }

        if (craftWindow != null)
        {
            craftWindow.gameObject.SetActive(false);
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.C)) { OpenCharacterUI(); }
    //    //if (Input.GetKeyDown(KeyCode.K)) { OpenCookUI(); }        // Use 'K' for cooking UI
    //    //if (Input.GetKeyDown(KeyCode.C)) { OpenCraftingUI(); }    // Use 'C' for crafting UI
    //    if (Input.GetKeyDown(KeyCode.O)) { OpenOptionsUI(); }
    //}

    //private void AddButtonListeners(List<Button> buttons, UnityEngine.Events.UnityAction action)
    //{
    //    foreach (Button button in buttons)
    //    {
    //        button.onClick.AddListener(action);
    //    }
    //}

    // Add hover effect to a single button
    private void AddHoverEffect(Button button)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        // OnPointerEnter
        EventTrigger.Entry entryHoverEnter = new EventTrigger.Entry();
        entryHoverEnter.eventID = EventTriggerType.PointerEnter;
        entryHoverEnter.callback.AddListener((eventData) => OnHoverEnter(button));
        trigger.triggers.Add(entryHoverEnter);

        // OnPointerExit
        EventTrigger.Entry entryHoverExit = new EventTrigger.Entry();
        entryHoverExit.eventID = EventTriggerType.PointerExit;
        entryHoverExit.callback.AddListener((eventData) => OnHoverExit(button));
        trigger.triggers.Add(entryHoverExit);
    }

    // Add hover effects to multiple buttons
    private void AddHoverEffects(List<Button> buttons)
    {
        foreach (Button button in buttons)
        {
            AddHoverEffect(button);
        }
    }

    // Set initial opacity for buttons in a list
    private void SetInitialButtonOpacities(List<Button> buttons)
    {
        foreach (Button button in buttons)
        {
            SetInitialButtonOpacity(button);
        }
    }

    // Set initial opacity for a single button
    private void SetInitialButtonOpacity(Button button)
    {
        Color buttonColor = button.image.color;
        buttonColor.a = 0.6f; // Set lower opacity as initial state (unhovered)
        button.image.color = buttonColor;
    }

    private void OnHoverEnter(Button button)
    {
        // Play hover sound
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }

        // Change button opacity on hover
        Color buttonColor = button.image.color;
        buttonColor.a = 1.0f; // Full opacity on hover
        button.image.color = buttonColor;
    }

    private void OnHoverExit(Button button)
    {
        // Revert button opacity when no longer hovering
        Color buttonColor = button.image.color;
        buttonColor.a = 0.6f; // Lower opacity when not hovered
        button.image.color = buttonColor;
    }



    

    // Close the currently active UI
    private void CloseUI()
    {
        if (activeUI != null)
        {
            activeUI.SetActive(false);
            activeUI = null;
            activeButtons = null;

            if (closeSound != null) audioSource.PlayOneShot(closeSound);

            ShowGameScreenUIs(); // Restore game screen UIs

            // Reset cursor and controls via InputFreezeManager
            if (InputFreezeManager.instance != null)
            {
                InputFreezeManager.instance.UnfreezePlayerAndCursor();
            }

            // Stop any background music
            if (backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.Stop();
            }

            // **Hide the tooltip here**
            if (InventoryTooltipManager.instance != null)
            {
                InventoryTooltipManager.instance.HideTooltip();
            }
            else
            {
                Debug.LogWarning("InventoryTooltipManager.instance is null. Ensure it's initialized correctly.");
            }
        }
    }

    // Close a specific UI
    public void CloseSpecificUI(GameObject uiPanel)
    {
        Debug.Log("CloseSpecificUI before IF");
        if (uiPanel.activeSelf)
        {
            Debug.Log("CloseSpecificUI after IF");
            uiPanel.SetActive(false);

            if (closeSound != null) audioSource.PlayOneShot(closeSound);

            // If the closed UI is the currently active one, reset the activeUI and activeButtons
            if (uiPanel == activeUI)
            {
                activeUI = null;
                activeButtons = null;
            }

            ShowGameScreenUIs();

            // Reset cursor and controls via InputFreezeManager
            if (InputFreezeManager.instance != null)
            {
                InputFreezeManager.instance.UnfreezePlayerAndCursor();
            }
            // Re-enable opening CharacterUI
            UIManager.instance.EnableCharacterUIOpening();

            // Re-enable in-game menu
            InGameMenuController.instance.EnableMenuOpening();

            // **Hide the tooltip here**
            if (InventoryTooltipManager.instance != null)
            {
                InventoryTooltipManager.instance.HideTooltip();
            }
            else
            {
                Debug.LogWarning("InventoryTooltipManager.instance is null. Ensure it's initialized correctly.");
            }
        }
    }

    // Update the button images for active and inactive UIs
    private void UpdateButtonImages(List<Button> currentButtons)
    {
        List<List<Button>> allButtonSets = new List<List<Button>> { characterButtons, skillsButtons, cookButtons, craftingButtons, optionsButtons };

        foreach (var buttonSet in allButtonSets)
        {
            foreach (Button button in buttonSet)
            {
                Text buttonText = button.GetComponentInChildren<Text>(); // Assuming the button has a child Text component

                if (buttonSet == currentButtons)
                {
                    button.image.sprite = activeButtonImage; // Set to active image
                    if (buttonText != null) buttonText.color = Color.black; // Set active text color to black
                }
                else
                {
                    button.image.sprite = inactiveButtonImage; // Set to inactive image
                    if (buttonText != null) buttonText.color = Color.white; // Set inactive text color to white
                }
            }
        }
    }

    public bool IsUIActive()
    {
        return characterUI.activeSelf || skillsUI.activeSelf || cookUI.activeSelf || craftingUI.activeSelf || optionsUI.activeSelf;
    }


    private void HideGameScreenUIs()
    {
        foreach (var ui in gameScreenUIs)
        {
            if (ui.activeSelf) ui.SetActive(false); // Hide any active game screen UIs
        }
    }

    private void ShowGameScreenUIs()
    {
        // Show the cross‑platform (or Windows) game UI normally
        foreach (var ui in gameScreenUIs)
        {
            if (!ui.activeSelf)
            {
                ui.SetActive(true);
            }
        }

        // Show Android-only UI if we are running on an Android device
        if (Application.platform == RuntimePlatform.Android)
        {
            foreach (var androidUI in androidOnlyUIs)
            {
                if (!androidUI.activeSelf)
                {
                    androidUI.SetActive(true);
                }
            }
        }
        //else
        //{
        //    // For safety, ensure Android-only UI stays hidden if we are *not* on Android
        //    // (in case they were visible from the Editor or something)
        //    foreach (var androidUI in androidOnlyUIs)
        //    {
        //        if (androidUI.activeSelf)
        //        {
        //            androidUI.SetActive(false);
        //        }
        //    }
        //}
    }


    private void EnableBlur()
    {
        if (depthOfField != null)
        {
            depthOfField.active = true;
        }
    }

    // Disable blur effect
    private void DisableBlur()
    {
        if (depthOfField != null)
        {
            depthOfField.active = false;
        }
    }


    public void OpenSpecificUI(GameObject uiPanel)
    {
        // If the clicked UI is already active, close it
        if (activeUI == uiPanel)
        {
            CloseUI();
            
        }
        else
        {
            

            // Get the UIAudioSettings for the uiPanel
            UIAudioSettings uiAudioSettings = GetUIAudioSettings(uiPanel);

            // Play the opening sound for this UI
            if (uiAudioSettings != null && uiAudioSettings.openingSound != null)
            {
                audioSource.PlayOneShot(uiAudioSettings.openingSound);
            }
            else if (defaultOpenSound != null)
            {
                // Play the default open sound if no specific opening sound is set
                audioSource.PlayOneShot(defaultOpenSound);
            }

            // Stop any currently playing background music
            if (backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.Stop();
            }

            // Play the background music for this UI
            if (uiAudioSettings != null && uiAudioSettings.backgroundMusic != null)
            {
                backgroundMusicSource.clip = uiAudioSettings.backgroundMusic;
                backgroundMusicSource.loop = true;
                backgroundMusicSource.Play();
            }

            if (activeUI != null) activeUI.SetActive(false); // Hide the previously active UI

            HideGameScreenUIs(); // Hide game screen UIs

            // Open the specified UI
            uiPanel.SetActive(true);
            activeUI = uiPanel;
            activeButtons = null;

            if (InputFreezeManager.instance != null)
            {
                InputFreezeManager.instance.FreezePlayerAndCursor();
            }

            EnableBlur();
        }
    }


    private UIAudioSettings GetUIAudioSettings(GameObject uiPanel)
    {
        return uiAudioSettingsList.Find(settings => settings.uiPanel == uiPanel);
    }

    // Modify the ToggleUI method
    private void ToggleUI(GameObject uiPanel, List<Button> buttons)
    {

        // Are we *closing* the same panel that is active?
        bool isClosing = (activeUI == uiPanel);

        // If the clicked UI is already active, close it
        if (isClosing)
        {
            // We're closing the currently active UI
            CloseUI(); // Or the block of logic you already have for closing

            // Specifically, if the UI being closed is the Character UI...
            if (uiPanel == characterUI)
            {
                // Now the in‑game menu can be opened again
                InGameMenuController.instance.EnableMenuOpening();
            }
        }
        else
        {
            

            // Get the UIAudioSettings for the uiPanel
            UIAudioSettings uiAudioSettings = GetUIAudioSettings(uiPanel);

            // Play the opening sound for this UI
            if (uiAudioSettings != null && uiAudioSettings.openingSound != null)
            {
                audioSource.PlayOneShot(uiAudioSettings.openingSound);
            }
            else if (defaultOpenSound != null)
            {
                // Play the default open sound if no specific opening sound is set
                audioSource.PlayOneShot(defaultOpenSound);
            }

            // Stop any currently playing background music
            if (backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.Stop();
            }

            // Play the background music for this UI
            if (uiAudioSettings != null && uiAudioSettings.backgroundMusic != null)
            {
                backgroundMusicSource.clip = uiAudioSettings.backgroundMusic;
                backgroundMusicSource.loop = true;
                backgroundMusicSource.Play();
            }

            if (activeUI != null) activeUI.SetActive(false); // Hide the previously active UI

            HideGameScreenUIs(); // Hide game screen UIs

            // Open the newly clicked UI
            uiPanel.SetActive(true);
            activeUI = uiPanel;
            activeButtons = buttons;

            if (uiPanel == characterUI)
            {
                // Disable the menu so that ESC won't open it while Char UI is up
                InGameMenuController.instance.DisableMenuOpening();
            }

            // Hide the craft window when opening Crafting or Cooking UI
            if ((uiPanel == craftingUI || uiPanel == cookUI) && craftWindow != null)
            {
                craftWindow.gameObject.SetActive(false);
            }

            // Update button images based on active/inactive state
            UpdateButtonImages(buttons);

            if (InputFreezeManager.instance != null)
            {
                InputFreezeManager.instance.FreezePlayerAndCursor();
            }

            EnableBlur();
        }
    }


    public void OpenCharacterUI()
    {
        if (!canOpenCharacterUI)
        {
            Debug.Log("Cannot open Character UI at this time.");
            return;
        }
        ToggleUI(characterUI, characterButtons);

        
    }
    private void OpenSkillsUI() => ToggleUI(skillsUI, skillsButtons);
    private void OpenCookUI()
    {
        ToggleUI(cookUI, cookButtons);
        

        if (craftWindow != null)
        {
            craftWindow.gameObject.SetActive(false); // Hide the craft window
            
        }
    }
    private void OpenCraftingUI()
    {
        ToggleUI(craftingUI, craftingButtons);
        


        if (craftWindow != null)
        {
            craftWindow.gameObject.SetActive(false); // Hide the craft window
        }
    }
    private void OpenOptionsUI() => ToggleUI(optionsUI, optionsButtons);

    private void OnButtonClicked(UnityEngine.Events.UnityAction action)
    {
        Debug.Log("OnButtonClicked called.");

        if (clickSound != null)
        {
            Debug.Log("Playing click sound.");
            if (clickAudioSource != null)
            {
                clickAudioSource.PlayOneShot(clickSound);
            }
            else
            {
                Debug.LogWarning("Click AudioSource is null.");
            }
        }
        else
        {
            Debug.LogWarning("Click sound is null.");
        }

        // Invoke the original action
        action.Invoke();
    }



    private void AddButtonListeners(List<Button> buttons, UnityEngine.Events.UnityAction action)
    {
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => OnButtonClicked(action));
        }
    }


    private void AddButtonClickListener(Button button, UnityEngine.Events.UnityAction action)
    {
        button.onClick.AddListener(() => OnButtonClicked(action));
    }


    private void OnEnable()
    {
        inputActions.UI.Enable();
    }

    private void OnDisable()
    {
        inputActions.UI.Disable();
    }

    public void DisableCharacterUIOpening()
    {
        canOpenCharacterUI = false;
    }

    public void EnableCharacterUIOpening()
    {
        canOpenCharacterUI = true;
    }

}
