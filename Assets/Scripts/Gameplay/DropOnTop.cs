using System.Collections;
using UnityEngine;

public class DropOnTop : MonoBehaviour
{
    [SerializeField] private float fallDuration = .5f;
    [SerializeField] private float landingEpsilon = .0005f;

    [Tooltip("Layers for landing => 'Blocks'")]
    [SerializeField] private LayerMask landingMask;

    private Collider selfCollider;
    private ActiveBlockMover mover;
    private bool isDropping;
    private bool isSubscribed;

    private void Awake()
    {
        selfCollider = GetComponent<Collider>();
        mover = GetComponent<ActiveBlockMover>();
    }
    private void OnEnable()
    {
        TrySubscribe();
        if (!isSubscribed) StartCoroutine(SubscribeWhenReady());
    }
    private void OnDisable()
    {
        if (isSubscribed && CoreInput.Instance != null)
        {
            CoreInput.Instance.OnDropPressed -= HandleDrop;
            isSubscribed = false;
        }
    }
    private void TrySubscribe() 
    {
        if (!isSubscribed && CoreInput.Instance != null)
        {
            CoreInput.Instance.OnDropPressed += HandleDrop;
            isSubscribed = true;
        }
    }
    private IEnumerator SubscribeWhenReady() 
    {
        while (CoreInput.Instance == null) yield return null;
        TrySubscribe();
    }
    private void HandleDrop() 
    {
        if (isDropping) return;
        StartCoroutine(FallRoutine());
    }
    private IEnumerator FallRoutine() 
    {
        isDropping = true;
        if (mover) mover.enabled = false;

        Vector3 startPos = transform.position;
        float halfHeight = transform.localScale.y * .5f;

        // boxcast
        bool colWasEnabled = selfCollider.enabled;
        if (colWasEnabled) selfCollider.enabled = false;

        const float shrink = 0.001f;
        Vector3 halfExtents = new Vector3(
            Mathf.Max(.0001f, transform.localScale.x * .5f - shrink),
            .05f,
            Mathf.Max(.0001f, transform.localScale.z * .5f - shrink)
        );

        Vector3 castOrigin = startPos + Vector3.up * 10f;
        float castDist = 100f;
        var hits = Physics.BoxCastAll(
            castOrigin, halfExtents, Vector3.down, transform.rotation, castDist,
            landingMask, QueryTriggerInteraction.Ignore
        );

        if (colWasEnabled) selfCollider.enabled = true;

        // if nothing hits, don't move
        if ( hits == null || hits.Length == 0)
        {
            if (mover) mover.enabled = true;
            isDropping = false;
            yield break;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        var hit = hits[0];

        float targetCenterY = hit.point.y + halfHeight + landingEpsilon;
        Vector3 endPos = new Vector3(startPos.x, targetCenterY, startPos.z);

        //vertical lerp
        float t = 0f;
        float dur = Mathf.Max(0.0001f, fallDuration);

        while ( t < 1f )
        {
            t += Time.deltaTime / dur;
            float eased = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }
        transform.position = endPos;

        // stay dropped
        isDropping = false;
    }
}
