
using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SnakeAgent : Agent
{
    private SnakeMovement snakeMovement;

    public Action CallEnding;

    [SerializeField] int winScore = 4;

    private void Awake()
    {
        snakeMovement = GetComponent<SnakeMovement>();
        snakeMovement.EatenFood += EatReward;
        snakeMovement.Dying += EndEpisode;
    }

    private void OnDisable()
    {
        snakeMovement.EatenFood -= EatReward;
        snakeMovement.Dying -= EndEpisode;
    }

    public override void OnEpisodeBegin()
    {

    }


    public override void Initialize()
    {
        base.Initialize();
        Time.timeScale = 1;
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
        Debug.Log("Reward");
        AddReward(1.0f);

        if (GetCumulativeReward() > winScore)
            Ending();
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
