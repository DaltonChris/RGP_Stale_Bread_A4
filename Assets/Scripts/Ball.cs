using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    public event Action OnBallReset;  // Event to notify when the ball resets

    public float constantSpeed = 10f;  // Desired constant speed

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Normalize the velocity to maintain constant speed
        rb.velocity = rb.velocity.normalized * constantSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ensure velocity maintains constant speed after collision
        rb.velocity = rb.velocity.normalized * constantSpeed;
    }

    private void OnBecameInvisible()
    {
        // Reset the ball when it leaves the screen
        OnBallReset?.Invoke();
        Destroy(gameObject);
    }
}
