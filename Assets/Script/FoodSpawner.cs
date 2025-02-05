using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public static FoodSpawner Instance;

    int maxLoops = 100000;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        //Break if error
        int ticks = 0;

        GameObject go = PoolManager.instance.GetObject("Food");

        bool legalPlacement = false;
        while (!legalPlacement)
        {
            Vector2Int placement = new Vector2Int(
                Random.Range(-Grid.verticalGirdSize, Grid.verticalGirdSize),
                Random.Range(-Grid.verticalGirdSize, Grid.verticalGirdSize));

            Vector2 boxSize = new Vector2(0.1f, 0.1f); // Small square area
            Collider2D hit = Physics2D.OverlapBox(placement, boxSize, 0f);

            if (hit == null)
            {
                legalPlacement = true;

                go.transform.position = new Vector3(placement.x, placement.y, 0);
            }

            ticks++;

            if(ticks > maxLoops)
            {
                Debug.LogError("no place to spawn");
                break;
            }    
        }

        go.SetActive(true);
    }
}
