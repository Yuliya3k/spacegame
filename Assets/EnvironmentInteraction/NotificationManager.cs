using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager instance;

    [Header("Notification Prefab")]
    public GameObject notificationPrefab; // Assign via inspector
    public GameObject sleepNotificationPrefab; // New prefab for sleep notifications


    [Header("Default Notification Settings")]
    public Vector2 startAnchoredPosition = new Vector2(0f, -200f); // Starting position (relative to center)
    public Vector2 endAnchoredPosition = new Vector2(0f, 0f);      // Ending position (relative to center)
    public float moveDuration = 2f;       // Duration of the movement
    public float fadeDuration = 0.5f;     // Duration of the fade in/out
    public float displayDuration = 0f;    // Time to display before fading out (default to 0)

    private void Awake()
    {
        instance = this;
    }

    public void ShowNotification(
        string message,
        Color? textColor = null,
        Color? backgroundColor = null,
        Sprite backgroundSprite = null,
        Vector2? startPos = null,
        Vector2? endPos = null,
        float? moveDur = null,
        float? fadeDur = null,
        float? displayDur = null,
        GameObject customPrefab = null // New parameter
    )
    {
        StartCoroutine(ShowNotificationCoroutine(
            message,
            textColor ?? Color.white,
            backgroundColor ?? new Color(0f, 0f, 0f, 0.5f),
            backgroundSprite,
            startPos ?? startAnchoredPosition,
            endPos ?? endAnchoredPosition,
            moveDur ?? moveDuration,
            fadeDur ?? fadeDuration,
            displayDur ?? displayDuration,
            customPrefab
        ));
    }

    private IEnumerator ShowNotificationCoroutine(
        string message,
        Color textColor,
        Color backgroundColor,
        Sprite backgroundSprite,
        Vector2 startPos,
        Vector2 endPos,
        float moveDur,
        float fadeDur,
        float displayDur,
        GameObject customPrefab // New parameter
    )
    {
        Debug.Log("ShowNotificationCoroutine started.");

        // Create the notification instance
        GameObject notificationGO = Instantiate(notificationPrefab, transform);
        Debug.Log("Notification instantiated.");

        CanvasGroup canvasGroup = notificationGO.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = notificationGO.AddComponent<CanvasGroup>();
            Debug.Log("CanvasGroup component added to notification.");
        }

        // Get references to background image and text
        Transform backgroundTransform = notificationGO.transform.Find("Background");
        if (backgroundTransform == null)
        {
            Debug.LogError("Notification prefab is missing a 'Background' child.");
            yield break;
        }

        RectTransform backgroundRectTransform = backgroundTransform.GetComponent<RectTransform>();

        Image backgroundImage = backgroundTransform.GetComponent<Image>();
        Transform textTransform = backgroundTransform.Find("Text");
        if (textTransform == null)
        {
            Debug.LogError("Background is missing a 'Text' child.");
            yield break;
        }

        TextMeshProUGUI notificationText = textTransform.GetComponent<TextMeshProUGUI>();

        // Set the message
        notificationText.text = message;
        Debug.Log($"Notification message set to: {message}");

        // Set colors and sprite
        notificationText.color = textColor;
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
            if (backgroundSprite != null)
            {
                backgroundImage.sprite = backgroundSprite;
                Debug.Log("Background sprite set.");
            }
        }

        // Set the starting position
        backgroundRectTransform.anchoredPosition = startPos;
        Debug.Log($"Notification starting position set to: {startPos}");

        float elapsedTime = 0f;

        // Movement and fade-in/fade-out happen together
        while (elapsedTime < moveDur)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled delta time
            float t = Mathf.Clamp01(elapsedTime / moveDur);

            // Apply easing to t
            float easedT = EaseInOutExpo(t);

            // Update position with eased t
            Vector2 position = Vector2.Lerp(startPos, endPos, easedT);
            backgroundRectTransform.anchoredPosition = position;

            // Update alpha
            float alpha = 1f;

            if (displayDur == 0f)
            {
                // Fade-in and fade-out during movement
                if (elapsedTime <= fadeDur)
                {
                    // Fade-in
                    alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDur);
                }
                else if (elapsedTime >= moveDur - fadeDur)
                {
                    // Fade-out
                    float fadeOutTime = elapsedTime - (moveDur - fadeDur);
                    alpha = Mathf.Lerp(1f, 0f, fadeOutTime / fadeDur);
                }
                else
                {
                    // Middle part, fully visible
                    alpha = 1f;
                }
            }
            else
            {
                // Fade-in during movement
                if (elapsedTime <= fadeDur)
                {
                    alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDur);
                }
                else
                {
                    alpha = 1f;
                }
            }

            canvasGroup.alpha = alpha;

            yield return null;
        }

        // Ensure final position and alpha are set
        backgroundRectTransform.anchoredPosition = endPos;
        canvasGroup.alpha = 1f;

        if (displayDur > 0f)
        {
            // Wait for display duration
            yield return StartCoroutine(WaitForSecondsUnscaled(displayDur));

            // Fade-out after display duration
            float elapsedFadeOutTime = 0f;
            while (elapsedFadeOutTime < fadeDur)
            {
                elapsedFadeOutTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedFadeOutTime / fadeDur);

                // Update fade-out
                float alpha = Mathf.Lerp(1f, 0f, t);
                canvasGroup.alpha = alpha;

                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        else
        {
            // Ensure alpha is 0 at the end
            canvasGroup.alpha = 0f;
        }

        // Destroy the notification
        Destroy(notificationGO);
        Debug.Log("Notification destroyed.");
    }

    private IEnumerator WaitForSecondsUnscaled(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private float EaseInOutExpo(float t)
    {
        if (t == 0f || t == 1f)
        {
            return t;
        }
        else if (t < 0.5f)
        {
            return 0.5f * Mathf.Pow(2f, (20f * t) - 10f);
        }
        else
        {
            return -0.5f * Mathf.Pow(2f, (-20f * t) + 10f) + 1f;
        }
    }
}