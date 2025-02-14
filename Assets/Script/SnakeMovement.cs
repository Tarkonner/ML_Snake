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
    List<Vector3> segmentVelocity = new List<Vector3>();
    [SerializeField] float targetDistance = 1.5f;
    [SerializeField] float smoothSpeed = 5;
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
        Debug.Log("Start");
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

        //Body parts movement
        for (int i = 0; i < bodyParts.Count; i++)
        {
            Transform previesPart;
            if (i - 1 < 0)
                previesPart = transform;
            else
                previesPart = bodyParts[i - 1].transform;

            //Segment velocity
            if (segmentVelocity.Count <= i)
                segmentVelocity.Add(Vector3.zero);

            // Use a temporary variable for velocity
            Vector3 tempVelocity = segmentVelocity[i];

            bodyParts[i].transform.position = Vector3.SmoothDamp(
                bodyParts[i].transform.position,
                previesPart.position - previesPart.forward * targetDistance,
                ref tempVelocity,
                smoothSpeed * Time.deltaTime);

            //bodyParts[i].transform.position = previesPart.position - previesPart.forward * targetDistance;
            segmentVelocity[i] = tempVelocity;

            //Rotation
            Vector3 direction = previesPart.position - bodyParts[i].transform.position;
            bodyParts[i].transform.eulerAngles = new Vector3(0, GetAngelIn3D(direction), 0);
        }

        // Fell off platform
        if (transform.localPosition.y < -0.1f)
        {
            Debug.Log("Snake fell off the platform! Penalizing agent.");
            FindFirstObjectByType<EnviormentManager>().OnFailure(); // Call blinking effect
            Dying?.Invoke();
            ResetSnake();
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
            body.transform.localPosition = previesBody.localPosition - previesBody.forward * targetDistance;
        }
        else
        {
            body.transform.localPosition = -transform.forward * targetDistance;
        }

        bodyParts.Add(body);
        segmentVelocity.Add(Vector3.zero);
        
        if (bodyParts.Count == TargetBodySize)  // Check if snake has 10 segments
        {
            OnReachedTargetSize?.Invoke(); // Invoke event to spawn target
        }
    }

    public void ResetSnake()
    {


        for (int i = bodyParts.Count - 1; i >= 0; i--)
        {
            Destroy(bodyParts[i]);
        }
        bodyParts.Clear();

        Dying?.Invoke();
    }
    public void SetMoveDirection(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        if (direction.normalized != desiredDirection.normalized)
            rotationValue = 1;

        desiredDirection = direction.normalized;
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
        
        // if snake collides with target and has more than 10 segments
        else if (collision.gameObject.CompareTag("Target") && bodyParts.Count >= TargetBodySize)
        {
            Debug.Log("Target hit");
            OnTargetReached?.Invoke();            
        }
    }
}
