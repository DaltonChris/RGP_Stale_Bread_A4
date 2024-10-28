using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    public event Action OnBallReset;  // Event to notify when the ball resets

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnBecameInvisible()
    {
        // Notify the shooter to reset when the ball leaves the screen
        OnBallReset?.Invoke();
        Destroy(gameObject);
    }
}
