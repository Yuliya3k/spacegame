using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour, ICameraControl
{
    [SerializeField] Transform followTarget;

    // Input Actions
    private PlayerInputActions inputActions;
    private Vector2 lookInput;


    // Camera Profiles
    [System.Serializable]
    public class CameraProfile
    {
        public float distance = 5;
        public float cameraHeight = 1;
        public float rotationSpeed = 2;
        public float minVerticalAngle = -45;
        public float maxVerticalAngle = 45;
        public Vector3 framingOffset = new Vector3(0, 1, 0);
        public bool invertX = false;
        public bool invertY = false;
        public float viewDistance = 50f;          // Adjustable view distance per profile
        public float interactionDistance = 3f;    // Interaction distance
        public float horizontalOffset = 0f;       // **New parameter for horizontal offset**
        public bool hideHead = false;             // Hide head when this profile is active
    }

    [Header("Camera Profiles")]
    public CameraProfile profile1;
    public CameraProfile profile2;
    public CameraProfile profile3;
    public CameraProfile profileSleep;
    //public CameraProfile profileFirstPerson; // new first-person profile

    // Current profile values
    float distance;
    float cameraHeight;
    float rotationSpeed;
    float minVerticalAngle;
    float maxVerticalAngle;
    Vector3 framingOffset;
    bool invertX;
    bool invertY;
    float viewDistance;
    float horizontalOffset; // **New variable**

    float rotationX;
    float rotationY;
    float invertXVal;
    float invertYVal;

    private CameraProfile currentProfile;
    private Camera mainCamera;
    private Camera activeSleepCamera;

    // Add collision smoothing factor
    public float collisionSmoothing = 10f;

    // Fog settings
    [Header("Fog Settings")]
    public bool enableFog = true;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.02f;

    // Reference to PlayerInteraction
    private PlayerInteraction playerInteraction;

    private bool isCameraControlEnabled = true;

    private float mouseSensitivityMultiplier = 1f;

    public static CameraController instance;

    [Header("UI Panels that Freeze Camera")]
    public List<GameObject> freezeCameraUIs = new List<GameObject>();

    [Header("First Person Settings")]
    public List<Renderer> headRenderers = new List<Renderer>();
    public float profileTransitionDuration = 0.5f;

    private Coroutine profileTransitionCoroutine;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        inputActions = new PlayerInputActions();

        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.SwitchToCameraProfile1.performed += OnSwitchToCameraProfile1;
        inputActions.Player.SwitchToCameraProfile2.performed += OnSwitchToCameraProfile2;
        inputActions.Player.SwitchToCameraProfile3.performed += OnSwitchToCameraProfile3;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.SwitchToCameraProfile1.performed -= OnSwitchToCameraProfile1;
        inputActions.Player.SwitchToCameraProfile2.performed -= OnSwitchToCameraProfile2;
        inputActions.Player.SwitchToCameraProfile3.performed -= OnSwitchToCameraProfile3;
    }



    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Initialize with Profile 1 by default
        SwitchToProfile(profile1);
        SetHeadVisible(!profile1.hideHead);

        // Initialize rotationY to match the character's Y rotation
        rotationY = followTarget.eulerAngles.y;

        // Initialize fog settings
        RenderSettings.fog = enableFog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

        // Find the PlayerInteraction script
        playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction == null)
        {
            Debug.LogError("PlayerInteraction script not found in the scene.");
        }
    }

    private void Update()
    {
        //Debug.Log("CameraUpdate");
        // Update camera's far clip plane based on the current profile
        mainCamera.farClipPlane = viewDistance;

        if (!isCameraControlEnabled)
        {
            return;
        }

        //// If a UI is currently active, prevent camera movement
        //if (UIManager.instance != null && UIManager.instance.IsUIActive())
        //{
        //    return; // Skip the rest of the camera update if a UI is active
        //}

        //if (StorageUIManager.instance != null && StorageUIManager.instance.storageUI.activeSelf)
        //{
        //    return; // Stop camera movement if Storage UI is active
        //}

        //if (SleepUIManager.instance != null && SleepUIManager.instance.sleepUIPanel.activeSelf)
        //{
        //    return; // Skip camera updates when sleep UI is active
        //}

        foreach (var uiPanel in freezeCameraUIs)
        {
            if (uiPanel != null && uiPanel.activeSelf)
            {
                return; // This prevents camera rotation/position updates
            }
        }

        //// Handle switching between profiles
        //inputActions.Player.SwitchToCameraProfile1.performed += ctx => SwitchToProfile(profile1);
        //inputActions.Player.SwitchToCameraProfile2.performed += ctx => SwitchToProfile(profile2);
        //inputActions.Player.SwitchToCameraProfile3.performed += ctx => SwitchToProfile(profile3);

        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    SwitchToProfile(profile1);
        //}

        // Handle camera rotation and positioning
        HandleCameraRotation();
        UpdateCameraPosition();
    }

    private void SwitchToProfile(CameraProfile profile)

    {
        if (profileTransitionCoroutine != null)
        {
            StopCoroutine(profileTransitionCoroutine);
        }
        profileTransitionCoroutine = StartCoroutine(TransitionToProfile(profile));
    }

    private IEnumerator TransitionToProfile(CameraProfile profile)
    {
        currentProfile = profile;
        // Apply the settings from the selected profile
        distance = profile.distance;
        cameraHeight = profile.cameraHeight;

        float startDistance = distance;
        float startHeight = cameraHeight;
        float startOffset = horizontalOffset;
        float elapsed = 0f;

        // Apply non-lerped settings immediately
        rotationSpeed = profile.rotationSpeed;
        minVerticalAngle = profile.minVerticalAngle;
        maxVerticalAngle = profile.maxVerticalAngle;
        framingOffset = profile.framingOffset;
        invertX = profile.invertX;
        invertY = profile.invertY;
        viewDistance = profile.viewDistance;
        horizontalOffset = profile.horizontalOffset; // **Assign horizontalOffset**

        SetHeadVisible(!profile.hideHead);

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionDistance(profile.interactionDistance);
        }

        while (elapsed < profileTransitionDuration)
        {
            float t = elapsed / profileTransitionDuration;
            distance = Mathf.Lerp(startDistance, profile.distance, t);
            cameraHeight = Mathf.Lerp(startHeight, profile.cameraHeight, t);
            horizontalOffset = Mathf.Lerp(startOffset, profile.horizontalOffset, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        distance = profile.distance;
        cameraHeight = profile.cameraHeight;
        horizontalOffset = profile.horizontalOffset;
    }

    private void HandleCameraRotation()
    {
        // Handle camera rotation using the input system
        invertXVal = (invertX) ? -1 : 1;
        invertYVal = (invertY) ? -1 : 1;

        rotationX += lookInput.y * invertYVal * rotationSpeed * mouseSensitivityMultiplier * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        rotationY += lookInput.x * invertXVal * rotationSpeed * mouseSensitivityMultiplier * Time.deltaTime;
    }

    private void UpdateCameraPosition()
    {
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        // Transform the framingOffset to be relative to the character's local space
        Vector3 offset = followTarget.TransformDirection(new Vector3(framingOffset.x, framingOffset.y + cameraHeight, framingOffset.z));

        var focusPosition = followTarget.position + offset;

        // Adjust the desired camera position to include the horizontal offset
        Vector3 desiredCameraPosition = focusPosition - targetRotation * new Vector3(horizontalOffset, 0, distance);

        // Perform a sphere cast from the target towards the desired camera position
        RaycastHit hit;
        Vector3 direction = (desiredCameraPosition - focusPosition).normalized;
        float maxDistance = distance;

        // Layer mask to exclude certain layers if needed
        int layerMask = ~LayerMask.GetMask("Player"); // Exclude the player layer

        if (Physics.SphereCast(focusPosition, 0.2f, direction, out hit, maxDistance, layerMask))
        {
            // Adjust the camera position to the hit point
            float adjustedDistance = hit.distance;
            desiredCameraPosition = focusPosition + direction * (adjustedDistance - 0.2f);
        }

        // Smoothly interpolate to the desired position
        transform.position = Vector3.Lerp(transform.position, desiredCameraPosition, collisionSmoothing * Time.deltaTime);
        transform.rotation = targetRotation;
    }

    public void SwitchToSleepCamera(Camera sleepCamera)
    {
        // Disable camera movement and main camera
        enabled = false;
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }

        if (sleepCamera != null)
        {
            activeSleepCamera = sleepCamera;
            activeSleepCamera.enabled = true;
        }
        else
        {
            Debug.LogWarning("CameraController.SwitchToSleepCamera called with null camera");
        }
    }

    public void SwitchToNormalCamera()
    {
         // Re-enable camera movement and main camera
        enabled = true;
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }
        if (activeSleepCamera != null)
        {
            activeSleepCamera.enabled = false;
            activeSleepCamera = null;
        }
    }

    public void SwitchToDefecationCamera(Transform defecationCameraTransform)
    {
        // Disable camera movement
        enabled = false;

        // Move camera to defecation position
        transform.position = defecationCameraTransform.position;
        transform.rotation = defecationCameraTransform.rotation;
    }

    public void DisableCameraControl()
    {
        isCameraControlEnabled = false;
        Debug.Log("CameraControlDisabled");
    }

    public void EnableCameraControl()
    {
        isCameraControlEnabled = true;
        Debug.Log("CameraControlEnabled");
    }

    public void SwitchToRestCamera(Transform restCameraTransform)
    {
        // Disable camera movement
        enabled = false;

        // Move camera to rest position
        transform.position = restCameraTransform.position;
        transform.rotation = restCameraTransform.rotation;
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        if (isCameraControlEnabled)
        {
            lookInput = context.ReadValue<Vector2>();
        }
        else
        {
            lookInput = Vector2.zero;
        }
    }

    private void OnSwitchToCameraProfile1(InputAction.CallbackContext context)
    {
        SwitchToProfile(profile1);
    }

    private void OnSwitchToCameraProfile2(InputAction.CallbackContext context)
    {
        SwitchToProfile(profile2);
    }

    private void OnSwitchToCameraProfile3(InputAction.CallbackContext context)
    {
        SwitchToProfile(profile3);
    }

    private void SetHeadVisible(bool visible)
    {
        foreach (var r in headRenderers)
        {
            if (r != null)
            {
                r.enabled = visible;
            }
        }
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        //Debug.Log("CameraController - SetMouseSensitivity called with sensitivity: " + sensitivity);
        mouseSensitivityMultiplier = sensitivity;
    }

    public void SetLookInput(Vector2 look)
    {
        lookInput = look;
    }

}
