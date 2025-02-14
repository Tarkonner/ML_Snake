using UnityEngine;

public class Food : MonoBehaviour
{
    private EnviormentManager enviormentManager;
    private bool isEaten = false;  // Prevents multiple triggers

    public bool IsEaten => isEaten;

    private void Awake()
    {
        enviormentManager = GetComponentInParent<EnviormentManager>();
        isEaten = false; // Reset flag when food is respawned
    }

    public void MarkAsEaten()
    {
        isEaten = true;
        Eaten();
    }

    public void Eaten()
    {
        enviormentManager.MoveFood(gameObject);
    }
    
    public void ResetFoodState()
    {
        isEaten = false; // Allow it to be eaten again
    }

}