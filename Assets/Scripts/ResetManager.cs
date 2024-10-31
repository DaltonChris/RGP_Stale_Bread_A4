using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetManager : MonoBehaviour
{
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private float holdTime = 2f;  // How long R must be held to reset the scene
    private float holdTimer = 0f;

    private void Start()
    {
        // Store the original positions of all draggable objects
        foreach (var obj in FindObjectsOfType<Draggable>())
        {
            originalPositions[obj.transform] = obj.transform.position;
        }
    }

    private void Update()
    {
        // Handle both tapping and holding R
        if (Input.GetKey(KeyCode.R))
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                ResetScene();  // Reset scene if R is held long enough
                holdTimer = 0f;  // Reset the timer
            }
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            if (holdTimer < holdTime)
            {
                ResetBall();  // Tap R to reset the ball
            }
            holdTimer = 0f;  // Reset the timer if R is released early
        }
    }

    void ResetBall()
    {

        // Find the active Ball instance and initiate the lerp reset
        Ball activeBall = FindObjectOfType<Ball>();
        if (activeBall != null)
        {
            StartCoroutine(activeBall.DestroyAfterLerp());
        }
    }


    private void ResetScene()
    {
        // Reset all draggable objects to their original positions
        foreach (var entry in originalPositions)
        {
            entry.Key.position = entry.Value;
        }

        ResetBall();  // Also reset the ball during scene reset
    }
}
