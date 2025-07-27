
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraRig : MonoBehaviour
{
    public CinemachineCamera thirdPersonCamera;
    public CinemachineCamera firstPersonCamera;

    private bool inFirstPerson = false;

    private void Start()
    {
        SetThirdPerson();
    }

    public void ToggleView()
    {
        if (inFirstPerson)
        {
            SetThirdPerson();
        }
        else
        {
            SetFirstPerson();
        }
    }

    public void SetThirdPerson()
    {
        inFirstPerson = false;
        if (thirdPersonCamera != null) thirdPersonCamera.Priority = 10;
        if (firstPersonCamera != null) firstPersonCamera.Priority = 0;
    }

    public void SetFirstPerson()
    {
        inFirstPerson = true;
        if (firstPersonCamera != null) firstPersonCamera.Priority = 10;
        if (thirdPersonCamera != null) thirdPersonCamera.Priority = 0;
    }
}