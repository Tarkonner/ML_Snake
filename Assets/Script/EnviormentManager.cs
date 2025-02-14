
using System.Collections.Generic;
using System;
using System.Collections;
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
    [Header("Food")]
    [SerializeField] GameObject foodPrefab;
    private List<GameObject> holdFood = new List<GameObject>();
    [SerializeField] int numberOfFoodInEnviorment = 2;
    [SerializeField] GameObject targetPrefab;

    private GameObject holdAgent;
    private GameObject holdTarget;
    
    private Renderer groundRenderer;

    //Action reset
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

        //Agent
        SpawnAgent();
        //Food
        for (int i = 0; i < numberOfFoodInEnviorment; i++)
        {
            GameObject food = Instantiate(foodPrefab, transform);
            holdFood.Add(food);
            food.transform.localPosition = GetFreeSpace();
        }
    }
    
    public void ResetEnviorment()
    {
        SpawnAgent();
        MoveAllFood();
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

    public void MoveAllFood()
    {
        foreach (GameObject go in holdFood)
            go.transform.localPosition = GetFreeSpace();
    }

    public void MoveFood(GameObject targetFood) => targetFood.transform.localPosition = GetFreeSpace();


    public void SpawnAgent()
    {
        //Agent
        if (holdAgent != null)
            Destroy(holdAgent);

        holdAgent = Instantiate(agentPrefab, transform);
        holdAgent.transform.localPosition = new Vector3(
                Random.Range(-centrumSpawnOffset.x, centrumSpawnOffset.x),
                0,
                Random.Range(-centrumSpawnOffset.y, centrumSpawnOffset.y));
    }
    
    //public void MoveAgent()
    //{
    //    holdAgent.transform.localPosition = GetFreeSpace();
    //    MoveAllFood();

    //    // Clear the target on reset
    //    if (holdTarget != null)
    //    {
    //        Debug.Log("Clearing target on reset...");
    //        Destroy(holdTarget);
    //        holdTarget = null;
    //    }
    //}

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


    public void ChangeFloorColor(Color color)
    {
        if (groundRenderer != null)
        {
            groundRenderer.material.color = color;
        }
    }
}
