using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    [SerializeField] float movementSpeed = 5;
    private Vector3 movementDirection = Vector3.forward;
    Rigidbody rb;

    public Action EatenFood;

    [SerializeField] GameObject bodyPrefab;

    //Body
    private List<GameObject> bodyParts = new List<GameObject>();
    List<Vector3> segmentVelocity = new List<Vector3>();
    [SerializeField] float targetDistance = 1.5f;
    [SerializeField] float smoothSpeed = 5;
    [SerializeField] int startBodySize = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        for (int i = 0; i < startBodySize; i++)
            GrowSnake();
    }

    private void FixedUpdate()
    {
        Vector3 calSpeed = movementDirection * movementSpeed * Time.deltaTime;

        rb.linearVelocity = new Vector3(calSpeed.x, rb.linearVelocity.y, calSpeed.z);

        //Rotate to face movement direction
        if (movementDirection.sqrMagnitude < 0.1f) // Ensure there's movement
            return;

        //Movement
        rb.MoveRotation(Quaternion.Euler(0, GetAngelIn3D(movementDirection), 0));

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
            body.transform.localPosition = -transform.forward * targetDistance;
        bodyParts.Add(body);
        segmentVelocity.Add(Vector3.zero);
    }

    public void SetMoveDirection(Vector3 direction)
    {
        movementDirection = direction;
        if (movementDirection.magnitude > 1)
            movementDirection = movementDirection.normalized;
    }

    // Rotate on the y-axis
    float GetAngelIn3D(Vector3 direction)
    {
        return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Food")
        {
            EatenFood?.Invoke();
            collision.gameObject.GetComponent<Food>().Eaten();
            GrowSnake();
        }

    }
}
