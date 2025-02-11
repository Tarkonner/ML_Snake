using Unity.MLAgents;
using UnityEngine;

public class EnviormentManager : MonoBehaviour
{
    [SerializeField] Vector2 enviormentSize = new Vector3(10, 10);
    public Vector2 enviormentRadius => enviormentSize / 2;

    [SerializeField] GameObject ground;
    [SerializeField] GameObject agentPrefab;
    [SerializeField] GameObject foodPrefab;

    private GameObject holdFood;

    void Start()
    {
        ground.transform.localScale = new Vector3(enviormentSize.x  + 1, 0, enviormentSize.y + 1);

        //Agent
        GameObject spawn = Instantiate(agentPrefab, transform);
        spawn.transform.localPosition = GetFreeSpace();
        if(spawn.TryGetComponent(out SnakeAgent sa))
        {
            sa.CallEnding += MoveFood;
        }

        //Food
        holdFood = Instantiate(foodPrefab, transform);
        holdFood.transform.localPosition = GetFreeSpace();
        holdFood.GetComponent<Food>().Setup(this);
    }

    public void MoveFood()
    {
        holdFood.transform.localPosition = GetFreeSpace();
    }

    public Vector3 GetFreeSpace()
    {
        int maxStep = 1000;

        while(maxStep > 0)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-enviormentRadius.x, enviormentRadius.x),
                0,
                Random.Range(-enviormentRadius.y, enviormentRadius.y));

            bool hit = Physics.BoxCast(randomPos, new Vector3(.1f, .1f, .1f), Vector3.zero);
            if(!hit)
                return randomPos;

            maxStep--;
        }

        return Vector3.zero;
    }
}
