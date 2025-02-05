using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class SnakeAgent : Agent
{
    [SerializeField] private SnakeMovement snake;

    private void Reset()
    {
        SnakeGameManager.instance.StartGame();
    }

    public override void Initialize()
    {
        Reset();
        Debug.Log("ini");
    }
    
    public override void OnEpisodeBegin()
    {
        Reset();
        Debug.Log("epid");
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // Add agent's current direction
        Vector2 direction = snake.movementDirection;
        sensor.AddObservation(direction.x);
        sensor.AddObservation(direction.y);

        // Add agent's position (normalized)
        Vector2 snakePosition = snake.transform.position;
        sensor.AddObservation(snakePosition.x / (Grid.horzontalGridSize*2 +1));
        sensor.AddObservation(snakePosition.y / (Grid.verticalGirdSize*2 +1));

        // // Add the distance to the nearest food (normalized)
        // Vector2 foodPosition = GetClosestFoodPosition(snakePosition);
        // Vector2 foodDirection = (foodPosition - snakePosition).normalized;
        // sensor.AddObservation(foodDirection.x);
        // sensor.AddObservation(foodDirection.y);
    }

    private Vector2 GetClosestFoodPosition(Vector2 snakePosition)
    {
        GameObject[] foodObjects = GameObject.FindGameObjectsWithTag("Food");
        if (foodObjects.Length == 0)
        {
            return snakePosition;
        }

        GameObject closestFood = foodObjects[0];
        float closestDistance = Vector2.Distance(snakePosition, closestFood.transform.position);
        foreach (GameObject food in foodObjects)
        {
            float distance = Vector2.Distance(snakePosition, food.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFood = food;
            }
        }

        return closestFood.transform.position;
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        // Extract the first discrete action.
        int action = actions.DiscreteActions[0];

        // Map the action to a directional vector.
        Vector2 newDirection = Vector2.zero;
        if (action == 0)
        {
            newDirection = Vector2.up;
        }
        else if (action == 1)
        {
            newDirection = Vector2.down;
        }
        else if (action == 2)
        {
            newDirection = Vector2.left;
        }
        else if (action == 3)
        {
            newDirection = Vector2.right;
        }
        
        // move snake to new direction
        snake.MoveGivenDirection(newDirection);

        // Apply a small step penalty to motivate shorter paths and efficient food collection.
        AddReward(-0.001f);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[0] = 0;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActions[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 2;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 3;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the snake head collides with an object tagged "Food"
        if (collision.CompareTag("edible"))
        {
            // Grow the snake.
            snake.Grow();

            // Destroy the food object.
            Destroy(collision.gameObject);

            // Optionally, spawn new food.
            
            if (SnakeGameManager.instance != null && FoodSpawner.Instance != null)
            {
                FoodSpawner.Instance.SpawnFood();
            }

            // Reward the agent for eating food.
            SnakeAgent agent = GetComponent<SnakeAgent>();
            if (agent != null)
            {
                agent.AddReward(1.0f);  // Adjust reward as needed.
            }
        }
        // If the snake head collides with a wall or its own body.
        else if (collision.CompareTag("Wall") || collision.CompareTag("Body"))
        {
            Debug.Log("Collision detected: " + collision.tag);

            // Penalize the agent for a collision.
            SnakeAgent agent = GetComponent<SnakeAgent>();
            if (agent != null)
            {
                agent.AddReward(-1.0f);  // Adjust penalty as needed.
                agent.EndEpisode();      // End the episode.
            }

            snake.Dead();
        }
    }
}