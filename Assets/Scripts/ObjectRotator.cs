using UnityEngine;
using UnityEngine.UI;  // Required for UI elements

public class ObjectRotator : MonoBehaviour
{
    [Header("Object to Rotate")]
    [Tooltip("Assign the GameObject you want to rotate.")]
    public GameObject objectToRotate;  // The object that will be rotated

    [Header("Rotation Amount")]
    [Tooltip("Degrees to rotate per button click.")]
    public float rotationAmount = 30f;  // Rotate by 30 degrees per click

    [Header("UI Buttons")]
    [Tooltip("Button to rotate clockwise.")]
    public Button rotateClockwiseButton;

    [Tooltip("Button to rotate counterclockwise.")]
    public Button rotateCounterclockwiseButton;

    void Start()
    {
        // Ensure the object to rotate is assigned
        if (objectToRotate == null)
        {
            Debug.LogError("Object to rotate is not assigned.");
            return;
        }

        // Assign the button events
        if (rotateClockwiseButton != null)
        {
            rotateClockwiseButton.onClick.AddListener(RotateClockwise);
        }

        if (rotateCounterclockwiseButton != null)
        {
            rotateCounterclockwiseButton.onClick.AddListener(RotateCounterclockwise);
        }
    }

    // Method to rotate the object clockwise
    public void RotateClockwise()
    {
        objectToRotate.transform.Rotate(0f, rotationAmount, 0f);  // Rotate around the Y-axis
    }

    // Method to rotate the object counterclockwise
    public void RotateCounterclockwise()
    {
        objectToRotate.transform.Rotate(0f, -rotationAmount, 0f);  // Rotate around the Y-axis
    }
}
