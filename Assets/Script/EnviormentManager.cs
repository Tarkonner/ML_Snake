using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnviormentManager : MonoBehaviour
{
    [SerializeField] Vector2 enviormentSize = new Vector3(10, 10);
    public Vector2 enviormentRadius => enviormentSize / 2;

    [SerializeField] GameObject ground;
    [SerializeField] GameObject agentPrefab;
    [SerializeField] GameObject foodPrefab;
    [SerializeField] GameObject targetPrefab;

    private GameObject holdFood;
    private GameObject holdAgent;
    private GameObject holdTarget;
    
    private Renderer groundRenderer;

    private void Awake()
    {
        groundRenderer = ground.GetComponent<Renderer>(); 
    }

    void Start()
    {
        ground.transform.localScale = new Vector3(enviormentSize.x + 1, 0, enviormentSize.y + 1);

        //Agent
        holdAgent = Instantiate(agentPrefab, transform);
        holdAgent.transform.localPosition = GetFreeSpace();
        
        SnakeMovement snakeMovement = holdAgent.GetComponentInChildren<SnakeMovement>();
        if (snakeMovement != null)
        {
            snakeMovement.Dying += MoveAgent;
            snakeMovement.OnReachedTargetSize += SpawnTarget; // Subscribe to event
        }

        if (holdAgent.GetComponentInChildren<SnakeAgent>())
        {
            holdAgent.GetComponentInChildren<SnakeAgent>().CallEnding += MoveFood;
        }

        //Food
        holdFood = Instantiate(foodPrefab, transform);
        holdFood.transform.localPosition = GetFreeSpace();
        holdFood.GetComponent<Food>().Setup(this);
    }
    
    void SpawnTarget()
    {
        Debug.Log("Spawning Special Target");

        if (holdTarget == null) // If target doesn't exist, create it
        {
            holdTarget = Instantiate(targetPrefab, transform);
        }

        holdTarget.transform.localPosition = GetFreeSpace();
    }

    public void MoveFood()
    {
        holdFood.transform.localPosition = GetFreeSpace();
    }
    
    public void MoveAgent()
    {
        holdAgent.transform.localPosition = GetFreeSpace();
        MoveFood();
        holdAgent.GetComponentInChildren<SnakeMovement>().StartGame();

        // Clear the target on reset
        if (holdTarget != null)
        {
            Debug.Log("Clearing target on reset...");
            Destroy(holdTarget);
            holdTarget = null;
        }
    }

    public Vector3 GetFreeSpace()
    {
        int maxStep = 1000;

        while (maxStep > 0)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-enviormentRadius.x, enviormentRadius.x),
                0,
                Random.Range(-enviormentRadius.y, enviormentRadius.y));

            bool hit = Physics.BoxCast(randomPos, new Vector3(.1f, .1f, .1f), Vector3.zero);
            if (!hit)
                return randomPos;

            maxStep--;
        }

        return Vector3.zero;
    }
    
    public void OnSuccess()
    {
        StartCoroutine(BlinkFloor(Color.green)); // Blink green on success
    }

    public void OnFailure()
    {
        StartCoroutine(BlinkFloor(Color.red)); // Blink red on failure
    }

    private IEnumerator BlinkFloor(Color color)
    {
        if (groundRenderer == null)
            yield break;

        for (int i = 0; i < 3; i++) // Blink 3 times
        {
            groundRenderer.material.color = color;
            yield return new WaitForSeconds(0.2f); // Wait 0.3 sec
            groundRenderer.material.color = Color.gray; // Reset to default
            yield return new WaitForSeconds(0.2f);
        }
        
        groundRenderer.material.color = Color.gray; // Reset to default
    }


    private void ChangeFloorColor(Color color)
    {
        if (groundRenderer != null)
        {
            groundRenderer.material.color = color;
        }
    }
}
