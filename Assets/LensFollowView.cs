using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LensFollowView : MonoBehaviour
{
    [Header("References")]
    public Transform lensCenter;   // The physical Glass object
    public Transform headCamera;   // Your VR Headset Camera (Main Camera)

    [Header("Zoom Settings")]
    [Range(5f, 60f)]
    public float zoomFov = 15f;    // Lower number = Higher Magnification

    private Camera _lensCam;

    void Awake()
    {
        _lensCam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (lensCenter == null || headCamera == null) return;

        // 1. POSITION: Stick the camera to the center of the glass
        transform.position = lensCenter.position;

        // 2. ROTATION: The "Window" Effect
        // Calculate the direction from your Eye -> The Glass
        Vector3 directionToLens = transform.position - headCamera.position;

        // Force the camera to look along that direction.
        // using 'headCamera.up' keeps it stable so it doesn't spin wildly.
        if (directionToLens != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToLens, headCamera.up);
        }

        // 3. ZOOM: Ensure the zoom stays locked
        if (_lensCam != null)
        {
            _lensCam.fieldOfView = zoomFov;
        }
    }
}