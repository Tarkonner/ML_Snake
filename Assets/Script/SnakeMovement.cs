using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    private EnviormentManager enviormentManager;

    Rigidbody rb;

    [Header("Movement")]
    [SerializeField] float movementSpeed = 5;
    [SerializeField] float rotationSpeed = 5;
    private float rotationValue;
    private Vector3 desiredDirection = Vector3.forward;
    [SerializeField] float snakeHeadDiameter = 1.0f; // Approximate snake head diameter
    [SerializeField] float turningSpeed = 45.0f; // Degrees per second
    private float currentTurnAngle = 0f;

    [Header("Body")]
    [SerializeField] GameObject bodyPrefab;
    private List<GameObject> bodyParts = new List<GameObject>();
    [SerializeField] float desiredDistance = 1.0f;
    [SerializeField] float followSpeed = 5f;
    [SerializeField] int startBodySize = 3;

    [Header("Enviroment")]
    [SerializeField]
    private int TargetBodySize = 4;

    public Action Dying;
    public Action EatenFood;
    public Action OnReachedTargetSize;
    public Action OnTargetReached;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enviormentManager = GetComponentInParent<EnviormentManager>();

        rb = GetComponent<Rigidbody>();

        for (int i = 0; i < startBodySize; i++)
            GrowSnake();
    }

    // private void FixedUpdate()
    // {  
    //     
    //     //Movement
    //     Vector3 calSpeed = desiredDirection * movementSpeed * Time.deltaTime;
    //     rb.linearVelocity = new Vector3(calSpeed.x, rb.linearVelocity.y, calSpeed.z);
    //
    //     //Rotation
    //     if (rotationValue > 0)
    //     {
    //         rotationValue -= rotationSpeed;
    //         if(rotationValue < 0) 
    //             rotationValue = 0;
    //     }        
    //     Vector3 currentDirection = MathTool.GetVectorFromYAxis(transform.eulerAngles.y);
    //     Vector3 smoothMovement = Vector3.Slerp(desiredDirection, currentDirection, rotationValue);
    //     rb.MoveRotation(Quaternion.Euler(0, MathTool.GetAngelIn3D(smoothMovement), 0));
    //
    //     // Loop over each segment.
    //     for (int i = 0; i < bodyParts.Count; i++)
    //     {
    //         // The leader is the head for the first segment,
    //         // or the previous segment for subsequent ones.
    //         Transform leader = (i == 0) ? transform : bodyParts[i - 1].transform;
    //         Transform follower = bodyParts[i].transform;
    //
    //         // Compute the vector from the leader to the follower.
    //         Vector3 offset = follower.position - leader.position;
    //         float currentDistance = offset.magnitude;
    //
    //         // If the follower is too far or too close, move it to maintain the desired distance.
    //         if (Mathf.Abs(currentDistance - desiredDistance) > 0.01f)
    //         {
    //             // Compute the target position for the follower.
    //             Vector3 targetPos = leader.position + offset.normalized * desiredDistance;
    //             // Smoothly move the follower towards the target position.
    //             follower.position = Vector3.MoveTowards(follower.position, targetPos, followSpeed * Time.deltaTime);
    //         }
    //
    //         //Rotate forward last previels element
    //         follower.transform.eulerAngles = new Vector3(0, MathTool.GetAngelIn3D(-offset), 0);
    //     }
    //     // Fell off platform
    //     if (transform.localPosition.y < -1f)
    //     {
    //         Debug.Log("Snake fell off the platform! Penalizing agent.");
    //         enviormentManager.OnFailure(); // Call blinking effect
    //         SnakeDies();
    //     }
    // }
    
    private void FixedUpdate()
    {  
        // Always move forward
        Vector3 forwardMovement = transform.forward * movementSpeed * Time.deltaTime;
        rb.linearVelocity = new Vector3(forwardMovement.x, rb.linearVelocity.y, forwardMovement.z);

        // Smooth Turning
        if (rotationValue != 0)
        {
            float turnStep = turningSpeed * Time.deltaTime;
            currentTurnAngle = Mathf.Clamp(currentTurnAngle + rotationValue * turnStep, -turningSpeed, turningSpeed);
            transform.Rotate(Vector3.up, currentTurnAngle);
        }

        // Update Body Parts Movement
        UpdateBodyParts();
    
        // Fell off platform check
        if (transform.localPosition.y < -1f)
        {
            Debug.Log("Snake fell off the platform! Penalizing agent.");
            enviormentManager.OnFailure();
            SnakeDies();
        }
    }

    private void UpdateBodyParts()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            Transform leader = (i == 0) ? transform : bodyParts[i - 1].transform;
            Transform follower = bodyParts[i].transform;

            Vector3 offset = follower.position - leader.position;
            float currentDistance = offset.magnitude;

            if (Mathf.Abs(currentDistance - desiredDistance) > 0.01f)
            {
                Vector3 targetPos = leader.position + offset.normalized * desiredDistance;
                follower.position = Vector3.MoveTowards(follower.position, targetPos, followSpeed * Time.deltaTime);
            }

            follower.transform.eulerAngles = new Vector3(0, MathTool.GetAngelIn3D(-offset), 0);
        }
    }
    
    public void ResetSnake()
    {
        // Destroy all existing body parts.
        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (bodyParts[i] != null)
                Destroy(bodyParts[i]);
        }
        // Clear the list.
        bodyParts.Clear();
    
        // Recreate the snake with the starting body size.
        for (int i = 0; i < startBodySize; i++)
        {
            GrowSnake();
        }
    }


    private void GrowSnake()
    {
        // Instantiate body instance and
        // add it to the list
        GameObject body = Instantiate(bodyPrefab, transform.parent);
        
        if (bodyParts.Count != 0)
        {
            Transform previesBody = bodyParts[bodyParts.Count - 1].transform; //Get previus body
            body.transform.localPosition = previesBody.transform.localPosition - (previesBody.forward * desiredDistance);
        }
        else
        {
            //Get head
            body.transform.localPosition = -transform.forward * desiredDistance;
            
            body.GetComponent<SphereCollider>().enabled = false;
        }

        bodyParts.Add(body);
        
        if (bodyParts.Count == TargetBodySize)  // Check if snake has 10 segments
        {
            OnReachedTargetSize?.Invoke(); // Invoke event to spawn target
        }
    }

    public void SetMoveDirection(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        direction = direction.normalized;

        // Prevent backward movement (snake can't reverse)
        if (Vector3.Dot(direction, desiredDirection) < -0.9f)
            return;

        // Determine rotation direction (left or right)
        float angle = Vector3.SignedAngle(desiredDirection, direction, Vector3.up);
        rotationValue = angle > 0 ? 1 : -1;
    }

 
    private void SnakeDies()
    {
        //Call envts
        Dying?.Invoke();
        enviormentManager.ResetAction?.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            Food food = collision.gameObject.GetComponent<Food>();
            if (food != null && !food.IsEaten)
            {
                //Debug.Log($"Food");
                food.MarkAsEaten(); // Prevents multiple triggers
                EatenFood?.Invoke();
                GrowSnake();
            }
        }
        
        if (collision.gameObject.tag == "Wall")
        {
            SnakeDies();
        }
        
        // if snake collides with target and has more than 10 segments
        else if (collision.gameObject.CompareTag("Target") && bodyParts.Count >= TargetBodySize)
        {
            Debug.Log("Target hit");
            OnTargetReached?.Invoke();
        }
    }
}
