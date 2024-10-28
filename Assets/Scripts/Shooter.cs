using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject ballPrefab;      // Ball to be launched
    public Transform spawnPoint;       // Spawn location (at the top of the player)
    public LineRenderer lineRenderer;  // Line to show trajectory
    public float maxLaunchForce = 15f; // Maximum force for launch

    private bool ballActive = false;   // Track if a ball is active

    void Update()
    {
        RotateToFaceMouse();  // Always rotate the player to face the mouse

        if (!ballActive)
        {
            UpdateTrajectory();  // Only show trajectory if no ball is active

            if (Input.GetMouseButtonDown(0))
            {
                LaunchBall();
            }
        }
    }

    void RotateToFaceMouse()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UpdateTrajectory()
    {
        lineRenderer.enabled = true;  // Make sure the line is enabled

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 launchDirection = (mousePosition - (Vector2)spawnPoint.position).normalized;

        DrawTrajectory(spawnPoint.position, launchDirection * maxLaunchForce);
    }

    void LaunchBall()
    {
        ballActive = true;  // Set ball as active
        lineRenderer.enabled = false;  // Disable the trajectory line

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 launchDirection = (mousePosition - (Vector2)spawnPoint.position).normalized;

        GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        rb.AddForce(launchDirection * maxLaunchForce, ForceMode2D.Impulse);

        Ball ballScript = ball.GetComponent<Ball>();
        ballScript.OnBallReset += ResetBall;  // Subscribe to reset event
    }

    void DrawTrajectory(Vector2 startPoint, Vector2 initialVelocity)
{
    List<Vector2> trajectoryPoints = new List<Vector2>();  // Store trajectory points
    Vector2 currentPoint = startPoint;  // Start at the spawn point
    Vector2 velocity = initialVelocity;  // Use the initial velocity for calculations
    float timeStep = 0.05f;  // Smaller time steps for precision

    trajectoryPoints.Add(currentPoint);  // Add the starting point

    // Loop through trajectory points until a collision occurs or the maximum steps are reached
    for (int i = 0; i < 100; i++)  // More points for smoother trajectory
    {
        // Calculate the next point using kinematics: next = current + velocity * t + 0.5 * gravity * t^2
        Vector2 nextPoint = currentPoint + velocity * timeStep + 0.5f * Physics2D.gravity * (timeStep * timeStep);

        // Perform a raycast between the current point and the next point
        RaycastHit2D hit = Physics2D.Raycast(currentPoint, nextPoint - currentPoint, (nextPoint - currentPoint).magnitude);

        if (hit.collider != null)
        {
            // Add the hit point to the trajectory and stop further calculations
            trajectoryPoints.Add(hit.point);
            break;
        }
        else
        {
            // No collision detected, add the next point to the trajectory
            trajectoryPoints.Add(nextPoint);
            currentPoint = nextPoint;  // Move to the next point

            // Update velocity for the next step (gravity affects vertical velocity)
            velocity += Physics2D.gravity * timeStep;
        }
    }

    // Apply the calculated points to the LineRenderer
    lineRenderer.positionCount = trajectoryPoints.Count;
    for (int i = 0; i < trajectoryPoints.Count; i++)
    {
        lineRenderer.SetPosition(i, trajectoryPoints[i]);
    }
}
    void ResetBall()
    {
        ballActive = false;  // Set ball as inactive
    }
}
