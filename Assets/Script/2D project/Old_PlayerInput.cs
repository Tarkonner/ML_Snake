using UnityEngine;

public class Old_PlayerInput : MonoBehaviour
{
    private Old_SnakeMovement snakeMovement;

    void Start()
    {
        // Get reference to the Old_SnakeMovement component on the same GameObject.
        snakeMovement = GetComponent<Old_SnakeMovement>();
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
