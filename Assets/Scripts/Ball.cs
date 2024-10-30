using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Ball : MonoBehaviour
{
    public static bool IsBallActive = false;
    public event Action OnBallReset;  // Event to notify when the ball resets

    private Rigidbody2D rb;
    public GameObject shootTrail;
    private GameObject existingTrail;

    Volume globalVol;
    SplitToning SplitToning;
    float splitDefValue = -100f;
    float splitHitValue = 100f;
    DepthOfField DepthOfField;
    float dofDefValue = 1f;
    float dofHitValue = 300f;
    public float lerpDuration = 2.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsBallActive = true;  // Sets ball as active

        globalVol = GameObject.FindWithTag("GlobalVol").GetComponent<Volume>();

        existingTrail = GameObject.FindWithTag("ShootTrail");//Check if a trail is already in the scene
        if(existingTrail != null)
        {
            existingTrail.GetComponent<TrailRenderer>().startColor = Color.black;
            existingTrail.GetComponent<TrailRenderer>().endColor = Color.black;
        }

        // Access components in the Volume
        if (globalVol.profile.TryGet(out SplitToning))
            SplitToning.balance.Override(splitDefValue);

        if (globalVol.profile.TryGet(out DepthOfField))
            DepthOfField.focalLength.Override(dofDefValue);
    }

    private void OnBecameInvisible()
    {
        // Notify the shooter to reset when the ball leaves the screen
        OnBallReset?.Invoke();
        IsBallActive = false; // Sets ball as inactive

        if(existingTrail != null)
        {
            Destroy(existingTrail);//If it is, destroy it
        }
        shootTrail.gameObject.tag = "ShootTrail";//Add tag to new one so it can be found by the next ball
        shootTrail.transform.parent = null;//Unparent so it doesn't get destroyed

        if(SplitToning != null)
        {
            SplitToning.balance.Override(splitHitValue);
        }
        if(DepthOfField != null)
        {
            DepthOfField.focalLength.Override(dofHitValue);
        }
        StartCoroutine(DestroyAfterLerp()); // Start lerp coroutine
        //Destroy(gameObject);
    }

    // Collison check for objects that need to be avoided
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Destroy"))
        {
            OnBecameInvisible(); // Call Method to reset ball
        }
    }

    IEnumerator DestroyAfterLerp()
    {
        DisabledInteractions();
        yield return StartCoroutine(BallDestoryedLerpDefault());
        Destroy(gameObject);
    }
    IEnumerator BallDestoryedLerpDefault()
    {
        float elapsedTime = 0f;

        // Initial values at the start of the lerp
        float startSplitToning = splitHitValue;
        float startDepthOfField = dofHitValue;

        while (elapsedTime < lerpDuration)
        {
            // Calculate lerped values
            float lerpSplitToning = Mathf.Lerp(startSplitToning, splitDefValue, elapsedTime / lerpDuration);
            float lerpDepthOfField = Mathf.Lerp(startDepthOfField, dofDefValue, elapsedTime / lerpDuration);

            // Apply the lerped values
            if (SplitToning != null)
            {
                SplitToning.balance.Override(lerpSplitToning);
            }
            if (DepthOfField != null)
            {
                DepthOfField.focalLength.Override(lerpDepthOfField);
            }

            elapsedTime += Time.deltaTime; // Update elapsed time
            yield return null; // Wait for the next frame
        }

        // Ensure final values are exactly at the default values after the lerp is complete
        if (SplitToning != null)
        {
            SplitToning.balance.Override(splitDefValue);
        }
        if (DepthOfField != null)
        {
            DepthOfField.focalLength.Override(dofDefValue);
        }
    }

    void DisabledInteractions()
    {
        gameObject.GetComponent<Light2D>().enabled = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
    }
}
