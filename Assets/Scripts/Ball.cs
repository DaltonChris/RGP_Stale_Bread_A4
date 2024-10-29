using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    public static bool IsBallActive = false;
    public event Action OnBallReset;  // Event to notify when the ball resets

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsBallActive = true;  // Sets ball as active
    }

    private void OnBecameInvisible()
    {
        // Notify the shooter to reset when the ball leaves the screen
        OnBallReset?.Invoke();
        IsBallActive = false; // Sets ball as inactive
        Destroy(gameObject);
    }

    // Collison check for objects that need to be avoided
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Destroy"))
        {
            OnBecameInvisible(); // Call Method to reset ball
        }
    }
}
