using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// Controls the snake agent in the ML-Agents environment. Inspired by the structure of PushAgentBasic,
/// this agent resets itself each episode by calling the SnakeMovement.ResetSnake() method and spawning new food.
/// </summary>
public class SnakeAgent : Agent
{
    // Private field for caching the SnakeMovement component.
    private SnakeMovement snakeMovement;

    /// <summary>
    /// Initializes the snake agent by caching required components.
    /// </summary>
    public override void Initialize()
    {
        // Cache the SnakeMovement component attached to this GameObject.
        snakeMovement = GetComponent<SnakeMovement>();
    }

    /// <summary>
    /// Called at the beginning of each episode to reset the snake and the environment.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reset the snake's position, direction, and body parts.
        snakeMovement.ResetSnake();

        // Spawn new food in the environment.
        FoodSpawner.Instance.SpawnFood();
    }

    /// <summary>
    /// Collects observations used by the agent. In this example we observe the current movement direction.
    /// </summary>
    /// <param name="sensor">The sensor that collects observations.</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Add the x component of the snake's current movement direction.
        sensor.AddObservation(snakeMovement.GetDirection().x);
        // Add the y component of the snake's current movement direction.
        sensor.AddObservation(snakeMovement.GetDirection().y);
    }

    /// <summary>
    /// Receives actions from the neural network or heuristic and calls the corresponding movement methods.
    /// Discrete actions:
    /// 0 - Move Up
    /// 1 - Move Down
    /// 2 - Move Left
    /// 3 - Move Right
    /// </summary>
    /// <param name="actions">The actions chosen by the agent.</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Retrieve the first discrete action from the action buffer.
        int moveAction = actions.DiscreteActions[0];

        // Map each discrete action to a movement method.
        switch (moveAction)
        {
            case 0:
                snakeMovement.MoveUp();
                break;
            case 1:
                snakeMovement.MoveDown();
                break;
            case 2:
                snakeMovement.MoveLeft();
                break;
            case 3:
                snakeMovement.MoveRight();
                break;
            default:
                // If an unknown action is received, do nothing.
                break;
        }

        // Optionally add a small time penalty each step to encourage the snake to finish its objective quickly.
        AddReward(-0.001f);
    }

    /// <summary>
    /// Provides manual control over the snake for debugging via keyboard inputs.
    /// </summary>
    /// <param name="actionsOut">The action buffer to populate with heuristic actions.</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Get the discrete actions output buffer.
        var discreteActions = actionsOut.DiscreteActions;
        // Map key presses to discrete actions.
        if (Input.GetKey(KeyCode.W))
            discreteActions[0] = 0;
        else if (Input.GetKey(KeyCode.S))
            discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.A))
            discreteActions[0] = 2;
        else if (Input.GetKey(KeyCode.D))
            discreteActions[0] = 3;
    }

    /// <summary>
    /// Called by SnakeMovement when the snake eats food. Rewards the agent.
    /// </summary>
    public void RewardForEating()
    {
        // Add a reward when food is consumed.
        AddReward(1.0f);
    }

    /// <summary>
    /// Called by SnakeMovement when the snake dies (e.g. by hitting a wall or its own body).
    /// Penalizes the agent and ends the episode.
    /// </summary>
    public void PenalizeForDeath()
    {
        // Apply a penalty for death.
        AddReward(-1.0f);
        // End the current episode, triggering OnEpisodeBegin.
        EndEpisode();
    }
}
