using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Shooter : MonoBehaviour
{
    public GameObject ballPrefab;      // Ball to be launched
    public Transform spawnPoint;       // The spawn location at the top of the player
    public LineRenderer lineRenderer;  // Line to show trajectory
    public GameObject shootParticles;  // Particles to fire when shooting
    public float maxLaunchForce = 15f; // Maximum force for launch
    public float minAngle = -80f;      // Minimum rotation angle
    public float maxAngle = 80f;       // Maximum rotation angle
    public Light2D globalLight;        // Global light component in scene

    [SerializeField]private bool ballActive = false;   // Track if a ball is active
    public float recoilDistance = 0.2f; //  recoil distance
    public float recoilDuration = 0.05f; //  recoil effect duratuin

    void Update()
    {
        RotateToFaceMouse();  // Rotate player towards mouse with clamped angles
        

        if (!ballActive && !ResetManager.Instance.victoryUI.activeInHierarchy)
        {
            UpdateTrajectory();  // Show trajectory preview

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (ResetManager.Instance.resetUI.activeInHierarchy)
                { StartCoroutine(ResetManager.Instance.ResetFadeOut());
                }else{ LaunchBall(); }// Launch the ball when the mouse is clicked
            }
        }
    }


    void RotateToFaceMouse()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        // Calculate the angle based on the mouse direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;

        // Clamp the rotation angle within the allowed range
        angle = Mathf.Clamp(angle, minAngle, maxAngle);

        // Apply the clamped rotation to the player
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UpdateTrajectory()
    {
        lineRenderer.enabled = true;  // Ensure the LineRenderer is active

        // Use the direction from the spawnPoint's forward direction (top side)
        Vector2 launchDirection = spawnPoint.up;  // spawnPoint.up gives the correct direction

        DrawTrajectory(spawnPoint.position, launchDirection * maxLaunchForce);
    }

    void LaunchBall()
    {
        ballActive = true;  // Set the ball as active
        lineRenderer.enabled = false;  // Disable the trajectory during play
        globalLight.intensity = 0.15f;

        // Use the spawnPoint's up direction for consistent launch
        Vector2 launchDirection = spawnPoint.up;

        // Instantiate and launch the ball
        GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        rb.AddForce(launchDirection * maxLaunchForce, ForceMode2D.Impulse);

        //Add shoot particles
        Instantiate(shootParticles, spawnPoint.transform.position, Quaternion.identity);
        // start recoil effect
        StartCoroutine(Recoil(launchDirection));

        // Subscribe to the reset event
        Ball ballScript = ball.GetComponent<Ball>();
        ballScript.OnBallReset += ResetBall;
    }

    IEnumerator Recoil(Vector2 launchDirection)
    {
        Vector3 originalPosition = transform.position;
        Vector3 recoilPosition = originalPosition - (Vector3)(launchDirection * recoilDistance);

        //recoil position
        float elapsedTime = 0f;
        while (elapsedTime < recoilDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, recoilPosition, elapsedTime / recoilDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Return to original position
        elapsedTime = 0f;
        while (elapsedTime < recoilDuration)
        {
            transform.position = Vector3.Lerp(recoilPosition, originalPosition, elapsedTime / recoilDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }

    void DrawTrajectory(Vector2 startPoint, Vector2 initialVelocity)
    {
        List<Vector2> trajectoryPoints = new List<Vector2>();  // Store trajectory points
        Vector2 currentPoint = startPoint;
        Vector2 velocity = initialVelocity;
        float timeStep = 0.05f;  // Small time steps for precision

        trajectoryPoints.Add(currentPoint);  // Add the starting point

        // Loop through trajectory points until collision or max steps
        for (int i = 0; i < 100; i++)
        {
            Vector2 nextPoint = currentPoint + velocity * timeStep + 0.5f * Physics2D.gravity * (timeStep * timeStep);

            // Check for collisions along the path
            RaycastHit2D hit = Physics2D.Raycast(currentPoint, nextPoint - currentPoint, (nextPoint - currentPoint).magnitude);

            if (hit.collider != null)
            {
                trajectoryPoints.Add(hit.point);  // Add hit point
                break;  // Stop the trajectory at the hit point
            }
            else
            {
                trajectoryPoints.Add(nextPoint);  // Add the next point
                currentPoint = nextPoint;  // Update current point
                velocity += Physics2D.gravity * timeStep;  // Update velocity for gravity
            }
        }

        // Apply the trajectory points to the LineRenderer
        lineRenderer.positionCount = trajectoryPoints.Count;
        for (int i = 0; i < trajectoryPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, trajectoryPoints[i]);
        }
    }

    void ResetBall()
    {
        ballActive = false;  // Reset the ball state
        globalLight.intensity = 1;
    }
}
