using UnityEngine;

public class InputFreezeManager : MonoBehaviour
{
    public static InputFreezeManager instance;

    private int freezeCount = 0;

    private ICameraControl cameraControl;

    public bool IsFrozen => freezeCount > 0;

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
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb is ICameraControl control)
            {
                cameraControl = control;
                break;
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
        freezeCount++;

        if (freezeCount == 1)
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
        }
    }

    public void UnfreezePlayerAndCursor()
    {
        if (freezeCount == 0)
            return;

        freezeCount--;

        if (freezeCount == 0)
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
        }
    }
}