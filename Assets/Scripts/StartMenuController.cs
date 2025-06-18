using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    public GameObject startMenuUI;     // Assign the Start Menu Canvas
    public GameObject inGameUI;        // Assign the In-Game UI Canvas
    public SaveSystem saveSystem;      // Assign in the Editor
    public GameObject optionsMenuUI;   // Assign the Options Menu UI Panel
    public CharacterSelectUI characterSelectUI; // Assign in the Inspector

    private bool isOptionsMenuActive = false;

    public InGameMenuController inGameMenuController; // Assign in the Inspector


    private void Start()
    {
        // Ensure the Start Menu is active and In-Game UI is inactive
        ShowStartMenu();
    }

    public void ShowStartMenu()
    {
        // Disable opening CharacterUI
        UIManager.instance.DisableCharacterUIOpening();

        // Disable in-game menu opening
        inGameMenuController.DisableMenuOpening();

        // Activate the Start Menu UI and deactivate in-game UI
        startMenuUI.SetActive(true);
        inGameUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 0f; // Pause the game

        // Show and unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        // Re-enable opening CharacterUI
        //UIManager.instance.EnableCharacterUIOpening();

        // Enable in-game menu opening
        inGameMenuController.EnableMenuOpening();

        // Deactivate the Start Menu and activate the In-Game UI
        PlayerInventory.instance.InitializeStartingEquipment();
        startMenuUI.SetActive(false);

        if (characterSelectUI != null)
        {
            characterSelectUI.OpenCharacterSelectUI();
        }
        else
        {
            Debug.LogError("CharacterSelectUI not assigned in StartMenuController!");
        }

        optionsMenuUI.SetActive(false);
        //inGameUI.SetActive(true);
        //Time.timeScale = 1f; // Resume the game

        //// Lock and hide the cursor during gameplay
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    public void LoadGame()
    {
        // Re-enable opening CharacterUI
        UIManager.instance.EnableCharacterUIOpening();

        // Enable in-game menu opening
        inGameMenuController.EnableMenuOpening();

        // Load the game data
        saveSystem.LoadGame();

        // Deactivate the Start Menu and activate the In-Game UI
        startMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        inGameUI.SetActive(true);
        Time.timeScale = 1f; // Resume the game

        // Lock and hide the cursor during gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenOptions()
    {
        //startMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
        isOptionsMenuActive = true;

        // Cursor should remain visible and unlocked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        //startMenuUI.SetActive(true);
        isOptionsMenuActive = false;

        // Cursor should remain visible and unlocked in the start menu
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
}
