using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LensFollowView : MonoBehaviour
{
    public Transform headCamera;   // XR Origin/Main Camera
    public Transform lensPlane;    // LensPlane (شیشه)

    [Header("Tuning")]
    public float backOffset = 0.03f;        // پشت شیشه
    public float parallaxStrength = 2.5f;   // 1 تا 4
    public float maxOnPlaneOffset = 0.03f;  // حداکثر لغزش روی شیشه (متر)

    void Start()
    {
        if (!headCamera && Camera.main) headCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!headCamera || !lensPlane) return;

        // نگاه = نگاه سر
        transform.rotation = headCamera.rotation;

        Vector3 planePoint = lensPlane.position;
        Vector3 n = lensPlane.forward;
        if (n.sqrMagnitude < 0.0001f) return;
        n.Normalize();

        Vector3 eye = headCamera.position;

        // projection امن
        float d = Vector3.Dot(eye - planePoint, n);
        if (float.IsNaN(d) || float.IsInfinity(d)) return;

        Vector3 projected = eye - n * d;

        // آفست روی صفحه
        Vector3 onPlaneOffset = (projected - planePoint) * parallaxStrength;

        // ✅ جلوگیری از رفتن دوربین "برای خودش"
        if (onPlaneOffset.magnitude > maxOnPlaneOffset)
            onPlaneOffset = onPlaneOffset.normalized * maxOnPlaneOffset;

        transform.position = planePoint + onPlaneOffset - n * backOffset;
    }
}
