using UnityEngine;

public class ActiveBlockMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform waypointA;
    [SerializeField] private Transform waypointB;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float amplitude = 6f;
    [SerializeField] private bool moveOnX = true; // false => Z

    [Header("Anchoring")]
    [Tooltip("Centre de référence quand on change d'axe (ex: dernier StackBlock). Si null -> on utilise la position actuelle.")]
    [SerializeField] private Vector3 anchor;
    [SerializeField] private bool resetPositionOnAxisChange = true;

    private Vector3 startPos;
    private Vector3 endPos;
    private bool lastMoveOnX;
    private float phaseStartTime;

    private void Start()
    {
        lastMoveOnX = moveOnX;
        anchor = new Vector3(0, 0, 0);
        RecomputePath(resetPosition: true); // init
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

        Vector3 center = resetPosition
            ? (new Vector3(anchor.x, transform.position.y, anchor.z))
            : transform.position;

        Vector3 dir = moveOnX ? Vector3.right : Vector3.forward;
        startPos = center - dir * (amplitude * 0.5f);
        endPos = center + dir * (amplitude * 0.5f);

        if (resetPosition)
        {
            transform.position = center;
        }
    }
}
