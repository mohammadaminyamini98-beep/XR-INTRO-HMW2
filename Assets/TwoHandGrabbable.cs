using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TwoHandGrabbable : MonoBehaviour
{
    [Header("Settings")]
    public bool allowTwoHand = true;
    public bool doubleRotation = false; // Extra Credit

    private Rigidbody rb;

    // Hands
    private Transform handA;
    private Transform handB;

    // History
    private Vector3 prevPosA;
    private Quaternion prevRotA;
    private Vector3 prevPosB;
    private Quaternion prevRotB;

    // Physics Backup
    private bool wasKinematic;
    private bool wasUseGravity;

    void Awake() => rb = GetComponent<Rigidbody>();

    public bool Grab(Transform hand)
    {
        // 1. First Hand
        if (handA == null)
        {
            handA = hand;
            prevPosA = hand.position;
            prevRotA = hand.rotation;
            BeginHoldPhysics();
            return true;
        }

        // 2. Second Hand
        if (allowTwoHand && handB == null && hand != handA)
        {
            handB = hand;
            prevPosB = hand.position;
            prevRotB = hand.rotation;
            return true;
        }

        return false;
    }

    public void Release(Transform hand)
    {
        if (hand == handB)
        {
            handB = null;
            return;
        }

        if (hand == handA)
        {
            if (handB != null)
            {
                handA = handB;
                prevPosA = prevPosB;
                prevRotA = prevRotB;
                handB = null;
            }
            else
            {
                handA = null;
                EndHoldPhysics();
            }
        }
    }

    void FixedUpdate()
    {
        if (handA == null) return;

        Vector3 posChange = Vector3.zero;
        Quaternion rotChange = Quaternion.identity;

        // --- ONE HAND MODE ---
        if (handB == null)
        {
            GetHandDelta(handA, ref prevPosA, ref prevRotA, out Vector3 dPos, out Quaternion dRot);
            posChange = dPos;
            rotChange = dRot;
        }
        // --- TWO HAND MODE ---
        else
        {
            // 1. Calculate STEERING (Handlebar) Rotation FIRST
            // We must do this BEFORE GetHandDelta updates the 'prevPos' variables.
            Vector3 prevDir = prevPosB - prevPosA;
            Vector3 currDir = handB.position - handA.position;
            
            Quaternion steerRot = Quaternion.identity;
            if (prevDir.sqrMagnitude > 0.001f && currDir.sqrMagnitude > 0.001f)
            {
                steerRot = Quaternion.FromToRotation(prevDir.normalized, currDir.normalized);
            }

            // 2. Calculate Individual Hand Movements (Wrists & Position)
            GetHandDelta(handA, ref prevPosA, ref prevRotA, out Vector3 dPosA, out Quaternion dRotA);
            GetHandDelta(handB, ref prevPosB, ref prevRotB, out Vector3 dPosB, out Quaternion dRotB);

            // 3. Combine Everything
            
            // Average the Position (Fixes the "Go Further" bug)
            posChange = (dPosA + dPosB) * 0.5f;

            // Combine Rotation: Steering * WristA * WristB
            rotChange = steerRot * (dRotA * dRotB);
        }

        // --- EXTRA CREDIT: DOUBLE ROTATION ---
        if (doubleRotation)
        {
            rotChange.ToAngleAxis(out float angle, out Vector3 axis);
            // Safety check to prevent NaN errors
            if (!float.IsInfinity(axis.x) && axis.sqrMagnitude > 0.001f)
                rotChange = Quaternion.AngleAxis(angle * 2.0f, axis);
        }

        // --- APPLY PHYSICS ---
        rb.MovePosition(rb.position + posChange);
        rb.MoveRotation(rotChange * rb.rotation);
    }

    // Helper: Calculates Wrist Rotation and "Pivot" Arc Movement
    void GetHandDelta(Transform hand, ref Vector3 prevPos, ref Quaternion prevRot, out Vector3 dPos, out Quaternion dRot)
    {
        // Basic movement
        dPos = hand.position - prevPos;
        dRot = hand.rotation * Quaternion.Inverse(prevRot);

        // Pivot Logic: Object orbits the hand (like Moon around Earth)
        Vector3 relative = rb.position - hand.position;
        Vector3 rotatedRelative = dRot * relative;
        Vector3 pivotMove = rotatedRelative - relative;

        dPos += pivotMove;

        // Update history
        prevPos = hand.position;
        prevRot = hand.rotation;
    }

    void BeginHoldPhysics()
    {
        wasKinematic = rb.isKinematic;
        wasUseGravity = rb.useGravity;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void EndHoldPhysics()
    {
        rb.isKinematic = wasKinematic;
        rb.useGravity = wasUseGravity;
    }
}