using UnityEngine;
using UnityEngine.UI;

public class TachometerUI : MonoBehaviour
{
    [Header("UI refs")]
    public Image tachFill; // fill image, type = Filled, FillMethod = Horizontal
    public RectTransform silverZoneRect;
    public RectTransform goldZoneRect;
    public RectTransform redZoneRect;
    public Text gearText;
    public Text rpmText;

    [Header("Configuration")]
    public float maxRPM = 7000f;
    public float silverStart = 4200f;
    public float goldStart = 5200f;
    public float redStart = 6000f;

    private float cachedWidth;

    void Start()
    {
        if (tachFill != null) cachedWidth = ((RectTransform)tachFill.transform).rect.width;
        LayoutZones();
    }

    public void LayoutZones()
    {
        // Compute zone positions within tach (works if tachFill is a simple rect)
        if (tachFill == null) return;
        RectTransform tachRect = (RectTransform)tachFill.transform;
        float w = tachRect.rect.width;
        cachedWidth = w;

        float silverX = Mathf.Clamp01(silverStart / maxRPM) * w;
        float goldX = Mathf.Clamp01(goldStart / maxRPM) * w;
        float redX = Mathf.Clamp01(redStart / maxRPM) * w;

        // position thin boxes before red zone
        SetZone(silverZoneRect, silverX, 0.03f * w);
        SetZone(goldZoneRect, goldX, 0.05f * w);
        SetZone(redZoneRect, redX, 0.06f * w);
    }

    void SetZone(RectTransform rt, float centerX, float width)
    {
        if (rt == null) return;
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, centerX - width * 0.5f, width);
    }

    public void UpdateTachometer(float rpm, float maxR, int gear)
    {
        if (tachFill != null) tachFill.fillAmount = Mathf.Clamp01(rpm / maxR);
        if (rpmText != null) rpmText.text = Mathf.RoundToInt(rpm).ToString() + " RPM";
        if (gearText != null) gearText.text = "G" + gear;
    }

    // Called on shift for temporary flash or UI animation
    public void OnShift(int newGear, float multiplier)
    {
        // small flash or update
        if (gearText != null) gearText.text = $"G{newGear}";
    }
}
