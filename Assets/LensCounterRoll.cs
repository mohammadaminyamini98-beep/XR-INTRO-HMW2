using UnityEngine;

public class LensCounterRoll : MonoBehaviour
{
    [Header("References")]
    public Transform headCamera;         // XR Main Camera (HMD)
    public Transform magnifierHandle;    // Magnifier root or handle transform
    public Renderer lensRenderer;        // Renderer on the LensPlane quad

    [Header("Shader Property")]
    public string angleProperty = "_Angle"; // must exist in shader

    [Header("Tuning")]
    public bool invert = true;           // usually true (cancels roll)

    Material _mat;

    void Start()
    {
        if (!headCamera && Camera.main)
            headCamera = Camera.main.transform;

        if (!lensRenderer)
            lensRenderer = GetComponent<Renderer>();

        if (lensRenderer)
            _mat = lensRenderer.material; // instance material
    }

    void LateUpdate()
    {
        if (!_mat || !magnifierHandle || !headCamera) return;

        // We want: rotating the handle should NOT rotate the picture.
        // So we measure roll difference and cancel it in UV rotation.

        // Take "up" vectors projected onto view plane
        Vector3 viewForward = headCamera.forward;

        Vector3 handleUp = Vector3.ProjectOnPlane(magnifierHandle.up, viewForward).normalized;
        Vector3 worldUp = Vector3.ProjectOnPlane(headCamera.up, viewForward).normalized;

        if (handleUp.sqrMagnitude < 0.0001f || worldUp.sqrMagnitude < 0.0001f)
            return;

        // Signed angle around view forward axis
        float roll = Vector3.SignedAngle(worldUp, handleUp, viewForward);

        // Convert degrees -> radians (shader expects radians)
        float angleRad = roll * Mathf.Deg2Rad;

        if (invert) angleRad = -angleRad;

        _mat.SetFloat(angleProperty, angleRad);
    }
}
