using UnityEngine;

public class EngineVFX : MonoBehaviour
{
    public ParticleSystem contrail;
    public ParticleSystem afterburner;
    public Light afterburnerLight;

    // intensity 0..1
    public void SetIntensity(float normalized)
    {
        if (contrail != null)
        {
            var em = contrail.emission;
            em.rateOverTime = Mathf.Lerp(0f, 200f, normalized);
        }
        if (afterburner != null)
        {
            var main = afterburner.main;
            main.startSize = Mathf.Lerp(0.1f, 1.5f, normalized);
            main.startSpeed = Mathf.Lerp(1f, 6f, normalized);
            if (!afterburner.isPlaying && normalized > 0.1f) afterburner.Play();
            if (afterburner.isPlaying && normalized <= 0.01f) afterburner.Stop();
        }
        if (afterburnerLight != null)
        {
            afterburnerLight.intensity = Mathf.Lerp(0f, 5f, normalized);
        }
    }
}
