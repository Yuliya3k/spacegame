using UnityEngine;

[System.Serializable]
public class DoorSettings
{
    public Transform doorTransform;  // Reference to the door object
    public Vector3 openPositionOffset;  // Offset for this door (positive or negative)
    [HideInInspector]
    public Vector3 initialPosition;  // Store the initial position
}
