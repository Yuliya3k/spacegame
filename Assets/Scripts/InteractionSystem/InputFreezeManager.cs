using UnityEngine;

public class InputFreezeManager : MonoBehaviour
{
    public static InputFreezeManager instance;

    private bool isFrozen = false;

    private ICameraControl cameraControl;

    public bool IsFrozen => isFrozen;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            FindCameraControl();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void FindCameraControl()
    {
        foreach (var mb in FindObjectsOfType<MonoBehaviour>())
        {
            if (mb is ICameraControl control)
            {
                var behaviour = mb as Behaviour;
                if (behaviour != null && behaviour.isActiveAndEnabled)
                {
                    cameraControl = control;
                    break;
                }
            }
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

        if (cameraControl == null)
        {
            FindCameraControl();
        }

        cameraControl?.DisableCameraControl();

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

        if (cameraControl == null)
        {
            FindCameraControl();
        }

        cameraControl?.EnableCameraControl();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isFrozen = false;
    }
}