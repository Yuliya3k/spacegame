using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AreaData
{
    public Transform areaTransform; // The area's transform (AreaName)
    public Transform areaIconTransform; // The area icon's parent transform (RoomIcon)
    public SpriteRenderer areaBGSpriteRenderer; // The area icon's BG SpriteRenderer
    public SpriteRenderer areaIconSpriteRenderer; // The area icon's Icon SpriteRenderer

    [HideInInspector]
    public GameObject edgeIconInstance; // The edge icon instance
    [HideInInspector]
    public RectTransform edgeIconRectTransform; // Edge icon RectTransform
    [HideInInspector]
    public EdgeIconComponents edgeIconComponents; // Reference to the EdgeIconComponents script
    [HideInInspector]
    public bool isEdgeIconActive; // Whether the edge icon is currently active
}
