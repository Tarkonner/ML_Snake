using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SnakeAgent : Agent
{
    private EnviormentManager enviormentManager;
    private SnakeMovement snakeMovement;

    private Vector3 lastFoodPosition;
    private float previousDistanceToFood = float.MaxValue;

    private int foodCollected;

    public TargetDummyManager targetDummyManager;

    [SerializeField] int winScore = 20;

    // Agent's rotation speed (in degrees per second) for controlling the snake
    [SerializeField] float agentRotationSpeed = 100f;

    

    private void Awake()
    {
        

        snakeMovement = GetComponent<SnakeMovement>();
        enviormentManager = GetComponentInParent<EnviormentManager>();

        // Subscribe to events from the movement script
        snakeMovement.EatenFood += EatReward;
        snakeMovement.Dying += ApplyPenalty;
        snakeMovement.OnTargetReached += TargetReward;
    }

    private void OnDisable()
    {
        if (snakeMovement != null)
        {
            snakeMovement.EatenFood -= EatReward;
            snakeMovement.Dying -= ApplyPenalty;
            snakeMovement.OnTargetReached -= TargetReward;
        }
    }

    private void ApplyPenalty()
    {
        AddReward(-1.0f);
        enviormentManager.OnFailure();
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        // Reset the snake's position and movement
        transform.localPosition = Vector3.zero;
        // Let the SnakeMovement script handle resetting the snake body
        snakeMovement.ResetSnake();

        MaxStep = 1000;
        lastFoodPosition = enviormentManager.GetFreeSpace();
        previousDistanceToFood = float.MaxValue;
        foodCollected = 0;

        enviormentManager.MoveAllFood();
        targetDummyManager.ResetTargetDummy();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the snake's position and rotation.
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation);
        // Optionally, observe the current forward speed or movementSpeed from SnakeMovement.
        //sensor.AddObservation(snakeMovement == null ? 0 : snakeMovement.movementSpeed);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Use a single continuous action for turning.
        float rotationInput = actionBuffers.ContinuousActions[0];

        // Calculate the rotation angle based on input, the agent's rotation speed, and the fixed delta time.
        float rotationAngle = rotationInput * agentRotationSpeed * Time.fixedDeltaTime;

        // Rotate the current forward direction by the computed angle.
        Vector3 newDirection = Quaternion.Euler(0, rotationAngle, 0) * transform.forward;

        // Pass the new desired direction to the SnakeMovement component.
        snakeMovement.SetMoveDirection(newDirection);

        // Reward shaping: small time penalty
        AddReward(-0.001f);

        // Reward shaping based on the distance to the current food target.
        float currentDistance = Vector3.Distance(transform.localPosition, lastFoodPosition);
        if (currentDistance < previousDistanceToFood)
        {
            AddReward(0.01f); // Reward for getting closer
        }
        else
        {
            AddReward(-0.01f); // Penalty for moving away
        }
        previousDistanceToFood = currentDistance;
    }

    private void EatReward()
    {
        // Increase allowed steps when food is eaten.
        MaxStep += 1000;
        lastFoodPosition = enviormentManager.GetFreeSpace();
        AddReward(10.0f);

        foodCollected++;

        if (GetCumulativeReward() > winScore)
            Ending();
    }

    private void TargetReward()
    {
        Debug.Log("Target reached! Rewarding agent.");
        AddReward(10.0f);
        enviormentManager.OnSuccess();
        Ending();
    }

    void Ending()
    {
        Debug.Log("Ending episode.");
        enviormentManager.ResetAction?.Invoke();
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Use the horizontal input axis to control rotation.
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
    }

    private void Update()
    {
        if (StateManager.Instance.academyInfoText == null)
            return;

        int episode = this.CompletedEpisodes;
        int steps = this.StepCount;
        int maxSteps = this.MaxStep;
        float currentReward = GetCumulativeReward();

        StateManager.Instance.academyInfoText.text =
            $"Episode: {episode}\n" +
            $"Steps: {steps}/{maxSteps}\n" +
            $"Food Collected: {foodCollected}\n" +
            $"Reward: {currentReward:F2}";
    }
}
