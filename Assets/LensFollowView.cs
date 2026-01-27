using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LensFollowView : MonoBehaviour
{
    [Header("References")]
    public Transform headCamera;   // XR Origin/Main Camera (HMD)

    void Start()
    {
        if (!headCamera && Camera.main)
            headCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!headCamera) return;

        // Lens camera must match the eye (so the render texture is from head view)
        transform.position = headCamera.position;
        transform.rotation = headCamera.rotation;
    }
}
