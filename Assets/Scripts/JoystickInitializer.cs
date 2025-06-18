using UnityEngine;

public class JoystickInitializer : MonoBehaviour
{
    [SerializeField] private GameObject joystickCanvas;

    void Start()
    {
#if UNITY_ANDROID
        joystickCanvas.SetActive(true);
#else
            joystickCanvas.SetActive(false);
#endif
    }
}
