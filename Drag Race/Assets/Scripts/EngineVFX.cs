using UnityEngine;

public class EngineVFX : MonoBehaviour
{
    public ParticleSystem afterburner;

    [Header("Color switch")]
    [Range(0f, 1f)] public float switchThreshold = 0.5f; // center threshold
    [Tooltip("Small deadzone to avoid flicker around the threshold")]
    [Range(0f, 0.2f)] public float hysteresis = 0.05f;
    [Tooltip("Color used at low intensity (orange)")]
    public Color lowColor = new Color(1f, 0.5f, 0f); // orange
    [Tooltip("Color used at high intensity (blue)")]
    public Color highColor = new Color(0.2f, 0.6f, 1f); // blue

    // internal state to apply hysteresis
    bool usingHighColor = false;

  
    public void SetIntensity(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        if (afterburner == null) return;

        var main = afterburner.main;
        main.startSize = Mathf.Lerp(1.4f, 2f, normalized);
        main.startSpeed = Mathf.Lerp(1f, 6f, normalized);

        // Play/stop control
        if (!afterburner.isPlaying && normalized > 0.1f) afterburner.Play();
        if (afterburner.isPlaying && normalized <= 0.01f) afterburner.Stop();

       
        float highThreshold = Mathf.Clamp01(switchThreshold + hysteresis * 0.5f);
        float lowThreshold = Mathf.Clamp01(switchThreshold - hysteresis * 0.5f);

        if (usingHighColor)
        {
            if (normalized < lowThreshold) usingHighColor = false;
        }
        else
        {
            if (normalized > highThreshold) usingHighColor = true;
        }

        Color chosen = usingHighColor ? highColor : lowColor;

        var colModule = afterburner.colorOverLifetime;

        Gradient existing = null;
        try
        {
            existing = colModule.color.gradient;
        }
        catch
        {
            existing = null;
        }

        GradientAlphaKey[] alphaKeys;
        if (existing != null && existing.alphaKeys != null && existing.alphaKeys.Length > 0)
        {
            alphaKeys = existing.alphaKeys;
        }
        else
        {
            // fallback alpha keys: opaque at start, fade to zero at end
            alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            };
        }

        // create color keys that keep color constant across lifetime (no intermediate hues)
        GradientColorKey[] colorKeys = new GradientColorKey[] {
            new GradientColorKey(chosen, 0f),
            new GradientColorKey(chosen, 1f)
        };

        Gradient g = new Gradient();
        g.SetKeys(colorKeys, alphaKeys);

        colModule.color = new ParticleSystem.MinMaxGradient(g);

        main.startColor = chosen;
    }
}
