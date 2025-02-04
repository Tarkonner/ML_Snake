using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private SnakeMovement snakeMovement;

    void Start()
    {
        // Get reference to the SnakeMovement component on the same GameObject.
        snakeMovement = GetComponent<SnakeMovement>();
    }

    void Update()
    {
        // Check for movement inputs and call corresponding functions.
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            snakeMovement.MoveUp();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            snakeMovement.MoveDown();
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            snakeMovement.MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            snakeMovement.MoveRight();
        }
    }
}
