using UnityEngine;

public class GameManager : MonoBehaviour
{
    public SaveSystem saveSystem; // Assign in the Editor

    void Start()
    {
        if (PlayerPrefs.GetInt("LoadGame", 0) == 1)
        {
            saveSystem.LoadGame();
            PlayerPrefs.SetInt("LoadGame", 0); // Reset the flag
            PlayerPrefs.Save();
        }
    }
}
