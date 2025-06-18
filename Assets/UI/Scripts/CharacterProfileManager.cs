using UnityEngine;

public class CharacterProfileManager : MonoBehaviour
{
    public static CharacterProfileManager instance;

    public CharacterProfile chosenProfile = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetChosenCharacter(CharacterProfile profile)
    {
        chosenProfile = profile;
    }
}
