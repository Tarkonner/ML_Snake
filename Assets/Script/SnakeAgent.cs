
using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SnakeAgent : Agent
{
    private SnakeMovement snakeMovement;

    public Action CallEnding;

    [SerializeField] int winScore = 20;

    private void Awake()
    {
        snakeMovement = GetComponent<SnakeMovement>();
        snakeMovement.EatenFood += EatReward;
        snakeMovement.Dying += ApplyPenalty;
        snakeMovement.OnTargetReached += TargetReward; 
    }

    private void OnDisable()
    {
        snakeMovement.EatenFood -= EatReward;
        snakeMovement.Dying -= ApplyPenalty;
        snakeMovement.OnTargetReached -= TargetReward; 
    }
    
    private void ApplyPenalty()
    {
        Debug.Log("Applying penalty for falling off!");
        AddReward(-3.0f); // Give a penalty of -3
        FindFirstObjectByType<EnviormentManager>().OnFailure(); // Trigger floor blinking red
        EndEpisode(); // End the episode after penalty
    }

    

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
        //FindFirstObjectByType<EnviormentManager>().GetComponent<Renderer>().material.color = Color.gray;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log(sensor.ObservationSize());

        sensor.AddObservation(this.transform.localPosition);
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
        FindFirstObjectByType<EnviormentManager>().OnSuccess();
        Ending(); // End episode
    }


    void Ending()
    {
        Debug.Log("Ending");

        CallEnding?.Invoke();
        EndEpisode();
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
