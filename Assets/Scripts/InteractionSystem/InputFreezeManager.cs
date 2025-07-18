using UnityEngine;

public class InputFreezeManager : MonoBehaviour
{
    public static InputFreezeManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
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
    }
}