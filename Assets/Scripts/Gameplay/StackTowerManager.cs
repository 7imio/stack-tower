using System.Collections;
using UnityEngine;

public class StackTowerManager : MonoBehaviour
{
    [Header("Scene Refs")]
    [SerializeField] private Transform baseBlock;
    [SerializeField] private Transform initialActive;
    [SerializeField] private GameObject activeBlockPrefab;
    [SerializeField] private Transform stackRoot;

    [Header("Settings")]
    [SerializeField] private float blockHeight = 1f;
    [SerializeField] private bool startAxisX = true;
    [SerializeField] private float respawnDelay = .2f;

    private float spawnHeight;
    private bool currentAxisX;
    private Transform lastBlock;

    private void Start() 
    { 
    if (!baseBlock || !initialActive || !activeBlockPrefab)
        {
            Debug.LogError("[StackTowerManager] Missing References");
            enabled = false;
            return;
        }
        lastBlock = baseBlock;
        currentAxisX = startAxisX;

        // process initial height between upper base block and active center
        float baseTopY = baseBlock.position.y + baseBlock.localScale.y * .5f;
        spawnHeight = initialActive.position.y - baseTopY;

        // first active block configuration
        SetupActiveBlock(initialActive.gameObject, currentAxisX);

        // listen landing
        var dropper = initialActive.GetComponent<DropOnTop>();
        dropper.StartCoroutine(WatchLanding(dropper));
    }
    private IEnumerator WatchLanding(DropOnTop dropper)
    {
        // wait until block completely gets down
        while (dropper != null && dropper.enabled && dropper.gameObject.transform.position.y > lastBlock.position.y + blockHeight * 1.1f)
            yield return null;

        yield return new WaitForSeconds(respawnDelay);

        // while dropped -> set to stackroot
        dropper.gameObject.transform.SetParent(stackRoot, true);
        dropper.enabled = false;

        // last block ref
        lastBlock = dropper.transform;

        // switch axis
        currentAxisX = !currentAxisX;

        SpawnNextActive();
    }
    private void SpawnNextActive() 
    {
        Vector3 lastPos = lastBlock.position;
        float lastTopY = lastPos.y + lastBlock.localScale.y * 0.5f;
        float newCenterY = lastTopY + spawnHeight;

        Vector3 spawnPos = new Vector3(
            lastPos.x,
            newCenterY,
            lastPos.z
        );

        var go = Instantiate(activeBlockPrefab, spawnPos, Quaternion.identity);
        go.transform.localScale = lastBlock.localScale;
        go.tag = "ActiveBlock";
        go.layer = LayerMask.NameToLayer("Blocks");

        SetupActiveBlock(go, currentAxisX);

        var dropper = go.GetComponent<DropOnTop>();
        dropper.StartCoroutine(WatchLanding(dropper));

    }
    private void SetupActiveBlock(GameObject active, bool axisX) {
        var mover = active.GetComponent<ActiveBlockMover>();
        if (mover)
        {
            mover.SetAxis(axisX, resetPosition: true);
            mover.SetAmplitude(Mathf.Max(active.transform.localScale.x, active.transform.localScale.z) * 2f);
        }
    }
}
