using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableBoost : MonoBehaviour
{
    public float pushForce = 10f;  // strength of the push
    public bool isWeakBooster = false;
    public bool isGravityFlipper = false;

    [Header("SFX")]
    public AudioClip weakBoostSFX;
    public AudioClip strongBoostSFX;
    public AudioClip gravityFlipSFX;

    [Header("VFX")]
    public Vector2 scrollDirection;
    public float scrollValue = 1.0f;

    private Vector2 scrollVector;

    private void Start()
    {
        if (isGravityFlipper)
        {
            scrollVector = new Vector2(scrollDirection.x * scrollValue, scrollDirection.y * scrollValue);
            GetComponent<SpriteRenderer>().sharedMaterial.SetVector("_ScrollingOffset", scrollVector);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Ball"))
        {
            Rigidbody2D ballRigidbody = other.GetComponent<Rigidbody2D>();

            if (ballRigidbody != null)
            {
                if (isGravityFlipper)
                {
                    // Flip the gravity on the ball
                    ballRigidbody.gravityScale *= -1;

                    scrollVector = new Vector2(scrollDirection.x * scrollValue, scrollDirection.y * scrollValue * ballRigidbody.gravityScale);
                    GetComponent<SpriteRenderer>().sharedMaterial.SetVector("_ScrollingOffset", scrollVector);

                    // Play gravity flip SFX if available
                    if (gravityFlipSFX != null)
                    {
                        SfxManager.Instance.PlaySfx(gravityFlipSFX);
                    }
                }
                else if (!isWeakBooster)
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
