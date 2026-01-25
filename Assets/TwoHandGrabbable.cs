using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TwoHandGrabbable : MonoBehaviour
{
    public bool allowTwoHand = true;

    [Header("Extra Credit")]
    public bool doubleRotation = false;

    Rigidbody rb;

    Transform handA;
    Transform handB;

    Vector3 prevPosA;
    Quaternion prevRotA;

    Vector3 prevPosB;
    Quaternion prevRotB;

    Vector3 twoHandLocalOffset;

    bool wasKinematic;
    bool wasUseGravity;

    void Awake() => rb = GetComponent<Rigidbody>();

    public bool Grab(Transform hand)
    {
        if (handA == null)
        {
            handA = hand;
            prevPosA = handA.position;
            prevRotA = handA.rotation;
            BeginHoldPhysics();
            return true;
        }

        if (!allowTwoHand || handB != null || hand == handA) return false;

        handB = hand;
        prevPosB = handB.position;
        prevRotB = handB.rotation;

        Vector3 mid = (handA.position + handB.position) * 0.5f;
        twoHandLocalOffset = rb.position - mid;

        return true;
    }

    public void Release(Transform hand)
    {
        if (hand == handB)
        {
            handB = null;
            prevPosA = handA.position;
            prevRotA = handA.rotation;
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
                return;
            }

            handA = null;
            handB = null;
            EndHoldPhysics();
        }
    }

    void FixedUpdate()
    {
        if (handA == null) return;

        if (handB == null)
        {
            OneHandUpdate();
        }
        else
        {
            TwoHandUpdate();
        }
    }

    void OneHandUpdate()
    {
        Vector3 dp = handA.position - prevPosA;
        Quaternion dq = handA.rotation * Quaternion.Inverse(prevRotA);

        if (doubleRotation) dq = DoubleQuaternion(dq);

        Vector3 newPos = rb.position + dp;
        newPos = RotateAround(newPos, handA.position, dq);

        Quaternion newRot = dq * rb.rotation;

        rb.MovePosition(newPos);
        rb.MoveRotation(newRot);

        prevPosA = handA.position;
        prevRotA = handA.rotation;
    }

    void TwoHandUpdate()
    {
        Vector3 midPrev = (prevPosA + prevPosB) * 0.5f;
        Vector3 midNow  = (handA.position + handB.position) * 0.5f;

        Vector3 dpMid = midNow - midPrev;

        Vector3 vPrev = (prevPosB - prevPosA);
        Vector3 vNow  = (handB.position - handA.position);

        Quaternion dqHands = Quaternion.identity;
        if (vPrev.sqrMagnitude > 1e-6f && vNow.sqrMagnitude > 1e-6f)
            dqHands = Quaternion.FromToRotation(vPrev, vNow);

        if (doubleRotation) dqHands = DoubleQuaternion(dqHands);
        twoHandLocalOffset = dqHands * twoHandLocalOffset;

        Vector3 targetPos = midNow + twoHandLocalOffset + dpMid; 
        Quaternion targetRot = dqHands * rb.rotation;

        rb.MovePosition(targetPos);
        rb.MoveRotation(targetRot);

        prevPosA = handA.position;
        prevPosB = handB.position;

        prevRotA = handA.rotation;
        prevRotB = handB.rotation;
    }

    Vector3 RotateAround(Vector3 p, Vector3 pivot, Quaternion dq)
    {
        Vector3 r = p - pivot;
        r = dq * r;
        return pivot + r;
    }

    Quaternion DoubleQuaternion(Quaternion q)
    {
        q.ToAngleAxis(out float angle, out Vector3 axis);
        if (axis.sqrMagnitude < 1e-6f) return q;
        return Quaternion.AngleAxis(angle * 2f, axis.normalized);
    }

    void BeginHoldPhysics()
    {
        wasKinematic = rb.isKinematic;
        wasUseGravity = rb.useGravity;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void EndHoldPhysics()
    {
        rb.isKinematic = wasKinematic;
        rb.useGravity = wasUseGravity;
    }
}
