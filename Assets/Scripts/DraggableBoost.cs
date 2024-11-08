using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableBoost : MonoBehaviour
{
    public float pushForce = 10f;  // strength of the push
    public bool isWeakBooster = false;

    [Header("SFX")]
    public AudioClip weakBoostSFX;
    public AudioClip strongBoostSFX;

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Ball"))
        {
            Rigidbody2D ballRigidbody = other.GetComponent<Rigidbody2D>();

            if (ballRigidbody != null)
            {
                if (!isWeakBooster)
                {
                    // Calculate the force direction 
                    Vector2 forceDirection = -transform.right * pushForce;

                    //Cancel out the current velocity on the ball
                    ballRigidbody.velocity = new Vector2(0, 0);
                    // Apply force 
                    ballRigidbody.AddForce(forceDirection, ForceMode2D.Impulse);
                    SfxManager.Instance.PlaySfx(strongBoostSFX);
                }
                else
                {
                    // Calculate the force direction 
                    Vector2 forceDirection = -transform.right * pushForce;

                    // Apply force 
                    ballRigidbody.AddForce(forceDirection, ForceMode2D.Impulse);
                    SfxManager.Instance.PlaySfx(weakBoostSFX);
                }
            }
        }
    }

}
