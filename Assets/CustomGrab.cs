using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CustomGrab : MonoBehaviour
{
    [Header("Input (Button Action)")]
    public InputActionReference action; // مثلا XRI Left/Right Interaction / UI Press

    [Header("Filtering")]
    public bool requireGrabbableTag = true;   // اگر ذره‌بین Tag درست ندارد، این را false کن
    public string grabbableTag = "grabbable";

    [Header("Debug")]
    public bool debugLogs = false;

    // چیزهایی که داخل محدوده دست هستند (کامپوننت اصلی، نه Transform خام)
    private readonly List<TwoHandGrabbable> near = new();

    // چیزی که این دست نگه داشته
    private TwoHandGrabbable held;

    void Awake()
    {
        // Trigger لازم است
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // Rigidbody کینماتیک برای پایدار بودن trigger
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void OnEnable()
    {
        if (action != null) action.action.Enable();
    }

    void OnDisable()
    {
        if (action != null) action.action.Disable();
    }

    void Update()
    {
        if (action == null) return;

        if (action.action.WasPressedThisFrame())
            TryGrab();

        if (action.action.WasReleasedThisFrame())
            Release();
    }

    void TryGrab()
    {
        if (held != null) return;

        var target = GetClosest();
        if (target == null) return;

        // از خود Grabbable درخواست grab می‌کنیم
        if (target.Grab(transform))
        {
            held = target;
            if (debugLogs) Debug.Log($"[{name}] GRAB -> {target.name}");
        }
    }

    void Release()
    {
        if (held == null) return;

        held.Release(transform);
        if (debugLogs) Debug.Log($"[{name}] RELEASE");

        held = null;
    }

    TwoHandGrabbable GetClosest()
    {
        TwoHandGrabbable best = null;
        float bestD = float.MaxValue;

        for (int i = near.Count - 1; i >= 0; i--)
        {
            if (near[i] == null) { near.RemoveAt(i); continue; }

            float d = (near[i].transform.position - transform.position).sqrMagnitude;
            if (d < bestD) { bestD = d; best = near[i]; }
        }
        return best;
    }

    void OnTriggerEnter(Collider other)
    {
        // اگر می‌خوای فقط با Tag کار کنه:
        if (requireGrabbableTag && !other.CompareTag(grabbableTag)) return;

        // مهم: اگر Collider روی child باشد، از parent می‌گیریم
        var g = other.GetComponentInParent<TwoHandGrabbable>();
        if (g == null) return;

        if (!near.Contains(g))
        {
            near.Add(g);
            if (debugLogs) Debug.Log($"[{name}] ENTER -> {other.name} (grabbable={g.name}) near={near.Count}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (requireGrabbableTag && !other.CompareTag(grabbableTag)) return;

        var g = other.GetComponentInParent<TwoHandGrabbable>();
        if (g == null) return;

        if (near.Remove(g) && debugLogs)
            Debug.Log($"[{name}] EXIT -> {other.name} (grabbable={g.name}) near={near.Count}");
    }
}
