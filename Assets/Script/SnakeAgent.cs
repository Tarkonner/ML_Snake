using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SnakeAgent : Agent
{
    private EnviormentManager enviormentManager;
    private SnakeMovement snakeMovement;


    [SerializeField] int winScore = 20;

    private void Awake()
    {
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
    
    private void ApplyPenalty()
    {
        //Debug.Log("Applying penalty for falling off!");
        AddReward(-3.0f); // Give a penalty of -3
        enviormentManager.OnFailure(); // Trigger floor blinking red
        EndEpisode(); // End the episode after penalty
    }

    

    public override void OnEpisodeBegin()
    {
        //Debug.Log("OnEpisodeBegin");
        MaxStep = 1000;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];

        snakeMovement.SetMoveDirection(controlSignal);
    }

    private void EatReward()
    {
        MaxStep += 1000;
        Debug.Log("Reward");
        AddReward(1.0f);

        if (GetCumulativeReward() > winScore)
            Ending();
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
    
    // if collide with body, add small penalty eacvh frame in contact
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Body"))
        {
            Debug.Log("Collided with body! Penalizing agent.");
            AddReward(-0.01f); // Give a penalty of -0.01
        }
    }
    
    private void Update()
    {
        if (StateManager.Instance.academyInfoText != null)
        {
            int episode = this.CompletedEpisodes;  // Get the number of completed episodes **for this agent**
            int steps = this.StepCount;            // Get steps taken by **this agent**
            float currentReward = GetCumulativeReward(); // Correct cumulative reward

            // Update UI text
            StateManager.Instance.academyInfoText.text = 
                $"Episode: {episode}\n" +
                $"Steps: {steps}\n" +
                $"Reward: {currentReward:F2}";
        }
    }

}