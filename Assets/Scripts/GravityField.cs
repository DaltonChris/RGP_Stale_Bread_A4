using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    public float gravityStrength = 5f;  // Strength of the gravitational pull
    public float pullRadius = 3f;       // Radius where the gravity starts affecting the ball
    public ParticleSystem orbitingParticles; 
    public float orbitSpeed = 50f;      // speed of orbiting of particles go make it look like there is a orbit

    private void FixedUpdate()
    {
        // Detect all objects within the pull radius
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, pullRadius);

        foreach (Collider2D collider in objectsInRange)
        {
            // Check if the object is the ball
            if (collider.CompareTag("Ball"))
            {
                Rigidbody2D ballRigidbody = collider.GetComponent<Rigidbody2D>();
                if (ballRigidbody != null)
                {
                    // Calculate direction and distance to the center of the gravity field
                    Vector2 direction = (Vector2)transform.position - ballRigidbody.position;
                    float distance = direction.magnitude;

                    // Apply gravitational pull inversely proportional to distance squared
                    Vector2 gravityForce = direction.normalized * gravityStrength / Mathf.Max(distance * distance, 0.1f); // Prevent division by zero
                    ballRigidbody.AddForce(gravityForce);
                }
            }
        }

        // Rotate the particle system around the object
        if (orbitingParticles != null)
        {
            orbitingParticles.transform.RotateAround(transform.position, Vector3.forward, orbitSpeed * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the pull radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}
