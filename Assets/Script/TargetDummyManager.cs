using UnityEngine;

public class TargetDummyManager : MonoBehaviour
{
    [Tooltip("Prefab for the target dummy.")]
    [SerializeField] private GameObject targetDummyPrefab;

    private GameObject currentTargetDummy;

    [Header("Environment Settings")]
    [SerializeField] private Vector2 environmentSize = new Vector2(10, 10);
    public Vector2 environmentRadius => environmentSize / 2;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private int maxStep = 1000;

    private void Start()
    {
        // Spawn the target dummy automatically at startup.
        //ResetTargetDummy();
    }

    /// <summary>
    /// Spawns or repositions the target dummy.
    /// Called on startup, when the dummy is hit, or on episode reset.
    /// </summary>
    public void ResetTargetDummy()
    {
        if (currentTargetDummy == null)
        {
            currentTargetDummy = Instantiate(targetDummyPrefab);
            // Ensure the dummy is tagged as "Enemy" (if needed)
            currentTargetDummy.tag = "Enemy";

            TargetDummy dummyScript = currentTargetDummy.GetComponent<TargetDummy>();
            if (dummyScript != null)
            {
                dummyScript.manager = this;
            }
        }
        // Reposition the existing dummy.
        currentTargetDummy.transform.position = GetFreeSpace();
    }

    /// <summary>
    /// Finds a free position within the environment.
    /// </summary>
    private Vector3 GetFreeSpace()
    {
        int steps = maxStep;
        while (steps > 0)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-environmentRadius.x, environmentRadius.x),
                yOffset,
                Random.Range(-environmentRadius.y, environmentRadius.y)
            );
            float hitboxRadius = 0.45f;
            bool hit = Physics.BoxCast(randomPos, new Vector3(hitboxRadius, hitboxRadius, hitboxRadius), Vector3.zero);
            if (!hit)
                return randomPos;
            steps--;
        }
        return Vector3.zero;
    }
}

