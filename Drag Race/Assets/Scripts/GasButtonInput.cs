using UnityEngine;
using UnityEngine.EventSystems;

public class GasButtonInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public ShipController controller;

    public void OnPointerDown(PointerEventData eventData)
    {
        controller?.SetGas(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        controller?.SetGas(false);
    }
}
