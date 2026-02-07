using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LensFollowView : MonoBehaviour
{
    [Header("References")]
    public Transform lensCenter;   // Empty at lens center
    public Transform headCamera;   // XR Main Camera

    [Header("Zoom")]
    [Range(5f, 120f)]
    public float zoomFov = 20f;

    Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void Start()
    {
        if (!headCamera && Camera.main) headCamera = Camera.main.transform;
    }

void LateUpdate()
    {
        if (!lensCenter || !headCamera) return;

        // 1) Position: Stick to the lens center
        transform.position = lensCenter.position;

        // 2) Rotation: Look FROM the player's head THROUGH the lens
        // Calculate the vector from Head -> Lens
        Vector3 directionToLens = transform.position - headCamera.position;

        // Rotate the camera to look along that vector
        // We use 'headCamera.up' to make sure the camera rolls when your head tilts
        transform.rotation = Quaternion.LookRotation(directionToLens, headCamera.up);

        if (_cam && !_cam.orthographic)
            _cam.fieldOfView = zoomFov;
    }
}