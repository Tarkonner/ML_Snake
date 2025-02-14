using UnityEngine;

public class Food : MonoBehaviour
{
    private EnviormentManager enviormentManager;
    private bool isEaten = false;  // Prevents multiple triggers

    public bool IsEaten => isEaten;

    public void Setup(EnviormentManager enviorment)
    {
        enviormentManager = enviorment;
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
}