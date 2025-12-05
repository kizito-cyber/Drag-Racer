using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -7f);
    public float followSpeed = 8f;
    public float stiffFOV = 70f;
    public float relaxedFOV = 60f;
    public ShipController ship;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = relaxedFOV;
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followSpeed);
        transform.LookAt(target.position + Vector3.up * 0.5f);

        // fov based on torque intensity
        if (ship != null)
        {
            float intensity = ship.CurrentTorqueNormalized();
            cam.fieldOfView = Mathf.Lerp(relaxedFOV, stiffFOV, intensity);
          
            followSpeed = Mathf.Lerp(6f, 12f, intensity);
        }
    }
}
