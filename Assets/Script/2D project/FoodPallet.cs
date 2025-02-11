using Unity.VisualScripting;
using UnityEngine;

public class FoodPallet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Move with game tick
        Old_SnakeGameManager.instance.GameOver += RemoveFood;  
    }
    
    void OnDisable()
    {
        Old_SnakeGameManager.instance.GameOver -= RemoveFood;
    }
    
    void RemoveFood()
    {
        Destroy(gameObject);
    }

    public void EatMe()
    {
        FoodSpawner.Instance.SpawnFood();
        Destroy(gameObject);
    }
}
