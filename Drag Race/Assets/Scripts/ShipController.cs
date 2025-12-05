using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Gear
{
    public string name = "1";
    [Tooltip("Multiplier used to compute target RPM from ship speed")]
    public float ratio = 3.5f;
    [Tooltip("Curve: input 0..1 => normalized RPM; shape controls torque across RPM.")]
    public AnimationCurve torqueCurve = AnimationCurve.Linear(0, 0.2f, 1, 0.1f);
    [Tooltip("Base peak torque for this gear (Newton-like units for our simple sim)")]
    public float maxTorque = 2000f;
}

[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    [Header("Engine / Gearing")]
    public List<Gear> gears = new List<Gear>(6);
    public int currentGearIndex = 0;
    public float maxRPM = 7000f;
    public float idleRPM = 800f;

    [Header("Zones (RPM)")]
    public float silverZoneStartRPM = 4200f;
    public float goldZoneStartRPM = 5200f;
    public float redZoneStartRPM = 6000f;

    [Header("RPM dynamics")]
    public float rpmRiseSpeed = 2200f;
    public float rpmFallSpeed = 1500f; 
    [HideInInspector] public float engineRPM = 0f;

    [Header("Drive")]
    public float throttle = 0f; 
    public float speedDrag = 0.1f;
    public float baseForceMultiplier = 1f; // global scale

    [Header("Shift multipliers")]
    public float goldShiftMultiplier = 1.25f;
    public float silverShiftMultiplier = 1.0f;
    public float earlyShiftMultiplier = 0.6f; // lugging effect

    [Header("References")]
    public Rigidbody rb;
    public Transform shipModel; // for stretch
    public Camera mainCam;
    public EngineVFX engineVFX; // particle toggles
    public TachometerUI tachometerUI;

    // internal
    private float currentShiftMultiplier = 1f;
    private bool gasHeld = false;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (gears.Count == 0)
        {
            // default quick gears (you can tune in inspector)
            gears = new List<Gear>()
            {
                new Gear(){ name="1", ratio=4.0f, maxTorque=700f, torqueCurve=CreateDefaultCurve(3500f, 5000f, 1f) },
                new Gear(){ name="2", ratio=3.0f, maxTorque=1600f, torqueCurve=CreateDefaultCurve(4000f, 5500f, 1.1f) },
                new Gear(){ name="3", ratio=2.2f, maxTorque=1200f, torqueCurve=CreateDefaultCurve(4200f, 5600f, 0.95f) },
                new Gear(){ name="4", ratio=1.6f, maxTorque=1800f, torqueCurve=CreateDefaultCurve(4300f, 5800f, 1.05f) },
                new Gear(){ name="5", ratio=1.1f, maxTorque=2000f, torqueCurve=CreateDefaultCurve(4400f, 6000f, 1.0f) },
                new Gear(){ name="6", ratio=0.8f, maxTorque=1700f, torqueCurve=CreateDefaultCurve(4500f, 6200f, 0.95f) },
            };
        }
        engineRPM = idleRPM;
    }
 
    private static AnimationCurve CreateDefaultCurve(float peakRpm, float beyondRpm, float height)
    {
        
        return new AnimationCurve(
            new Keyframe(0f, 0.2f),
            new Keyframe(0.5f, height),
            new Keyframe(1f, 0.1f)
        );
    }

    void Update()
    {
        // For UI refresh and non-physics per-frame actions
        if (tachometerUI != null) tachometerUI.UpdateTachometer(engineRPM, maxRPM, currentGearIndex + 1);

        // simple ship model stretch based on current torque
        if (shipModel != null)
        {
            float stretch = 1f + Mathf.Clamp((CurrentTorqueNormalized() - 0.5f) * 0.4f, 0f, 0.6f);
            shipModel.localScale = Vector3.Lerp(shipModel.localScale, new Vector3(1f, 1f, stretch), Time.deltaTime * 6f);
        }

        // camera effects: update via EngineVFX (handles FOV toggles, contrails)
        if (engineVFX != null) engineVFX.SetIntensity(CurrentTorqueNormalized());
    }

    void FixedUpdate()
    {
      
        float gearMaxRPM = maxRPM; 
        float targetRPM = Mathf.Lerp(idleRPM, gearMaxRPM, throttle);

        float speedFactor = rb.linearVelocity.magnitude * (1f / Mathf.Max(0.0001f, gears[currentGearIndex].ratio));
       
        float speedContribution = Mathf.Clamp01(speedFactor * 0.05f);
        targetRPM = Mathf.Lerp(idleRPM, gearMaxRPM, Mathf.Max(throttle, speedContribution));

        if (throttle > 0.01f)
            engineRPM = Mathf.MoveTowards(engineRPM, targetRPM, rpmRiseSpeed * Time.fixedDeltaTime);
        else
            engineRPM = Mathf.MoveTowards(engineRPM, idleRPM, rpmFallSpeed * Time.fixedDeltaTime);

        // compute torque from current gear curve
        Gear g = gears[currentGearIndex];
        float rpmNormalized = Mathf.Clamp01(engineRPM / maxRPM);
        float curveValue = g.torqueCurve.Evaluate(rpmNormalized); 
        float baseTorque = curveValue * g.maxTorque;

        float appliedTorque = baseTorque * currentShiftMultiplier;

        // simple drag
        Vector3 drag = -rb.linearVelocity * speedDrag;
        rb.AddForce(drag, ForceMode.Acceleration);

        // apply forward acceleration
        Vector3 forwardForce = transform.forward * appliedTorque * baseForceMultiplier * Time.fixedDeltaTime;
        rb.AddForce(forwardForce, ForceMode.Acceleration);

      
    }

    // Called by UI Gas input
    public void SetGas(bool held)
    {
        gasHeld = held;
        throttle = held ? 1f : 0f;
    }

    // Called by UI shift button
    public void TryShiftUp()
    {
        // Evaluate zone where shift happens
        float rpm = engineRPM;
        float multiplier = silverShiftMultiplier;
        if (rpm >= goldZoneStartRPM)
        {
            multiplier = goldShiftMultiplier;
            currentShiftMultiplier = multiplier;
        }
        else if (rpm >= silverZoneStartRPM)
        {
            multiplier = silverShiftMultiplier;
            currentShiftMultiplier = multiplier;
        }
        else
        {
            // early shift -> lug
            multiplier = earlyShiftMultiplier;
            currentShiftMultiplier = multiplier;
            // small rpm drop to show lug effect
            engineRPM = Mathf.Max(idleRPM, engineRPM * 0.7f);
        }

        // Shift logic: advance gear if possible
        if (currentGearIndex < gears.Count - 1)
        {
            // apply slight rpm drop on shift (realistic behavior)
            engineRPM = Mathf.Clamp(engineRPM * 0.6f + 200f, idleRPM, maxRPM);
            currentGearIndex++;
        }

        // update UI if present
        if (tachometerUI != null) tachometerUI.OnShift(currentGearIndex + 1, multiplier);
    }

    public float CurrentTorqueNormalized()
    {
        Gear g = gears[currentGearIndex];
        float rpmNorm = Mathf.Clamp01(engineRPM / maxRPM);
        float val = g.torqueCurve.Evaluate(rpmNorm);
       
        return Mathf.Clamp01(val / 1.5f);
    }
}
