using UnityEngine;

public class ActiveBlockMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform waypointA;
    [SerializeField] private Transform waypointB;
    [SerializeField] private float speed = 1f;

    [SerializeField] private float amplitude = 10f;
    [SerializeField] private bool moveOnX = true;

    [Header("Anchoring")]
    [Tooltip("Centre XZ de référence (si non défini, on prend la position courante au 1er recalcul).")]
    [SerializeField] private Vector3 anchor;
    [SerializeField] private bool resetPositionOnAxisChange = true;

    private Vector3 startPos, endPos;
    private bool lastMoveOnX;
    private float phaseStartTime;
    private bool anchorInitialized = false;

    private void Start()
    {
        lastMoveOnX = moveOnX;
        RecomputePath(resetPosition: true);
        phaseStartTime = Time.time;
    }

    private void Update()
    {
        if (moveOnX != lastMoveOnX)
        {
            lastMoveOnX = moveOnX;
            RecomputePath(resetPosition: resetPositionOnAxisChange);
            phaseStartTime = Time.time;
        }

        float t = Mathf.PingPong((Time.time - phaseStartTime) * speed, 1f);
        transform.position = Vector3.Lerp(startPos, endPos, t);
    }

    private void EnsureAnchorInitialized()
    {
        if (anchorInitialized) return;
        anchor = new Vector3(transform.position.x, 0f, transform.position.z);
        anchorInitialized = true;
    }

    private void RecomputePath(bool resetPosition)
    {
        if (waypointA && waypointB)
        {
            if (resetPosition)
            {
                Vector3 mid = (waypointA.position + waypointB.position) * 0.5f;
                transform.position = new Vector3(mid.x, transform.position.y, mid.z);
            }
            startPos = waypointA.position;
            endPos = waypointB.position;
            startPos.y = endPos.y = transform.position.y;
            return;
        }

        EnsureAnchorInitialized();

        Vector3 center = resetPosition
            ? new Vector3(anchor.x, transform.position.y, anchor.z)
            : transform.position;

        Vector3 dir = moveOnX ? Vector3.right : Vector3.forward;
        startPos = center - dir * (amplitude * 0.5f);
        endPos = center + dir * (amplitude * 0.5f);

        if (resetPosition)
            transform.position = center;
    }

    public void SetAxis(bool onX, bool resetPosition = true)
    {
        if (moveOnX == onX && !resetPosition) return;
        moveOnX = onX;
        lastMoveOnX = !onX; 
        RecomputePath(resetPosition);
        phaseStartTime = Time.time;
    }

    public void SetAmplitude(float value, bool resetPosition = false)
    {
        amplitude = Mathf.Max(0.0001f, value);
        RecomputePath(resetPosition);
    }

    public void SetAnchorXZ(Vector3 centerXZ, bool resetPosition = true)
    {
        anchor = new Vector3(centerXZ.x, 0f, centerXZ.z);
        anchorInitialized = true;
        RecomputePath(resetPosition);
        phaseStartTime = Time.time;
    }
}
