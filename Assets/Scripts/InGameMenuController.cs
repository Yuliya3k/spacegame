using UnityEngine;
using UnityEngine.UI;

public class InGameMenuController : MonoBehaviour
{

    // 1) Provide a static reference to this script
    public static InGameMenuController instance;

    [Header("UI References")]
    public GameObject inGameMenuUI;    // Assign the In-Game Menu UI Panel
    public GameObject gameplayUI;      // Assign the main gameplay UI panel (HUD)
    public Button openMenuButton;      // Assign the UI Button used to open the menu
    public GameObject optionsMenuUI;   // Assign the Options Menu UI Panel

    [Header("References")]
    public SaveSystem saveSystem;      // Assign in the Editor

    private bool isMenuActive = false;
    private bool isOptionsMenuActive = false;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        // 3) If no instance yet, use this one. Otherwise, destroy duplicates
        if (instance == null)
        {
            instance = this;
            // Optionally, if you want this persistent across scenes:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        inputActions = new PlayerInputActions();
        inputActions.UI.Menu.performed += ctx => ToggleInGameMenu();

    }

    private void Start()
    {
        // Ensure the in-game menu and options menu are inactive at the start
        inGameMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);

        // Assign the ToggleInGameMenu method to the button's onClick event
        if (openMenuButton != null)
        {
            openMenuButton.onClick.AddListener(ToggleInGameMenu);
        }

        //// Lock and hide the cursor during gameplay
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        // If options menu is active, close it
    //        if (isOptionsMenuActive)
    //        {
    //            CloseOptions();
    //        }
    //        else
    //        {
    //            ToggleInGameMenu();
    //        }
    //    }
    //}

    public void ToggleInGameMenu()
    {
        Debug.Log("Toggled In Game Menu!");

        isMenuActive = !isMenuActive;
        inGameMenuUI.SetActive(isMenuActive);
        gameplayUI.SetActive(!isMenuActive); // Hide gameplay UI when menu is active

        // Pause or resume the game
        Time.timeScale = isMenuActive ? 0f : 1f;

        // Manage cursor visibility and lock state
        if (isMenuActive)
        {           

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // Disable opening CharacterUI
            UIManager.instance.DisableCharacterUIOpening();
        }
        else
        {  

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Re-enable opening CharacterUI
            UIManager.instance.EnableCharacterUIOpening();
        }
    }

    public void SaveGame()
    {
        saveSystem.SaveGame();
    }

    public void LoadGame()
    {
        saveSystem.LoadGame();
        // Optionally, you can reload the scene or update game state
    }

    public void OpenOptions()
    {
        //inGameMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
        isOptionsMenuActive = true;

        // Cursor should remain visible and unlocked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        //inGameMenuUI.SetActive(true);
        isOptionsMenuActive = false;

        // Cursor should remain visible and unlocked in the in-game menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }



    public void ExitGame()
    {
        // Quit the application
        Application.Quit();

        // Note: In the Unity Editor, Application.Quit() does not stop play mode.
        // To test quitting in the editor, you can add:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    private void OnEnable()
    {
        inputActions.UI.Enable();
    }

    private void OnDisable()
    {
        inputActions.UI.Disable();
    }


    public void DisableMenuOpening()
    {
        inputActions.UI.Menu.Disable();
    }

    public void EnableMenuOpening()
    {
        inputActions.UI.Menu.Enable();
    }




}
