using UnityEngine;
using UnityEngine.EventSystems;

public class ShiftButtonInput : MonoBehaviour, IPointerClickHandler
{
    public ShipController controller;

    public void OnPointerClick(PointerEventData eventData)
    {
        controller?.TryShiftUp();
    }
}
