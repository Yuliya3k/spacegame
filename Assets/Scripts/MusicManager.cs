using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    public AudioSource audioSource;

    // Volume can be set from the inspector or through the SetVolume method.
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Set initial volume
        audioSource.volume = musicVolume;
    }

    /// <summary>
    /// Play a background music clip.
    /// </summary>
    /// <param name="clip">The AudioClip to play</param>
    /// <param name="loop">Should the clip loop?</param>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (audioSource == null) return;

        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }

    /// <summary>
    /// Set the music volume (0 to 1).
    /// </summary>
    /// <param name="volume">New volume level</param>
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = musicVolume;
        }
    }
}

