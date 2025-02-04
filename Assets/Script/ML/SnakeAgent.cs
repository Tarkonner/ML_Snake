using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SnakeAgent : Agent
{
    private SnakeMovement snakeMovement;
    private Vector2 movementDirection = Vector2.up;

    public override void Initialize()
    {
        snakeMovement = GetComponent<SnakeMovement>();
    }

    public override void OnEpisodeBegin()
    {
        snakeMovement.ResetSnake();
        FoodSpawner.Instance.SpawnFood();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe movement direction
        sensor.AddObservation(movementDirection.x);
        sensor.AddObservation(movementDirection.y);

        // Ray Perception Sensor handles additional vision-based observations
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];
        Debug.Log($"Agent received action: {moveAction}");

        switch (moveAction)
        {
            case 0: snakeMovement.MoveUp(); break;
            case 1: snakeMovement.MoveDown(); break;
            case 2: snakeMovement.MoveLeft(); break;
            case 3: snakeMovement.MoveRight(); break;
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.S)) discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.A)) discreteActions[0] = 2;
        if (Input.GetKey(KeyCode.D)) discreteActions[0] = 3;
    }

    public void RewardForEating()
    {
        AddReward(1.0f);
    }

    public void PenalizeForDeath()
    {
        AddReward(-1.0f);
        EndEpisode();
    }
}