using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LensFollowView : MonoBehaviour
{
    public Transform headCamera;  
    public Transform lensPlane;    

    [Header("Tuning")]
    public float backOffset = 0.03f;       
    public float parallaxStrength = 2.5f;   
    public float maxOnPlaneOffset = 0.03f;  
    void Start()
    {
        if (!headCamera && Camera.main) headCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!headCamera || !lensPlane) return;


        transform.rotation = headCamera.rotation;

        Vector3 planePoint = lensPlane.position;
        Vector3 n = lensPlane.forward;
        if (n.sqrMagnitude < 0.0001f) return;
        n.Normalize();

        Vector3 eye = headCamera.position;

             float d = Vector3.Dot(eye - planePoint, n);
        if (float.IsNaN(d) || float.IsInfinity(d)) return;

        Vector3 projected = eye - n * d;


        Vector3 onPlaneOffset = (projected - planePoint) * parallaxStrength;

         if (onPlaneOffset.magnitude > maxOnPlaneOffset)
            onPlaneOffset = onPlaneOffset.normalized * maxOnPlaneOffset;

        transform.position = planePoint + onPlaneOffset - n * backOffset;
    }
}
