using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DisableFogForMinimapCamera : MonoBehaviour
{
    private bool originalFogState;

    void OnPreRender()
    {
        // Store the original fog state
        originalFogState = RenderSettings.fog;
        // Disable fog
        RenderSettings.fog = false;
    }

    void OnPostRender()
    {
        // Restore the original fog state
        RenderSettings.fog = originalFogState;
    }
}
