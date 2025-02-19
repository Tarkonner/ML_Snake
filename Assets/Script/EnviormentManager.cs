using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnviormentManager : MonoBehaviour
{
    [SerializeField] Vector2 enviormentSize = new Vector3(10, 10);
    public Vector2 enviormentRadius => enviormentSize / 2;

    [SerializeField] Vector2 centrumSpawnOffset = new Vector2(2, 2);

    [Header("Enviorment")]
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject ground;
    [SerializeField] float yOffset = -.2f;
    
    [Header("Agent")]
    [SerializeField] GameObject agentPrefab;
    [SerializeField] GameObject brainAgentPrefab;
    [SerializeField] GameObject gunPrefab;
    private List<GameObject> holdAgents = new List<GameObject>();

    [Header("Food")]
    [SerializeField] GameObject foodPrefab;
    private List<GameObject> holdFood = new List<GameObject>();
    [SerializeField] int numberOfFoodInEnviorment = 2;


    private GameObject holdTarget;
    private GameObject holdGun;

    private Renderer groundRenderer;

    // Action reset
    public Action ResetAction;

    private void Awake()
    {
        groundRenderer = ground.GetComponent<Renderer>();
        ResetAction += ResetEnviorment;
    }

    private void OnDisable()
    {
        ResetAction -= ResetEnviorment;
    }

    void Start()
    {
        //Enviorment
        ground.transform.localScale = new Vector3(enviormentSize.x + 1, 0, enviormentSize.y + 1);
        
        //Walls
        //+X
        GameObject wall = Instantiate(wallPrefab, transform);
        wall.transform.localPosition = new Vector3(enviormentSize.x / 2 + 1, yOffset, 0);
        wall.transform.localScale = new Vector3(1, 1, enviormentSize.y + 2);
        //-X
        wall = Instantiate(wallPrefab, transform);
        wall.transform.localPosition = new Vector3(-enviormentSize.x / 2 - 1, yOffset, 0);
        wall.transform.localScale = new Vector3(1, 1, enviormentSize.y + 2);
        //+Z
        wall = Instantiate(wallPrefab, transform);
        wall.transform.localPosition = new Vector3(0, yOffset, enviormentSize.y / 2 + 1);
        wall.transform.localScale = new Vector3(enviormentSize.x + 2, 1, 1);
        //-Z
        wall = Instantiate(wallPrefab, transform);
        wall.transform.localPosition = new Vector3(0, yOffset, -enviormentSize.y / 2 - 1);
        wall.transform.localScale = new Vector3(enviormentSize.x + 2, 1, 1);

        // Spawn agent once at startup.
        SpawnAgent();

        // Spawn food.
        for (int i = 0; i < numberOfFoodInEnviorment; i++)
        {
            GameObject food = Instantiate(foodPrefab, transform);
            holdFood.Add(food);
            food.transform.localPosition = GetFreeSpace();
        }
    }

    /// <summary>
    /// Instead of destroying the agent, reset its state.
    /// </summary>
    public void SpawnAgent()
    {
        if (holdAgents.Count == 0)
        {
            //Traning agent
            GameObject spawn = Instantiate(agentPrefab, transform);
            holdAgents.Add(spawn);
            spawn.transform.localPosition = new Vector3(
                Random.Range(-centrumSpawnOffset.x, centrumSpawnOffset.x),
                0,
                Random.Range(-centrumSpawnOffset.y, centrumSpawnOffset.y));


            // Setup gun if available.
            if (gunPrefab != null)
                AddGun(spawn);


            // Subscribe to death events so that a reset is triggered.
            if (spawn.GetComponentInChildren<SnakeMovement>() != null)
            {
                var snakeMove = spawn.GetComponentInChildren<SnakeMovement>();
                snakeMove.Dying += SpawnAgent;
                snakeMove.Dying += MoveAllFood;
            }

            //Brain agent
            spawn = Instantiate(brainAgentPrefab, transform);
            holdAgents.Add(spawn);
            spawn.transform.localPosition = new Vector3(
                Random.Range(-centrumSpawnOffset.x, centrumSpawnOffset.x),
                0,
                Random.Range(-centrumSpawnOffset.y, centrumSpawnOffset.y));
            // Setup gun if available.
            if (gunPrefab != null)
                AddGun(spawn);

        }
        else
        {
            for (int i = 0; i < holdAgents.Count; i++)
            {
                // Reset the existing agent's position.
                holdAgents[i].transform.localPosition = new Vector3(
                    Random.Range(-centrumSpawnOffset.x, centrumSpawnOffset.x),
                    0,
                    Random.Range(-centrumSpawnOffset.y, centrumSpawnOffset.y));

                // Call EndEpisode() on the agent so that it can reset its internal state.
                var snakeAgent = holdAgents[i].GetComponent<SnakeAgent>();
                if (snakeAgent != null)
                {
                    snakeAgent.EndEpisode();
                }
            }


        }
    }

    private void AddGun(GameObject snake)
    {
        SnakeMovement snakeMovement = snake.GetComponentInChildren<SnakeMovement>();
        if (snakeMovement != null)
        {
            Transform snakeHead = snakeMovement.transform;
            holdGun = Instantiate(gunPrefab, snakeHead.position, Quaternion.identity);

            GunPositionFollower follower = holdGun.GetComponent<GunPositionFollower>();
            if (follower != null)
            {
                follower.target = snakeHead;
                follower.offset = new Vector3(0, 0.5f, 0);
            }
        }
        else
        {
            Debug.LogWarning("SnakeMovement component not found on spawned agent. Gun not attached.");
        }
    }


    public void ResetEnviorment()
    {
        // Instead of destroying and re-instantiating, we just reset the agent.
        SpawnAgent();
        MoveAllFood();
    }

    public void MoveAllFood()
    {
        foreach (GameObject go in holdFood)
            go.transform.localPosition = GetFreeSpace();
    }

    public void MoveFood(GameObject targetFood)
    {
        Vector3 newPosition = GetFreeSpace();
        if (targetFood != null)
        {
            targetFood.transform.localPosition = newPosition;
            Food foodComponent = targetFood.GetComponent<Food>();
            if (foodComponent != null)
            {
                foodComponent.ResetFoodState();
            }
        }
        else
        {
            Debug.LogError("MoveFood() called with a null food object!");
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
            float hitboxRadius = .45f;
            bool hit = Physics.BoxCast(randomPos, new Vector3(hitboxRadius, hitboxRadius, hitboxRadius), Vector3.zero);
            if (!hit)
                return randomPos;

            maxStep--;
        }
        return Vector3.zero;
    }

    public void OnSuccess()
    {
        StartCoroutine(BlinkFloor(Color.green));
    }

    public void OnFailure()
    {
        StartCoroutine(BlinkFloor(Color.red));
    }

    private IEnumerator BlinkFloor(Color color)
    {
        if (groundRenderer == null)
            yield break;

        for (int i = 0; i < 3; i++)
        {
            groundRenderer.material.color = color;
            yield return new WaitForSeconds(0.2f);
            groundRenderer.material.color = Color.gray;
            yield return new WaitForSeconds(0.2f);
        }
        groundRenderer.material.color = Color.gray;
    }

    public void ChangeFloorColor(Color color)
    {
        if (groundRenderer != null)
        {
            groundRenderer.material.color = color;
        }
    }
}
