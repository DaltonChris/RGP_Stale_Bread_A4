using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    public static bool IsBallActive = false;
    public event Action OnBallReset;  // Event to notify when the ball resets

    private Rigidbody2D rb;
    public GameObject shootTrail;
    private GameObject existingTrail;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsBallActive = true;  // Sets ball as active


        existingTrail = GameObject.FindWithTag("ShootTrail");//Check if a trail is already in the scene
        if(existingTrail != null)
        {
            existingTrail.GetComponent<TrailRenderer>().startColor = Color.black;
            existingTrail.GetComponent<TrailRenderer>().endColor = Color.black;
        }
    }

    private void OnBecameInvisible()
    {
        // Notify the shooter to reset when the ball leaves the screen
        OnBallReset?.Invoke();
        IsBallActive = false; // Sets ball as inactive

        if(existingTrail != null)
        {
            Destroy(existingTrail);//If it is, destroy it
        }
        shootTrail.gameObject.tag = "ShootTrail";//Add tag to new one so it can be found by the next ball
        shootTrail.transform.parent = null;//Unparent so it doesn't get destroyed

        Destroy(gameObject);
    }
}
