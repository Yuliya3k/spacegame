using UnityEngine;
using UnityEngine.Audio; // For audio mixing
using System.Collections.Generic;
using TMPro; // For TextMeshPro components
using UnityEngine.UI; // Still needed for Slider

public class OptionsManager : MonoBehaviour
{
    [Header("Resolution Settings")]
    public TMP_Dropdown resolutionDropdown;    // Assign in the Editor
    public Toggle fullscreenToggle;            // Assign in the Editor

    [Header("Graphics Settings")]
    public TMP_Dropdown qualityDropdown;       // Assign in the Editor

    [Header("View Range Settings")]
    public Slider viewDistanceSlider;          // Assign in the Editor
    public TMP_Text viewDistanceValueText;     // Assign in the Editor
    public float minViewDistance = 50f;
    public float maxViewDistance = 500f;
    public Camera mainCamera;                  // Assign in the Editor

    [Header("Audio Settings")]
    public Slider masterVolumeSlider;          // Assign in the Editor
    public Slider musicVolumeSlider;           // Assign in the Editor
    public Slider effectsVolumeSlider;         // Assign in the Editor
    public AudioMixer audioMixer;              // Assign in the Editor

    private Resolution[] allResolutions;
    private List<Resolution> filteredResolutions;
    private int currentResolutionIndex = 0;


    [Header("Mouse Sensitivity Settings")]
    public Slider mouseSensitivitySlider;       // Assign in the Editor
    public TMP_Text mouseSensitivityValueText;  // Assign in the Editor
    public float minMouseSensitivity = 0.1f;
    public float maxMouseSensitivity = 5f;      // Adjust max value as needed

    [Header("Camera Options")]
    public Toggle invertYToggle;
    //private void Start()
    //{
    //    // Initialize settings
    //    InitializeResolutionSettings();
    //    InitializeQualitySettings();
    //    InitializeViewDistanceSettings();
    //    InitializeAudioSettings();
    //    InitializeMouseSensitivitySettings();

    //    // Load saved settings
    //    LoadSettings();
    //}

    private void OnEnable()
    {
        Debug.Log("OptionsManager OnEnable called.");

        // Initialize settings
        InitializeResolutionSettings();
        InitializeQualitySettings();
        InitializeViewDistanceSettings();
        InitializeAudioSettings();
        InitializeMouseSensitivitySettings();
        InitializeInvertYSetting();

        // Check if slider and text are assigned
        Debug.Log("mouseSensitivitySlider is " + (mouseSensitivitySlider != null ? "assigned." : "null!"));
        Debug.Log("mouseSensitivityValueText is " + (mouseSensitivityValueText != null ? "assigned." : "null!"));


        // Load saved settings
        LoadSettings();
    }

    private void InitializeResolutionSettings()
    {
        allResolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);

        for (int i = 0; i < allResolutions.Length; i++)
        {
            Resolution res = allResolutions[i];

            // Filter out resolutions below 1920x1080
            if (res.width < 1920 || res.height < 1080)
            {
                continue;
            }

            string option = $"{res.width} x {res.height} @ {res.refreshRate}Hz";
            options.Add(option);
            filteredResolutions.Add(res);

            // Check if this resolution is the current screen resolution
            if (res.width == Screen.currentResolution.width &&
                res.height == Screen.currentResolution.height &&
                res.refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolutionIndex = filteredResolutions.Count - 1;
            }
        }

        // If the current resolution is below 1920x1080, add it to the list
        if (currentResolutionIndex == 0 && (Screen.currentResolution.width < 1920 || Screen.currentResolution.height < 1080))
        {
            Resolution currentRes = Screen.currentResolution;
            string option = $"{currentRes.width} x {currentRes.height} @ {currentRes.refreshRate}Hz";
            options.Insert(0, option);
            filteredResolutions.Insert(0, currentRes);
            currentResolutionIndex = 0;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Add listener for when the value changes
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        // Set fullscreen toggle
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < filteredResolutions.Count)
        {
            Resolution resolution = filteredResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen, resolution.refreshRate);
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    private void InitializeQualitySettings()
    {
        string[] qualityNames = QualitySettings.names;
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(qualityNames));

        int currentQualityLevel = QualitySettings.GetQualityLevel();
        qualityDropdown.value = currentQualityLevel;
        qualityDropdown.RefreshShownValue();

        // Add listener
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }

    private void InitializeViewDistanceSettings()
    {
        // Set slider range
        viewDistanceSlider.minValue = minViewDistance;
        viewDistanceSlider.maxValue = maxViewDistance;

        // Set initial value
        float currentViewDistance = mainCamera.farClipPlane;
        viewDistanceSlider.value = currentViewDistance;
        viewDistanceValueText.text = $"{currentViewDistance:F0} units";

        // Add listener
        viewDistanceSlider.onValueChanged.AddListener(SetViewDistance);
    }

    public void SetViewDistance(float distance)
    {
        mainCamera.farClipPlane = distance;
        viewDistanceValueText.text = $"{distance:F0} units";
        PlayerPrefs.SetFloat("ViewDistance", distance);
    }

    private void InitializeAudioSettings()
    {
        // Load saved volumes or set to default (0 dB)
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);
        float effectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 0f);

        masterVolumeSlider.value = masterVolume;
        musicVolumeSlider.value = musicVolume;
        effectsVolumeSlider.value = effectsVolume;

        // Apply volumes to audio mixer
        audioMixer.SetFloat("MasterVolume", masterVolume);
        audioMixer.SetFloat("MusicVolume", musicVolume);
        audioMixer.SetFloat("EffectsVolume", effectsVolume);

        // Add listeners
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        effectsVolumeSlider.onValueChanged.AddListener(SetEffectsVolume);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetEffectsVolume(float volume)
    {
        audioMixer.SetFloat("EffectsVolume", volume);
        PlayerPrefs.SetFloat("EffectsVolume", volume);
    }

    public void LoadSettings()
    {
        // Load Resolution
        int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        resolutionDropdown.value = savedResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        SetResolution(savedResolutionIndex);

        // Load Fullscreen
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        fullscreenToggle.isOn = isFullscreen;
        SetFullscreen(isFullscreen);

        // Load Quality Level
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        qualityDropdown.value = qualityLevel;
        qualityDropdown.RefreshShownValue();
        SetQuality(qualityLevel);

        // Load View Distance
        float viewDistance = PlayerPrefs.GetFloat("ViewDistance", mainCamera.farClipPlane);
        viewDistanceSlider.value = viewDistance;
        SetViewDistance(viewDistance);

        // Load Mouse Sensitivity
        float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        mouseSensitivitySlider.value = sensitivity;
        SetMouseSensitivity(sensitivity);

        // Load Invert Y
        bool invert = PlayerPrefs.GetInt("InvertY", 0) == 1;
        if (invertYToggle != null)
        {
            invertYToggle.isOn = invert;
        }
        SetInvertY(invert);
        // Audio settings are loaded in InitializeAudioSettings()
    }

    public void SaveSettings()
    {
        // PlayerPrefs are updated in each setter method
        // Optionally, you can call PlayerPrefs.Save() here to ensure settings are saved
        PlayerPrefs.Save();
    }



    private void InitializeMouseSensitivitySettings()
    {
        Debug.Log("InitializeMouseSensitivitySettings called.");
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        mouseSensitivitySlider.minValue = minMouseSensitivity;
        mouseSensitivitySlider.maxValue = maxMouseSensitivity;
        mouseSensitivitySlider.value = savedSensitivity;

        mouseSensitivityValueText.text = savedSensitivity.ToString("F1");

        // Remove existing listeners to prevent duplicates
        mouseSensitivitySlider.onValueChanged.RemoveAllListeners();

        // Add listener with debug log
        mouseSensitivitySlider.onValueChanged.AddListener((float sensitivity) =>
        {
            Debug.Log("Slider value changed: " + sensitivity);
            SetMouseSensitivity(sensitivity);
        });

        //mouseSensitivitySlider.onValueChanged.AddListener((float sensitivity) =>
        //{
        //    SetMouseSensitivity(sensitivity);
        //});

        // Apply the saved sensitivity at start
        SetMouseSensitivity(savedSensitivity);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        //Debug.Log("SetMouseSensitivity called with sensitivity: " + sensitivity);

        ICameraControl cam = null;
        foreach (var mb in FindObjectsOfType<MonoBehaviour>())
        {
            if (mb is ICameraControl control)
            {
                cam = control;
                break;
            }
        }
        if (cam is CameraController legacy)
        {
            legacy.SetMouseSensitivity(sensitivity);
        }
        else if (cam is CinemachineCameraController cine)
        {
            cine.SetMouseSensitivity(sensitivity);
        }
        else
        {
            Debug.LogWarning("No camera controller available.");
        }

        mouseSensitivityValueText.text = sensitivity.ToString("F1");
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
    }

    private void InitializeInvertYSetting()
    {
        bool invert = PlayerPrefs.GetInt("InvertY", 0) == 1;
        if (invertYToggle != null)
        {
            invertYToggle.isOn = invert;
            invertYToggle.onValueChanged.AddListener(SetInvertY);
        }

        SetInvertY(invert);
    }

    public void SetInvertY(bool invert)
    {
        if (CinemachineCameraController.instance != null)
        {
            CinemachineCameraController.instance.SetInvertY(invert);
        }
        else
        {
            Debug.LogWarning("CinemachineCameraController instance is not available.");
        }

        PlayerPrefs.SetInt("InvertY", invert ? 1 : 0);
    }

}
