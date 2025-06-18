using UnityEngine;
using UnityEngine.EventSystems;

public class TouchRegionInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Tooltip("If true, this panel controls movement; if false, it controls look.")]
    public bool isMovementPanel = false;

    private Vector2 startPointerPos;
    private Vector2 currentPointerPos;
    private bool isDragging = false;

    // Adjust this sensitivity to taste. Higher means small finger movements translate to bigger in-game movement/looking
    public float sensitivity = 0.01f;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out startPointerPos
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out currentPointerPos
        );

        Vector2 delta = currentPointerPos - startPointerPos;

        // Convert delta to input direction
        Vector2 inputDir;
        if (isMovementPanel)
        {
            // For movement, invert Y-axis (or adjust as needed)
            inputDir = new Vector2(delta.x * sensitivity, delta.y * sensitivity);
            PlayerControllerCode.instance.SetMoveDirection(inputDir);
            Debug.Log($"inputDir = {inputDir}");
        }
        else
        {
            // For camera look, leave the axes as is
            inputDir = delta * sensitivity;
            CameraController.instance.SetLookInput(inputDir);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        // Reset input to zero on release
        if (isMovementPanel)
        {
            PlayerControllerCode.instance.SetMoveDirection(Vector2.zero);
        }
        else
        {
            CameraController.instance.SetLookInput(Vector2.zero);
        }
    }
}
