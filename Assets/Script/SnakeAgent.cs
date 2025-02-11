
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
    }

    private void OnDisable()
    {
        snakeMovement.EatenFood -= EatReward;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log(sensor.ObservationSize());

        sensor.AddObservation(this.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];

        snakeMovement.SetMoveDirection(controlSignal);

        // Fell off platform
        if (this.transform.localPosition.y < -1)
        {
            Ending();
        }
    }

    private void EatReward()
    {
        Debug.Log("Reward");
        SetReward(1.0f);

        if (GetCumulativeReward() > winScore)
            Ending();


    }

    void Ending()
    {
        CallEnding?.Invoke();
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    /*
    public float forceMultiplier = 10;
    public Transform target;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rb.angularVelocity = Vector3.zero;
            this.rb.linearVelocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // Move the target to a new spot
        target.localPosition = new Vector3(Random.value * 8 - 4,
            0.5f,
            Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log(sensor.ObservationSize());

        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);

        
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rb.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, target.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            Debug.Log("Reward");
            SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    */
}
