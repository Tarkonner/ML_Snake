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
    Vector3[] segmentsPos;
    Vector3[] segDamp;
    [SerializeField] float targetDistance = 1.5f;
    [SerializeField] float smoothSpeed = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        GrowSnake();
        GrowSnake();

        segDamp = new Vector3[bodyParts.Count];
        segmentsPos = new Vector3[bodyParts.Count];
    }

    private void FixedUpdate()
    {
        Vector3 calSpeed = movementDirection * movementSpeed * Time.deltaTime;

        rb.linearVelocity = new Vector3(calSpeed.x, rb.linearVelocity.y, calSpeed.z);

        //Rotate to face movement direction
        if (movementDirection.sqrMagnitude > 0.01f) // Ensure there's movement
        {
            float angle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg;
            rb.MoveRotation(Quaternion.Euler(0, angle, 0)); // Rotate on the Z-axis
        }

        //segmentsPos[0] = -transform.forward * targetDistance;

        //for (int i = 1; i < segmentsPos.Length; i++)
        //{
        //    segmentsPos[i] = Vector3.SmoothDamp(
        //        segmentsPos[i], 
        //        segmentsPos[i - 1] + transform.forward * targetDistance, 
        //        ref segDamp[i], 
        //        smoothSpeed);
        //}

        //for (int i = 0; i < transform.childCount; i++)
        //{
        //    transform.GetChild(i).transform.position = segmentsPos[i];
        //}
    }

    private void GrowSnake()
    {
        // Instantiate body instance and
        // add it to the list
        GameObject body = Instantiate(bodyPrefab, transform);
        if (bodyParts.Count != 0)
        {
            Transform previesBody = bodyParts[bodyParts.Count - 1].transform;
            body.transform.localPosition = previesBody.localPosition - previesBody.forward * targetDistance;

        }
        else
            body.transform.localPosition = -transform.forward * targetDistance;
        bodyParts.Add(body);
    }

    public void SetMoveDirection(Vector3 direction)
    {
        movementDirection = direction;
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
