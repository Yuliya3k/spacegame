using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ActionProgressUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas canvas;
    public Image bgnImage; // Background for action name
    public TextMeshProUGUI actionNameText;
    public Image progressBarImage; // Circular progress bar

    [Header("Flickering Settings")]
    public float flickerFadeInDuration = 0.5f;
    public float flickerFadeOutDuration = 0.5f;
    public float flickerInterval = 1f; // Time between flickers

    [Header("Progress Settings")]
    public bool isProgressing = false;

    private Coroutine flickerCoroutine;
    private Coroutine progressCoroutine;

    private void Awake()
    {
        // Ensure the canvas is enabled when the action starts
        canvas.enabled = false;
    }

    public void StartAction(string actionName, float actionDuration)
    {
        Debug.Log("Start ActionProgress");
        actionNameText.text = actionName;
        canvas.enabled = true;

        // Reset progress bar
        progressBarImage.fillAmount = 0f;

        // Start flickering
        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);
        flickerCoroutine = StartCoroutine(FlickerRoutine());

        // Start progress bar
        if (progressCoroutine != null)
            StopCoroutine(progressCoroutine);
        progressCoroutine = StartCoroutine(ProgressRoutine(actionDuration));
    }

    public void StopAction()
    {
        Debug.Log("Stop ActionProgress");
        // Stop flickering smoothly
        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);
        StartCoroutine(StopFlickerRoutine());

        // Stop progress bar
        if (progressCoroutine != null)
            StopCoroutine(progressCoroutine);

        // Optionally, hide the UI after the action
        StartCoroutine(HideUIAfterDelay());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Fade in
            yield return StartCoroutine(FadeCanvasGroup(bgnImage.canvasRenderer, 0f, 1f, flickerFadeInDuration));
            yield return new WaitForSeconds(flickerInterval);

            // Fade out
            yield return StartCoroutine(FadeCanvasGroup(bgnImage.canvasRenderer, 1f, 0f, flickerFadeOutDuration));
            yield return new WaitForSeconds(flickerInterval);
        }
    }

    private IEnumerator StopFlickerRoutine()
    {
        // Finish the current fade-in or fade-out cycle smoothly
        float currentAlpha = bgnImage.canvasRenderer.GetAlpha();
        float remainingFadeDuration = currentAlpha > 0.5f ? flickerFadeOutDuration * (currentAlpha / 1f) : flickerFadeInDuration * (currentAlpha / 1f);

        yield return StartCoroutine(FadeCanvasGroup(bgnImage.canvasRenderer, currentAlpha, 0f, remainingFadeDuration));

        // Ensure the background is fully transparent
        bgnImage.canvasRenderer.SetAlpha(0f);
        actionNameText.canvasRenderer.SetAlpha(0f);
    }

    private IEnumerator ProgressRoutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            progressBarImage.fillAmount = progress;
            yield return null;
        }

        // Ensure the progress bar is full
        progressBarImage.fillAmount = 1f;
    }

    private IEnumerator FadeCanvasGroup(CanvasRenderer renderer, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            renderer.SetAlpha(alpha);
            // Also set alpha for the action name text
            if (renderer.gameObject == bgnImage.gameObject)
            {
                actionNameText.canvasRenderer.SetAlpha(alpha);
            }
            yield return null;
        }

        renderer.SetAlpha(endAlpha);
        if (renderer.gameObject == bgnImage.gameObject)
        {
            actionNameText.canvasRenderer.SetAlpha(endAlpha);
        }
    }

    private IEnumerator HideUIAfterDelay()
    {
        // Wait for a short delay to ensure smooth fade-out
        yield return new WaitForSeconds(0.1f);
        canvas.enabled = false;
    }
}
