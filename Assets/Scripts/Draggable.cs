using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Rigidbody2D rb;
    private Collider2D col;

    public float dragSpeed = 0.05f;  // Speed for smooth movement during dragging

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.isKinematic = true;  // Ensure it doesn't react to physics forces
    }

    void Update()
    {

        if (Ball.IsBallActive) return; // prevent dragging of objects while ball is active in scene

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

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
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            Vector3 targetPosition = mousePosition + offset;

            // Use Rigidbody2D.Cast to detect potential collisions along the drag direction
            Vector2 movementDirection = (targetPosition - transform.position).normalized;
            float distance = (targetPosition - transform.position).magnitude;

            RaycastHit2D[] hits = new RaycastHit2D[10];  // Array to store collision hits
            int hitCount = rb.Cast(movementDirection, hits, distance);

            bool canMove = true;
            Vector2 slidingDirection = movementDirection;

            if (hitCount > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider != null && hit.collider.transform != transform)
                    {
                        canMove = false;

                        // Calculate the sliding direction along the hit surface
                        Vector2 collisionNormal = hit.normal;
                        slidingDirection = Vector3.ProjectOnPlane(movementDirection, collisionNormal).normalized;
                        break;
                    }
                }
            }

            // Move the object either directly or with sliding
            if (canMove)
            {
                transform.position = targetPosition;
            }
            else
            {
                transform.position += (Vector3)slidingDirection * dragSpeed;
            }
        }
    }
}