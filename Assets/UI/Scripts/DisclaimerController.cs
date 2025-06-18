using UnityEngine;
using UnityEngine.UI;

public class DisclaimerController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject disclaimerUI;       // The parent panel containing the disclaimer
    public Button acceptButton;
    public Button doNotAcceptButton;

    [Header("References to Other UI")]
    public GameObject startMenuUI;        // The Start Menu UI (assign from the scene)
    public StartMenuController startMenu; // The existing StartMenuController

    private void Start()
    {
        // Initially, we want disclaimer to be active, so show it:
        disclaimerUI.SetActive(true);

        // Hide the start menu so it doesn't appear under the disclaimer
        if (startMenuUI != null)
            startMenuUI.SetActive(false);

        // Hook up button events
        acceptButton.onClick.AddListener(OnAcceptClicked);
        doNotAcceptButton.onClick.AddListener(OnDoNotAcceptClicked);

        // Lock or show cursor as needed (often you want to show it for disclaimer)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pause the game
        Time.timeScale = 0f;
    }

    private void OnAcceptClicked()
    {
        // Hide the disclaimer
        disclaimerUI.SetActive(false);

        // Resume time
        Time.timeScale = 1f;

        // Show the start menu UI
        //  (either directly or using the StartMenuController’s ShowStartMenu method).
        if (startMenu != null)
        {
            startMenu.ShowStartMenu();
        }
        else
        {
            // fallback if needed
            if (startMenuUI != null)
                startMenuUI.SetActive(true);
        }
    }

    private void OnDoNotAcceptClicked()
    {
        // Quit the game
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
