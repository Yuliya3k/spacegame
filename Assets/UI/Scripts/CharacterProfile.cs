using System.Collections.Generic;

[System.Serializable]
public class CharacterProfile
{
    public string characterName;
    public List<BlendShapeSetting> baseBlendShapes;
    public string description;

    public bool enableBoobGain;
    public bool enableTorsoGain;
    public bool enableThighsGain;
    public bool enableShinsGain;
    public bool enableArmsGain;
    public bool enableWholeBodyGain;
    public bool enableGlutesGain;
    // Each BlendShapeSetting is e.g. { blendShapeName: "NoseWidth", value: 20f }

    // Potentially more fields here:
    // e.g. public Sprite icon; // an icon to display in UI
    // e.g. public string description;
}
