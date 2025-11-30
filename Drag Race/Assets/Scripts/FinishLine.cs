using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FinishLine : MonoBehaviour
{
    public TextMeshProUGUI finishText;
    public bool finished = false;

    void Start()
    {
        if (finishText != null) finishText.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (finished) return;
        // assume player is tagged "Player" or has ShipController
        var sc = other.GetComponentInChildren<ShipController>();
        if (sc != null)
        {
            finished = true;
            if (finishText != null)
            {
                finishText.enabled = true;
                finishText.text = "FINISH!";
            }

            // optional: disable input by cutting throttle
            sc.SetGas(false);
            // optionally freeze the ship
            var rb = other.GetComponent<Rigidbody>();
            if (rb != null) rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
