using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CustomGrab : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference action; // Assign XRI Left/Right Interaction/Select here

    [Header("Settings")]
    public string grabbableTag = "grabbable";

    // Track objects currently inside the trigger
    private List<TwoHandGrabbable> nearObjects = new List<TwoHandGrabbable>();
    
    // Track what we are currently holding
    private TwoHandGrabbable currentHeldObject;

    void Awake()
    {
        // Ensure required components are set up correctly
        GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void OnEnable() => action.action.Enable();
    void OnDisable() => action.action.Disable();

    void Update()
    {
        // 1. Detect Grab Press
        if (action.action.WasPressedThisFrame())
        {
            TryGrab();
        }

        // 2. Detect Release
        if (action.action.WasReleasedThisFrame())
        {
            Release();
        }
    }

    void TryGrab()
    {
        // Don't grab if already holding something
        if (currentHeldObject != null) return;

        // Find the closest valid object
        TwoHandGrabbable target = GetClosest();
        if (target != null)
        {
            // Try to grab it. If successful, save it.
            if (target.Grab(transform))
            {
                currentHeldObject = target;
            }
        }
    }

    void Release()
    {
        if (currentHeldObject != null)
        {
            currentHeldObject.Release(transform);
            currentHeldObject = null;
        }
    }

    // Helper to find closest object in the trigger list
    TwoHandGrabbable GetClosest()
    {
        TwoHandGrabbable closest = null;
        float minDistance = float.MaxValue;

        // Iterate backwards to safely remove nulls if objects were destroyed
        for (int i = nearObjects.Count - 1; i >= 0; i--)
        {
            if (nearObjects[i] == null)
            {
                nearObjects.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(transform.position, nearObjects[i].transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = nearObjects[i];
            }
        }
        return closest;
    }

    // Trigger Logic to build the list of nearby objects
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(grabbableTag))
        {
            var script = other.GetComponentInParent<TwoHandGrabbable>();
            if (script != null && !nearObjects.Contains(script))
            {
                nearObjects.Add(script);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(grabbableTag))
        {
            var script = other.GetComponentInParent<TwoHandGrabbable>();
            if (script != null)
            {
                nearObjects.Remove(script);
            }
        }
    }
}