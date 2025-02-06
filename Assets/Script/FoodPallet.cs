using Unity.VisualScripting;
using UnityEngine;

public class FoodPallet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Move with game tick
        SnakeGameManager.instance.GameOver += RemoveFood;  
    }
    
    void OnDisable()
    {
        SnakeGameManager.instance.GameOver -= RemoveFood;
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
