using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] int numberOfFoodInEnviorment = 2;


    private List<GameObject> holdFood = new List<GameObject>();
    private GameObject holdAgent;

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
        holdAgent = Instantiate(agentPrefab, transform);
        holdAgent.transform.localPosition = new Vector3(
                Random.Range(-centrumSpawnOffset.x, centrumSpawnOffset.x),
                0,
                Random.Range(-centrumSpawnOffset.y, centrumSpawnOffset.y));

        if (holdAgent.GetComponentInChildren<SnakeMovement>())
        {
            holdAgent.GetComponentInChildren<SnakeMovement>().Dying += MoveAgent;
        }

        if (holdAgent.GetComponentInChildren<SnakeAgent>())
        {
            holdAgent.GetComponentInChildren<SnakeAgent>().CallEnding += MoveAllFood;
        }

        //Food
        for (int i = 0; i < numberOfFoodInEnviorment; i++)
        {
            GameObject food = Instantiate(foodPrefab, transform);
            holdFood.Add(food);
            food.transform.localPosition = GetFreeSpace();
            food.GetComponent<Food>().Setup(this);
        }


    }

    public void MoveAllFood()
    {
        foreach (GameObject go in holdFood)
            go.transform.localPosition = GetFreeSpace();
    }

    public void MoveFood(GameObject targetFood) => targetFood.transform.localPosition = GetFreeSpace();


    public void MoveAgent()
    {
        //Spawn in centrum
        holdAgent.transform.localPosition = new Vector3(
                Random.Range(-centrumSpawnOffset.x, centrumSpawnOffset.x),
                0,
                Random.Range(-centrumSpawnOffset.y, centrumSpawnOffset.y));

        MoveAllFood();
        SnakeMovement sm = holdAgent.GetComponentInChildren<SnakeMovement>();
        sm.StartGame();
        sm.SetMoveDirection(Vector3.forward);
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
}
