using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance;  // Singleton instance

    [Header("UI Elements")]
    public GameObject tooltipUI;              // Root GameObject of the tooltip UI
    public Image tooltipImage;                // Tooltip image (background)
    public TextMeshProUGUI tooltipText;       // Action prompt text
    public TextMeshProUGUI locationText;      // Location text
    public TextMeshProUGUI objectNameText;    // Object name text
    public Image iconImage;                   // Optional icon image

    [Header("Fade Settings")]
    public float fadeInTime = 0.1f;    // Time to fade in
    public float fadeOutTime = 0.1f;   // Time to fade out

    private CanvasGroup canvasGroup;   // To control fading of tooltip
    private Coroutine fadeCoroutine;   // Reference to the fade coroutine
    private bool isFadingOut = false;

    private void Awake()
    {
        // Ensure there's only one instance of TooltipManager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Get or add CanvasGroup for fading
        canvasGroup = tooltipUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tooltipUI.AddComponent<CanvasGroup>();
        }

        // Initialize tooltip as hidden
        canvasGroup.alpha = 0f;
        tooltipUI.SetActive(false);  // Ensure it's inactive at start
        isFadingOut = false;
    }

    // Method to show the tooltip indefinitely for interaction case
    public void ShowTooltipIndefinitely(string actionText, string location, string objectName, Sprite icon = null)
    {
        //Debug.Log($"ShowTooltipIndefinitely: actionText={actionText}, location={location}, objectName={objectName}");

        // Interrupt any ongoing fade-out and show the new tooltip
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // Reset isFadingOut since we're showing a new tooltip
        isFadingOut = false;

        // Update tooltip content
        tooltipText.text = actionText;
        locationText.text = location;
        locationText.gameObject.SetActive(!string.IsNullOrEmpty(location));
        objectNameText.text = objectName;
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        // Ensure the tooltip is fully visible indefinitely
        tooltipUI.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    // Method to hide the tooltip
    public void HideTooltip()
    {
        //Debug.Log("HideTooltip called");

        if (isFadingOut)
        {
            //Debug.Log("Already fading out, ignoring additional HideTooltip call.");
            return;
        }

        // Start the fade-out coroutine
        fadeCoroutine = StartCoroutine(FadeOutTooltip());
    }

    private IEnumerator FadeOutTooltip()
    {
        if (isFadingOut)
        {
            yield break; // Already fading out
        }

        isFadingOut = true;
        //Debug.Log("FadeOutTooltip started");

        float elapsedTime = 0f;
        float fadeDuration = Mathf.Max(fadeOutTime, 0.0001f);
        float startingAlpha = canvasGroup.alpha; // Use current alpha in case of interruptions

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startingAlpha, 0f, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure alpha is set to 0 and tooltip is hidden
        canvasGroup.alpha = 0f;
        tooltipUI.SetActive(false);
        //Debug.Log("FadeOutTooltip completed, tooltip hidden");

        isFadingOut = false;
        fadeCoroutine = null;
    }
}
