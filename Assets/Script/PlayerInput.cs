using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private SnakeMovement snakeMovement;


    void Start()
    {
        snakeMovement = GetComponent<SnakeMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical"));

        snakeMovement.SetMoveDirection(direction);
    }
}
