using UnityEngine;

public class Food : MonoBehaviour
{
    private EnviormentManager enviormentManager;
    private bool isEaten = false;  // Prevents multiple triggers

    public bool IsEaten => isEaten;

    private void Awake()
    {
        isEaten = false; // Reset flag when food is respawned
    }

    public void MarkAsEaten()
    {
        isEaten = true;
        Eaten();
        enviormentManager = GetComponentInParent<EnviormentManager>();
    }

    public void Eaten()
    {
        enviormentManager.MoveFood(gameObject);
    }
}