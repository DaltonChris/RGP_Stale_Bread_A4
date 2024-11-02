using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Draggable : MonoBehaviour
{
    private bool isDragging = false;
    private bool isHovering = false;
    private Vector3 offset;
    private Rigidbody2D rb;
    private Collider2D col;

    public float dragSpeed = 10f;            // Speed for smooth movement
    public float rotationSpeed = 100f;       // Speed of rotation when using arrow keys
    public float skinWidth = 0.01f;          // Extra padding to avoid clipping
    public float collisionBuffer = 0.001f;   // Small buffer to prevent jitter
    public float chromaticValueOnDrag = 0.25f;  // Intensity when dragging
    public float chromaticValueDefault = 0f;   // Default intensity when not dragging

    Volume globalVol;
    ChromaticAberration chromaticAberration;

    public GameObject HoverSprite;  // Child object with hover effect sprite
    public Color HoverColour = Color.white;   // Colour to tint the hoversprite
    public float HoverMultiply = 1.2f;        // How big to make hover sprite when hovering
    public Color DragColour = Color.white;    // Colour tint when dragging
    public float DragMultiply = 1.5f;         // How big to make the hover sprite when dragging
    public AudioClip HoverSFX;
    public AudioClip EndHoverSFX;
    public AudioClip ClickSFX;
    public AudioClip DropSFX;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        globalVol = GameObject.FindWithTag("GlobalVol").GetComponent<Volume>();

        rb.isKinematic = true;  // Disable physics interactions

        HoverSprite.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
        HoverSprite.GetComponent<SpriteRenderer>().color = HoverColour;

        // Access the Chromatic Aberration component in the Volume
        if (globalVol.profile.TryGet(out chromaticAberration))
            chromaticAberration.intensity.Override(chromaticValueDefault);
    }

    void Update()
    {
        if (Ball.IsBallActive) return;  // Prevent dragging when the ball is active

        HandleDrag();

        if (isDragging)  // Only allow rotation when dragging
        {
            HandleRotation();
        }
    }

    private void HandleDrag()
    {
        Vector3 mousePosition = GetMouseWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(mousePosition);

        // Update hover status
        isHovering = (hit != null && hit.transform == transform);
        if (isHovering && !isDragging)
        {
            HoverEffect();
            offset = transform.position - mousePosition;
        }
        else if (!isHovering && !isDragging)
        {
            EndHover();
        }

        // Begin drag
        if (Input.GetMouseButtonDown(0) && isHovering)
        {
            isDragging = true;
            chromaticAberration?.intensity.Override(chromaticValueOnDrag);
            ClickEffect();
        }

        // End drag
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            DropEffect();
            chromaticAberration?.intensity.Override(chromaticValueDefault);
        }

        // Move the object while dragging
        if (isDragging)
        {
            Vector3 targetPosition = GetMouseWorldPosition() + offset;
            MoveTowardsTarget(targetPosition);
        }
    }

    // Handles rotation using arrow keys
    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.back * rotationSpeed * Time.deltaTime);
        }
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        Vector2 movementDirection = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        Vector3 interpolatedPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dragSpeed);

        if (!CheckForCollisionsAndSlide(movementDirection, interpolatedPosition))
        {
            transform.position = interpolatedPosition;
        }
    }

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
                    Vector2 collisionNormal = hit.normal;
                    Vector2 projectedDirection = Vector3.ProjectOnPlane(finalDirection, collisionNormal).normalized;

                    if (projectedDirection == Vector2.zero)
                    {
                        return true;
                    }

                    remainingDistance = Mathf.Min(remainingDistance, hit.distance - skinWidth);
                    finalDirection = projectedDirection;
                }
            }

            transform.position += (Vector3)finalDirection * (remainingDistance - collisionBuffer);
            return true;
        }

        return false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        return mousePosition;
    }

    private void HoverEffect()
    {
        HoverSprite.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * HoverMultiply;
    }

    private void EndHover()
    {
        HoverSprite.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    private void ClickEffect()
    {
        HoverSprite.transform.localScale = new Vector3(1, 1, 1) * DragMultiply;
        HoverSprite.GetComponent<SpriteRenderer>().color = DragColour;
    }

    private void DropEffect()
    {
        HoverSprite.GetComponent<SpriteRenderer>().color = HoverColour;
        if (isHovering)
        {
            HoverEffect();
            return;
        }
        HoverSprite.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
}
