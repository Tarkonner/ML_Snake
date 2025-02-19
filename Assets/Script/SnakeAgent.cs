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
    
    private Rigidbody rb;
    private int foodCollected;


    [SerializeField] int winScore = 20;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        snakeMovement = GetComponent<SnakeMovement>();
        enviormentManager = GetComponentInParent<EnviormentManager>();

        //Evnets
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
        MaxStep = 1500; 
        transform.localPosition = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    
        snakeMovement?.ResetSnake();
    
        foodCollected = 0;
        lastFoodPosition = enviormentManager.GetFreeSpace();
        enviormentManager.MoveAllFood();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation);
        sensor.AddObservation(rb.linearVelocity.magnitude);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        snakeMovement.SetMoveDirection(controlSignal);

        // Step Penalty: Prevent wandering aimlessly
        AddReward(-0.0005f);

        // Add a small reward for decreasing distance to food
        float currentDistance = Vector3.Distance(transform.localPosition, lastFoodPosition);
        if (currentDistance < previousDistanceToFood)
        {
            AddReward(0.0003f);
        }
        else
        {
            AddReward(-0.0003f);
        }
        previousDistanceToFood = currentDistance;
    }


    private void EatReward()
    {
        Debug.Log("Food eaten! Rewarding agent.");
        float efficiencyBonus = Mathf.Clamp(1.5f - (StepCount / (float)MaxStep), 0.1f, 1.5f);
        float streakBonus = foodCollected * 0.5f; // Encourage consecutive pickups
    
        // Increase base food reward
        AddReward(10.0f + efficiencyBonus + streakBonus);
    
        foodCollected++; 
        lastFoodPosition = enviormentManager.GetFreeSpace();
        
        // if less then 1000 steps add more steps
        if (MaxStep < 1000)
        {
            MaxStep += 500;
        }
    }

    
    private void TargetReward()
    {
        Debug.Log("Target reached! Rewarding agent.");
        AddReward(5.0f); // Give 5 points
        enviormentManager.OnSuccess();
        Ending(); // End episode
    }


    void Ending()
    {
        Debug.Log("Ending");

        enviormentManager.ResetAction?.Invoke();
        EndEpisode();
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    
    // // if collide with body, add small penalty eacvh frame in contact
    // private void OnCollisionEnter(Collision other)
    // {
    //     if (other.gameObject.CompareTag("Body"))
    //     {
    //         Debug.Log("Collided with body! Penalizing agent.");
    //         AddReward(-0.005f); // Give a penalty of -0.01
    //     }
    // }
    
    private void ApplyPenalty()
    {
        AddReward(-3.0f);
        enviormentManager.OnFailure();
        EndEpisode();
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
        if (!StateManager.Instance.academyInfoText)
        {
            return;
        }

        int episode = this.CompletedEpisodes;  // Get the number of completed episodes **for this agent**
        int steps = this.StepCount;            // Get steps taken by **this agent**
        int maxSteps = this.MaxStep;           // Get the maximum number of steps per episode
        float currentReward = GetCumulativeReward(); // Correct cumulative reward
        

        // Update UI text
        StateManager.Instance.academyInfoText.text = 
            $"Episode: {episode}\n" +
            $"Steps: {steps}/{maxSteps}\n" +   
            $"Food Collected: {foodCollected}\n" +  // New Debugging Info
            $"Reward: {currentReward:F2}";

    }
}