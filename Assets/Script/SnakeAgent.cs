using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SnakeAgent : Agent
{
    public bool isTrainingMode = false; 

    private Rigidbody rb;
    private EnviormentManager enviormentManager;
    private SnakeMovement snakeMovement;
    private int foodCollected;

    [SerializeField] int winScore = 20;
    [SerializeField] float agentRotationSpeed = 100f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        snakeMovement = GetComponent<SnakeMovement>();
        enviormentManager = GetComponentInParent<EnviormentManager>();

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
        foodCollected = 0;
    }

    private void ApplyPenalty()
    {
        if (isTrainingMode)
        {
            AddReward(-1.0f);
            enviormentManager.OnFailure();
            EndEpisode();
        }
        else
        {
            Debug.Log("Game Over - Game Mode");
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = new Vector3(
            actionBuffers.ContinuousActions[0],
            0,
            actionBuffers.ContinuousActions[1]
        );
        snakeMovement.SetMoveDirection(controlSignal);

        if (isTrainingMode)
        {
            AddReward(-0.0005f);
        }

        float rotationInput = actionBuffers.ContinuousActions[0];
        float rotationAngle = rotationInput * agentRotationSpeed * Time.fixedDeltaTime;
        Vector3 newDirection = Quaternion.Euler(0, rotationAngle, 0) * transform.forward;
        snakeMovement.SetMoveDirection(newDirection);

        if (isTrainingMode)
        {
            AddReward(-0.001f);
        }
    }

    private void EatReward()
    {
        if (isTrainingMode)
        {
            float efficiencyBonus = Mathf.Clamp(1.5f - (StepCount / (float)MaxStep), 0.1f, 1.5f);
            float streakBonus = foodCollected * 0.5f;
            AddReward(1f + efficiencyBonus + streakBonus);
        }
        else
        {
            Debug.Log("Mad spist i game mode – opdater score i UI");
        }

        foodCollected++;

        if (MaxStep < 1000 && isTrainingMode)
        {
            MaxStep += 500;
        }

        if (isTrainingMode && GetCumulativeReward() > winScore)
        {
            Ending();
        }
        else if (!isTrainingMode && foodCollected >= winScore)
        {
            Ending();
        }
    }

    void Ending()
    {
        Debug.Log("Afslutter episode / sejr.");
        enviormentManager.ResetAction?.Invoke();

        if (isTrainingMode)
        {
            EndEpisode();
        }
        else
        {
            Debug.Log("Win - Game Mode");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Body"))
        {
            if (isTrainingMode)
            {
                AddReward(-0.01f);
            }
            else
            {
                Debug.Log("Kollision med snake body i game mode");
            }
        }
        
        if (other.gameObject.CompareTag("Wall"))
        {
            if (isTrainingMode)
            {
                AddReward(-1.0f);
                enviormentManager.OnFailure();
                EndEpisode();
            }
            else
            {
                snakeMovement.ShrinkSnake();
                Debug.Log("Kollision med væg i game mode");
            }
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
