using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SnakeAgent : Agent
{
    private const int MAX_ALLOWED_STEPS = 10000;
    
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

    public override void OnEpisodeBegin()
    {
        // Reset the snake's position and movement
        transform.localPosition = Vector3.zero;
        // Let the SnakeMovement script handle resetting the snake body
        snakeMovement.ResetSnake();

        MaxStep = 500;
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
        
        //sensor.AddObservation(snakeMovement.movementSpeed);
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
        // Increase allowed steps when food is eaten, but cap at MAX_ALLOWED_STEPS.
        MaxStep = Mathf.Min(MaxStep + 250, MAX_ALLOWED_STEPS);

        lastFoodPosition = enviormentManager.GetFreeSpace();
        AddReward(5.0f);
        foodCollected++;
        //snakeMovement.movementSpeed += 25;
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
    
    private void ApplyPenalty()
    {
        AddReward(-25.0f);
        enviormentManager.OnFailure();
        EndEpisode();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Body"))
        {
            Debug.Log("Collided with body! Penalizing agent.");
            AddReward(-0.5f); 
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall! Penalizing agent.");
            ApplyPenalty();
        }
    }

    private void Update()
    {
        int episode = this.CompletedEpisodes;
        int steps = this.StepCount;
        int maxSteps = this.MaxStep;
        // Hvis steps er 0 (dvs. en ny episode), vis 0 som belønning.
        float currentReward = steps == 0 ? 0 : GetCumulativeReward();

        StateManager.Instance.academyInfoText.text =
            $"Episode: {episode}\n" +
            $"Steps: {steps}/{maxSteps}\n" +
            $"Food Collected: {foodCollected}\n" +
            $"Reward: {currentReward:F2}";
    }
}
