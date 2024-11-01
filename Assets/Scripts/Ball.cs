using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;

public class Ball : MonoBehaviour
{
    public static bool IsBallActive = false;
    public event Action OnBallReset;  // Event to notify when the ball resets
    public GameObject BackObj;

    private Rigidbody2D rb;
    private CircleCollider2D col2d;
    public GameObject shootTrail;
    private GameObject existingTrail;
    public Gradient prevTrailGradient;

    public GameObject hitParticles;
    public GameObject destroyParticles;
    public GameObject firedParticles;

    // Volume & componets values
    Volume globalVol;
    SplitToning SplitToning;
    float splitDefValue = -100f;
    float splitHitValue = 100f;
    DepthOfField DepthOfField;
    float dofDefValue = 1f;
    float dofHitValue = 300f;
    public float lerpDuration = 2.25f;

    //cinemachine
    CinemachineVirtualCamera cinCam;
    CinemachineBasicMultiChannelPerlin BasicMultiChannelPerlin;
    float shakeActiveValue = 1.2f;

    GameObject resetUI;
    float resetVector = 0.01f;
    float lowVelTimer = 0f; // Timer for low velocity
    float lowVelDuration = 1.95f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsBallActive = true;  // Sets ball as active

        cinCam = GameObject.FindWithTag("CineCam").GetComponent<CinemachineVirtualCamera>();
        if (cinCam != null)
            BasicMultiChannelPerlin = cinCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        BasicMultiChannelPerlin.m_FrequencyGain = 0f;

        resetUI = ResetManager.Instance.resetUI;

        globalVol = GameObject.FindWithTag("GlobalVol").GetComponent<Volume>();

        existingTrail = GameObject.FindWithTag("ShootTrail");//Check if a trail is already in the scene
        if(existingTrail != null)
        {
            TrailRenderer trailComponent = existingTrail.GetComponent<TrailRenderer>();
            trailComponent.colorGradient = prevTrailGradient;
            trailComponent.sortingOrder = -1;
        }

        // Access components in the Volume
        if (globalVol.profile.TryGet(out SplitToning))
            SplitToning.balance.Override(splitDefValue);

        if (globalVol.profile.TryGet(out DepthOfField))
            DepthOfField.focalLength.Override(dofDefValue);
    }
    private void Update()
    {
        // Check if ball is below velocity and active
        if (rb.velocity.magnitude < resetVector && IsBallActive)
        {
            // Increment the low velocity timer
            lowVelTimer += Time.deltaTime;

            if (lowVelTimer >= lowVelDuration && resetUI != null)
            {
                if (this.gameObject.activeInHierarchy)
                    StartCoroutine(DestroyAfterLerp()); // Start lerp coroutine
                resetUI.SetActive(true);
            }
        }
        else
        {
            // Reset the timer
            lowVelTimer = 0f;
        }
    }

    private void OnBecameInvisible()
    {
        Instantiate(destroyParticles, transform.position, Quaternion.identity);

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
        
        if (this.gameObject.activeInHierarchy)
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
        else
        {
            Instantiate(hitParticles, transform.position, Quaternion.identity);
        }
    }
    /// <summary>
    /// Disable ball interactions, lerp for volume effects, destory once complete
    /// </summary>
    /// <returns></returns>
    public IEnumerator DestroyAfterLerp()
    {
        DisabledInteractions();
        yield return StartCoroutine(BallDestoryedLerpDefault());
        Destroy(gameObject);
    }
    /// <summary>
    /// Lerps between Def and Hit values for effect on Global VOl
    /// </summary>
    /// <returns></returns>
    IEnumerator BallDestoryedLerpDefault()
    {
        float elapsedTime = 0f;

        // Initial values at the start of the lerp
        float startSplitToning = splitHitValue;
        float startDepthOfField = dofHitValue;

        while (elapsedTime < lerpDuration)
        {
            BasicMultiChannelPerlin.m_FrequencyGain = shakeActiveValue;
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
        if (BasicMultiChannelPerlin.m_FrequencyGain != 0f) //Rest multichanperlin for cinemachine
        {
            BasicMultiChannelPerlin.m_FrequencyGain = 0f;
        }
    }
    /// <summary>
    /// Method to disable visual componets and collisons to be used delaying obj destroy
    /// </summary>
    void DisabledInteractions()
    {
        gameObject.GetComponent<Light2D>().enabled = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        BackObj.SetActive(false);
    }
}
