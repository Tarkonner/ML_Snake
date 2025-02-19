using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SnakeAgent : Agent
{
    private Rigidbody rb;

    private EnviormentManager enviormentManager;
    private SnakeMovement snakeMovement;

    private int foodCollected;

    [SerializeField] int winScore = 20;

    // Agent's rotation speed (in degrees per second) for controlling the snake
    [SerializeField] float agentRotationSpeed = 100f;

    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        snakeMovement = GetComponent<SnakeMovement>();
        enviormentManager = GetComponentInParent<EnviormentManager>();

        // Subscribe to events from the movement script
        snakeMovement.EatenFood += EatReward;
        snakeMovement.Dying += ApplyPenalty;
    }

    private void OnDisable()
    {
        if (snakeMovement != null)
        {
            snakeMovement.EatenFood -= EatReward;
            snakeMovement.Dying -= ApplyPenalty;
        }
    }

    public override void OnEpisodeBegin()
    {
        MaxStep = 1500;
        transform.localPosition = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        snakeMovement?.ResetSnake();

        foodCollected = 0;
        enviormentManager.MoveAllFood();
    }

    private void ApplyPenalty()
    {
        AddReward(-1.0f);
        enviormentManager.OnFailure();
        EndEpisode();
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
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        snakeMovement.SetMoveDirection(controlSignal);

        // Step Penalty: Prevent wandering aimlessly
        AddReward(-0.0005f);

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
    }


    private void EatReward()
    {
        float efficiencyBonus = Mathf.Clamp(1.5f - (StepCount / (float)MaxStep), 0.1f, 1.5f);
        float streakBonus = foodCollected * 0.5f; // Encourage consecutive pickups
    
        // Increase base food reward
        AddReward(10.0f + efficiencyBonus + streakBonus);
    
        foodCollected++; 
        
        // if less then 1000 steps add more steps
        if (MaxStep < 1000)
        {
            MaxStep += 500;
        }
        // Increase allowed steps when food is eaten.
        MaxStep += 1000;
        AddReward(10.0f);

        foodCollected++;

        if (GetCumulativeReward() > winScore)
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
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Body"))
        {
            AddReward(-0.01f);
        }
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
