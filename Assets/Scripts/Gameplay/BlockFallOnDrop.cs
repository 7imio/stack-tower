using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(ActiveBlockMover))]
[RequireComponent(typeof(Collider))]
public class BlockFallOnDrop : MonoBehaviour
{
    [Header("Fall")]
    [SerializeField] private float fallDuration = 0.5f;
    [Tooltip("Layers considered as landing surfaces (e.g., Ground, Blocks).")]
    [SerializeField] private LayerMask dropSurfaceMask;
    [Tooltip("Extra offset added to avoid z-fighting.")]
    [SerializeField] private float landingEpsilon = 0.0005f;

    private ActiveBlockMover mover;
    private Collider selfCol;
    private bool isDropping;
    private bool hasLanded;
    private bool isSubscribed;

    private void Awake()
    {
        mover = GetComponent<ActiveBlockMover>();
        selfCol = GetComponent<Collider>();
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
            CoreInput.Instance.OnDropPressed -= HandleDropPressed;
            isSubscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (!isSubscribed && CoreInput.Instance != null)
        {
            CoreInput.Instance.OnDropPressed += HandleDropPressed;
            isSubscribed = true;
        }
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (CoreInput.Instance == null) yield return null;
        TrySubscribe();
    }

    private void HandleDropPressed()
    {
        if (isDropping || hasLanded) return;
        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        isDropping = true;

        if (mover) mover.enabled = false;

        Vector3 startPos = transform.position;
        float halfHeight = transform.localScale.y * 0.5f;



        // --- NEW: BoxCastAll pour couvrir toute l'emprise XZ du bloc ---
        var col = GetComponent<Collider>();
        bool hadCollider = col && col.enabled;
        if (hadCollider) col.enabled = false; // éviter de se toucher soi-même

        // demi-extents de la box: un chouïa plus petit pour éviter les coplanarités
        float shrink = 0.001f;
        Vector3 halfExtents = new Vector3(
            Mathf.Max(0.0001f, transform.localScale.x * 0.5f - shrink),
            0.05f, // petite "épaisseur" suffisante pour intersecter
            Mathf.Max(0.0001f, transform.localScale.z * 0.5f - shrink)
        );

        // on démarre au-dessus et on cast vers le bas
        Vector3 castOrigin = startPos + Vector3.up * 10f;
        float castDistance = 100f;

        RaycastHit[] hits = Physics.BoxCastAll(
            castOrigin,
            halfExtents,
            Vector3.down,
            transform.rotation,
            castDistance,
            dropSurfaceMask,
            QueryTriggerInteraction.Ignore
        );

        // on peut réactiver le collider maintenant
        if (hadCollider) col.enabled = true;

        float targetCenterY;

        // tri par distance et on ignore tout hit "self" par sécurité
        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            RaycastHit? firstValid = null;
            foreach (var h in hits)
            {
                if (h.collider == col) continue;                  // ignore self
                                                                  // si tu veux être encore plus strict: ignore tout hit dont le point est dans tes bounds
                                                                  // if (col.bounds.Contains(h.point)) continue;
                firstValid = h;
                break;
            }

            if (firstValid.HasValue)
            {
                var hit = firstValid.Value;
                targetCenterY = hit.point.y + halfHeight + landingEpsilon;
                // Debug.Log($"[BlockFallOnDrop] BoxCast landing on '{hit.collider.name}' y={targetCenterY:0.###}");
            }
            else
            {
                // aucun hit valable (ne devrait pas arriver souvent) -> fallback sol à 0
                targetCenterY = halfHeight + landingEpsilon;
            }
        }
        else
        {
            // rien sous la box -> fallback sol à 0
            targetCenterY = halfHeight + landingEpsilon;
        }

        // suite identique: interpolation verticale vers endPos
        Vector3 endPos = new Vector3(startPos.x, targetCenterY, startPos.z);
        float t = 0f, dur = Mathf.Max(0.0001f, fallDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float eased = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        transform.position = endPos;

        hasLanded = true;
        isDropping = false;

        if (isSubscribed && CoreInput.Instance != null)
        {
            CoreInput.Instance.OnDropPressed -= HandleDropPressed;
            isSubscribed = false;
        }

        Debug.Log("[BlockFallOnDrop] Landed.");

        gameObject.tag = "StackBlock";
    }
}
