using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableBoost : MonoBehaviour
{
    public float pushForce = 10f;  // strength of the push

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Ball"))
        {
            Rigidbody2D ballRigidbody = other.GetComponent<Rigidbody2D>();

            if (ballRigidbody != null)
            {
                // Calculate the force direction 
                Vector2 forceDirection = -transform.right * pushForce;

                // Apply force 
                ballRigidbody.AddForce(forceDirection, ForceMode2D.Impulse);
            }
        }
    }

}
