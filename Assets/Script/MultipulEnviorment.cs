using UnityEngine;

public class MultipulEnviorment : MonoBehaviour
{
    [SerializeField] GameObject enviormentPrefab;
    [SerializeField] Vector2 numberToSpawn = new Vector2(3, 5);
    [SerializeField] Vector2 spaceBetweenEnviorment = new Vector2(50, 50);


    private void Start()
    {
        for (int x = 0; x < numberToSpawn.x; x++)
        {
            for (int y = 0; y < numberToSpawn.y; y++)
            {
                Vector3 targetPos = new Vector3(x * spaceBetweenEnviorment.x, 0, y * spaceBetweenEnviorment.y);
                GameObject spawn = Instantiate(enviormentPrefab, transform);
                spawn.transform.localPosition = targetPos;
            }
        }
    }
}
