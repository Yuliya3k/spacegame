using System;

[System.Serializable]
public class StatModifier
{
    public string statName; // Name of the stat (e.g., "strength", "agility")
    public float valueChange; // How much the stat will change
}

[System.Serializable]
public class BlendShapeModifier
{
    public string blendShapeName; // Name of the blend shape
    public float blendShapeValueChange; // How much the blend shape will change
}

namespace ModifiersNamespace
{
    [System.Serializable]
    public class StatModifier
    {
        public string statName; // Name of the stat (e.g., "strength", "agility")
        public float valueChange; // How much the stat will change
    }

    [System.Serializable]
    public class BlendShapeModifier
    {
        public string blendShapeName; // Name of the blend shape
        public float blendShapeValueChange; // How much the blend shape will change
    }
}