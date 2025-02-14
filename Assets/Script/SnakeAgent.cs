
using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SnakeAgent : Agent
{
    private EnviormentManager enviormentManager;
    private SnakeMovement snakeMovement;


    [SerializeField] int winScore = 20;

    //public override void Initialize()
    //{
    //    Time.timeScale = 1;
    //}
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
        snakeMovement.EatenFood -= EatReward;
        snakeMovement.Dying -= ApplyPenalty;
        snakeMovement.OnTargetReached -= TargetReward; 
    }
    
    private void ApplyPenalty()
    {
        AddReward(-3.0f); // Give a penalty of -3
        enviormentManager.OnFailure(); // Trigger floor blinking red
        EndEpisode(); // End the episode after penalty
    }    

    public override void OnEpisodeBegin()
    {
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
}
