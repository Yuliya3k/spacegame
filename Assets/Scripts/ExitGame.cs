using UnityEngine;
using UnityEngine.UI;  // Required for UI elements

public class ExitGameController : MonoBehaviour
{
    [Header("Exit Button")]
    [Tooltip("Assign the button that will exit the game.")]
    public Button exitButton;  // The button that will trigger the exit

    void Start()
    {
        // Assign the button event
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
        else
        {
            Debug.LogError("Exit button is not assigned.");
        }
    }

    // Method to exit the application
    public void ExitGame()
    {
        // Exit the application
        Application.Quit();

        // In the editor, stop playing the scene
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
