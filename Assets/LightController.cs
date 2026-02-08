using UnityEngine;
using UnityEngine.InputSystem;

public class LightController : MonoBehaviour
{
    private Light lightComponent;

    [Header("Input Action")]
    public InputActionReference action;

    private bool isRed = false;

    void Start()
    {
        lightComponent = GetComponent<Light>();

        if (lightComponent == null)
        {
            Debug.LogError("No Light component found on this GameObject!", this);
            return;
        }

        action.action.Enable();
        action.action.performed += (ctx) =>
        {
            if (isRed)
                lightComponent.color = Color.white;
            else
                lightComponent.color = new Color32(217, 16, 16, 255);

            isRed = !isRed;
        };
    }
}
