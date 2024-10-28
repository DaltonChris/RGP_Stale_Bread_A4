using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    private bool isDragging = false;    // Track if the object is being dragged
    private Vector3 offset;             // Offset between mouse and object position
    private Collider2D objectCollider;  // Reference to this object's collider
    private ContactFilter2D contactFilter;  // Filter to ignore triggers and non-solid objects

    void Start()
    {
        objectCollider = GetComponent<Collider2D>();  // Get the collider

        // Setup a contact filter to ignore triggers and only detect solid colliders
        contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.AllLayers);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))  // Start dragging with RMB
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

        if (Input.GetMouseButtonUp(1))  // Stop dragging when RMB is released
        {
            isDragging = false;
        }

        if (isDragging)  // Move the object while dragging
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            Vector3 newPosition = mousePosition + offset;

            // Only move the object if it doesn't overlap with others
            if (!WouldOverlapAtPosition(newPosition))
            {
                transform.position = newPosition;
            }
        }
    }

    bool WouldOverlapAtPosition(Vector3 newPosition)
    {
        // Temporarily move the object to the new position
        Vector3 originalPosition = transform.position;
        transform.position = newPosition;

        // Disable the collider to avoid self-detection
        objectCollider.enabled = false;

        // Check if the object's collider would overlap with others
        Collider2D[] results = new Collider2D[10];  // Adjust size if needed
        int overlapCount = objectCollider.OverlapCollider(contactFilter, results);

        // Re-enable the collider and move back to original position
        objectCollider.enabled = true;
        transform.position = originalPosition;

        return overlapCount > 0;  // Return true if any overlapping collider is detected
    }
}