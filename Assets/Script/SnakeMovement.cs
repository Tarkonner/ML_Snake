using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    Rigidbody rb;

    [Header("Movement")]
    [SerializeField] float movementSpeed = 5;
    [SerializeField] float rotationSpeed = 5;
    private float rotationValue;
    private Vector3 desiredDirection = Vector3.forward;

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
        rb = GetComponent<Rigidbody>();

        StartGame();
    }

    public void StartGame()
    {
        for (int i = 0; i < startBodySize; i++)
            GrowSnake();
    }

    private void FixedUpdate()
    {        
        //Movement
        Vector3 calSpeed = desiredDirection * movementSpeed * Time.deltaTime;
        rb.linearVelocity = new Vector3(calSpeed.x, rb.linearVelocity.y, calSpeed.z);

        //Rotation
        if (rotationValue > 0)
        {
            rotationValue -= rotationSpeed;
            if(rotationValue < 0) 
                rotationValue = 0;
        }        
        Vector3 currentDirection = GetVectorFromYAxis(transform.eulerAngles.y);
        Vector3 smoothMovement = Vector3.Slerp(desiredDirection, currentDirection, rotationValue);
        rb.MoveRotation(Quaternion.Euler(0, GetAngelIn3D(smoothMovement), 0));

        // Define the desired distance between segments.


        // Loop over each segment.
        for (int i = 0; i < bodyParts.Count; i++)
        {
            // The leader is the head for the first segment,
            // or the previous segment for subsequent ones.
            Transform leader = (i == 0) ? transform : bodyParts[i - 1].transform;
            Transform follower = bodyParts[i].transform;

            // Compute the vector from the leader to the follower.
            Vector3 offset = follower.position - leader.position;
            float currentDistance = offset.magnitude;

            // If the follower is too far or too close, move it to maintain the desired distance.
            if (Mathf.Abs(currentDistance - desiredDistance) > 0.01f)
            {
                // Compute the target position for the follower.
                Vector3 targetPos = leader.position + offset.normalized * desiredDistance;
                // Smoothly move the follower towards the target position.
                follower.position = Vector3.MoveTowards(follower.position, targetPos, followSpeed * Time.deltaTime);
            }

            //Rotate forward last previels element
            follower.transform.eulerAngles = new Vector3(0, GetAngelIn3D(-offset), 0);
        }
        // Fell off platform
        if (transform.localPosition.y < -1f)
        {
            Debug.Log("Snake fell off the platform! Penalizing agent.");
            FindFirstObjectByType<EnviormentManager>().OnFailure(); // Call blinking effect
            Dying?.Invoke();
        }
    }

    private void GrowSnake()
    {
        // Instantiate body instance and
        // add it to the list
        GameObject body = Instantiate(bodyPrefab, transform.parent);
        if (bodyParts.Count != 0)
        {
            Transform previesBody = bodyParts[bodyParts.Count - 1].transform;
            //Position
            body.transform.localPosition = previesBody.transform.localPosition - (previesBody.forward * desiredDistance);
        }
        else
        {
            body.transform.localPosition = -transform.forward * desiredDistance;
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

        if (Vector3.Dot(direction, desiredDirection) < -0.9f)
            return;

        if (direction.normalized != desiredDirection.normalized)
            rotationValue = 1;

        desiredDirection = direction;
    }

    // Rotate on the y-axis
    float GetAngelIn3D(Vector3 direction)
    {
        return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }

    Vector3 GetVectorFromYAxis(float angle)
    {
        float radian = angle * Mathf.Deg2Rad; // Convert degrees to radians
        return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
    }

 

    private void OnCollisionEnter(Collision collision)
    {
        // if snake collides with food
        if (collision.gameObject.CompareTag("Food"))
        {
            EatenFood?.Invoke();
            collision.gameObject.GetComponent<Food>().Eaten();
            GrowSnake();
        }

        if (collision.gameObject.tag == "Wall")
        {
            Dying?.Invoke(); 
        }
        // if snake collides with target and has more than 10 segments
        else if (collision.gameObject.CompareTag("Target") && bodyParts.Count >= TargetBodySize)
        {
            Debug.Log("Target hit");
            OnTargetReached?.Invoke();
        }
    }
}
