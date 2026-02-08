using UnityEngine;
using UnityEngine.InputSystem;

public class BreakOut : MonoBehaviour
{
    public Transform roomViewPoint;
    public Transform externalViewPoint;
    public InputActionReference action;

    private bool isOutside = false;
    private Transform xrCamera;

    void Start()
    {
        xrCamera = GetComponentInChildren<Camera>().transform;

        MoveTo(roomViewPoint);

        action.action.Enable();
        action.action.performed += (ctx) =>
        {
            MoveTo(isOutside ? roomViewPoint : externalViewPoint);
            isOutside = !isOutside;
        };
    }

    void MoveTo(Transform target)
    {
        if (target == null || xrCamera == null) return;
        Vector3 cameraOffset = xrCamera.position - transform.position;

        transform.position = target.position - cameraOffset;

        var e = transform.eulerAngles;
        e.y = target.eulerAngles.y;
        transform.eulerAngles = e;
    }
}
