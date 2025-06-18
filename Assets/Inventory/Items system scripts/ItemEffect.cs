using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Item", menuName = "Items/ItemEffect")]
public class ItemEffect : ScriptableObject
{
    public int appleVolume;
    public int appleCalories;

    public virtual void ExecuteEffect()
    {
        Debug.Log("Effect executed");

    }
}
