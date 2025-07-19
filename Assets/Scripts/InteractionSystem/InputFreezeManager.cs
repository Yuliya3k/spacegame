using UnityEngine;

public class InputFreezeManager : MonoBehaviour
{
    public static InputFreezeManager instance;

    private bool isFrozen = false;

    public bool IsFrozen => isFrozen;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance == null)
        {
            var obj = new GameObject(nameof(InputFreezeManager));
            instance = obj.AddComponent<InputFreezeManager>();
        }
    }
    public void FreezePlayerAndCursor()
    {
        if (PlayerControllerCode.instance != null)
        {
            PlayerControllerCode.instance.DisablePlayerControl();
        }

        if (CameraController.instance != null)
        {
            CameraController.instance.DisableCameraControl();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        isFrozen = true;
    }

    public void UnfreezePlayerAndCursor()
    {
        if (PlayerControllerCode.instance != null)
        {
            PlayerControllerCode.instance.EnablePlayerControl();
        }

        if (CameraController.instance != null)
        {
            CameraController.instance.EnableCameraControl();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isFrozen = false;
    }
}