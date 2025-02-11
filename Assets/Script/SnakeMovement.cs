using System;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    [SerializeField] float movementSpeed = 5;
    private Vector3 movementDirection = Vector3.forward;
    Rigidbody rb;

    public Action EatenFood;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 calSpeed = movementDirection * movementSpeed * Time.deltaTime;

        rb.linearVelocity = new Vector3(calSpeed.x, rb.linearVelocity.y, calSpeed.z);
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
        }

    }
}
