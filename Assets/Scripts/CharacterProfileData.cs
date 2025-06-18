using System;
using System.Collections.Generic;

[Serializable]
public class CharacterProfileData
{
    public string profileName;

    public bool enableBoobGain;
    public bool enableTorsoGain;
    public bool enableThighsGain;
    public bool enableShinsGain;
    public bool enableArmsGain;
    public bool enableWholeBodyGain;
    public bool enableGlutesGain;

    //public List<BlendShapeSettingData> baseBlendShapes;
    // You can also store baseBlendShapes or other fields if you want.
    // e.g. a list of (blendShapeName, value) pairs if the user modifies them in the UI, etc.
    public List<BlendShapeSettingData> baseBlendShapes = new List<BlendShapeSettingData>();
}
