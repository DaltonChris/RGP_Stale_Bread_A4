using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Rigidbody2D rb;
    private Collider2D col;

    public float dragSpeed = 10f;  // Speed for smooth movement
    public float skinWidth = 0.01f;  // Extra padding to avoid clipping
    public float collisionBuffer = 0.001f;  // Small buffer to prevent jitter

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.isKinematic = true;  // Disable physics interactions
    }

    void Update()
    {
        if (Ball.IsBallActive) return;  // Prevent dragging when the ball is active

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Collider2D hit = Physics2D.OverlapPoint(mousePosition);

            if (hit != null && hit.transform == transform)
            {
                isDragging = true;
                offset = transform.position - mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 targetPosition = GetMouseWorldPosition() + offset;
            MoveTowardsTarget(targetPosition);
        }
    }

    // Handles smooth movement and collision handling
    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        Vector2 movementDirection = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        // Smooth movement towards the target position
        Vector3 interpolatedPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dragSpeed);

        // Perform collision checks only along the interpolation path
        if (!CheckForCollisionsAndSlide(movementDirection, interpolatedPosition))
        {
            transform.position = interpolatedPosition;  // Move only if no blocking collisions
        }
    }

    // Check for collisions along the movement path and adjust movement
    private bool CheckForCollisionsAndSlide(Vector2 direction, Vector3 targetPosition)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        RaycastHit2D[] hits = new RaycastHit2D[10];
        int hitCount = rb.Cast(direction, hits, distance + skinWidth);

        if (hitCount > 0)
        {
            Vector2 finalDirection = direction;
            float remainingDistance = distance;

            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider != col)
                {
                    // Calculate sliding direction along the collision surface
                    Vector2 collisionNormal = hit.normal;
                    Vector2 projectedDirection = Vector3.ProjectOnPlane(finalDirection, collisionNormal).normalized;

                    if (projectedDirection == Vector2.zero)
                    {
                        return true;  // Stop if wedged between objects
                    }

                    // Adjust remaining distance to just before the collision
                    remainingDistance = Mathf.Min(remainingDistance, hit.distance - skinWidth);
                    finalDirection = projectedDirection;
                }
            }

            // Apply movement along the adjusted sliding direction
            transform.position += (Vector3)finalDirection * (remainingDistance - collisionBuffer);
            return true;  // Collision was detected and handled
        }

        return false;  // No collisions, proceed with normal movement
    }

    // Get the mouse position in world coordinates
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;  // Keep it in 2D space
        return mousePosition;
    }
}