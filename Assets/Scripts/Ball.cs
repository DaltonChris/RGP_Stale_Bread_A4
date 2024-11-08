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

    [Header("Shoot Trail")]
    public GameObject shootTrail;
    private GameObject existingTrail;
    public Gradient prevTrailGradient;

    [Header("Particles")]
    public GameObject hitParticles;
    public GameObject destroyParticles;
    public GameObject firedParticles;


    [Header("SFX")]
    public AudioClip shotSFX;
    public AudioClip[] hitSFX;
    public AudioClip[] shatterSFX;
    public AudioClip victorySFX;

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
    float resetVector = 0.045f;
    float lowVelTimer = 0f; // Timer for low velocity
    float lowVelDuration = 1.95f;
    bool hasWon;

    [Header("VFX")]
    public float chromaticValueOnHit = 0f;  // Intensity when dragging
    float chromaticValueDefault = 0f;   // Default intensity when not dragging
    ChromaticAberration chromaticAberration;
    LensDistortion lensDistortion;
    // Coroutine to lerp the LensDistortion intensity
    bool isLensDistortionLerping = false;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsBallActive = true;  // Sets ball as active

        SfxManager.Instance.PlaySfx(shotSFX);

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

        // Access the Chromatic Aberration component in the Volume
        if (globalVol.profile.TryGet(out chromaticAberration))
            chromaticAberration.intensity.Override(chromaticValueDefault);

        if (globalVol.profile.TryGet(out lensDistortion))
            lensDistortion.intensity.Override(chromaticValueDefault);
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
                    OnBecameInvisible(); // Start lerp coroutine
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
        int randSFX = UnityEngine.Random.Range(0, shatterSFX.Length);
        SfxManager.Instance.PlaySfx(shatterSFX[randSFX]);

        if(existingTrail != null)
        {
            Destroy(existingTrail);//If it is, destroy it
        }
        if(shootTrail != null)
        {
            shootTrail.gameObject.tag = "ShootTrail";//Add tag to new one so it can be found by the next ball
            shootTrail.transform.parent = null;//Unparent so it doesn't get destroyed
        }

        // Notify the shooter to reset when the ball leaves the screen
        OnBallReset?.Invoke();
        
        IsBallActive = false; // Sets ball as inactive

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
            //int randSFX = UnityEngine.Random.Range(0, hitSFX.Length);     // Get random hit sfx clip- disabled in favor of velocity based selection
            float velocityVolume = rb.velocity.magnitude/15.0f;             // Map the velocity to a value roughly between 0 - 1 to get the volume
            velocityVolume = Mathf.Clamp01(velocityVolume);                 // Prevent the volume from going over 1 and potentially clipping
            int sfxNumber = Mathf.FloorToInt(velocityVolume*10);            // Select an SFX clip based on the velocity, because the clips are organised by pitch
            SfxManager.Instance.PlaySfx(hitSFX[sfxNumber], velocityVolume); // Play the relevant clip
            
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Flag"))
        {
            CompleteLevel(collision.gameObject);
        }

    }
    /// <summary>
    /// Disable ball interactions, lerp for volume effects, destory once complete
    /// </summary>
    /// <returns></returns>
    public IEnumerator DestroyAfterLerp()
    {
        DisabledInteractions();
        if (!hasWon)
        {
            yield return StartCoroutine(BallDestoryedLerpDefault());
            chromaticAberration?.intensity.Override(chromaticValueDefault);
            Destroy(gameObject);
        }
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
        float startChromatic = chromaticValueOnHit;

        while (elapsedTime < lerpDuration)
        {
            BasicMultiChannelPerlin.m_FrequencyGain = shakeActiveValue;
            // Calculate lerped values
            float lerpSplitToning = Mathf.Lerp(startSplitToning, splitDefValue, elapsedTime / lerpDuration);
            float lerpDepthOfField = Mathf.Lerp(startDepthOfField, dofDefValue, elapsedTime / lerpDuration);
            float lerpChromatic = Mathf.Lerp(startChromatic, chromaticValueDefault, elapsedTime / lerpDuration);

            // Apply the lerped values
            if (SplitToning != null)
            {
                SplitToning.balance.Override(lerpSplitToning);
            }
            if (DepthOfField != null)
            {
                DepthOfField.focalLength.Override(lerpDepthOfField);
            }
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.Override(lerpChromatic);
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
        if (chromaticAberration.intensity != 0f)
        {
            chromaticAberration.intensity.Override(0);
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
        chromaticAberration?.intensity.Override(chromaticValueOnHit);
    }

    void CompleteLevel(GameObject flag)
    {
        hasWon = true;
        ResetManager.Instance.isWinning = true;
        DisabledInteractions();
        DepthOfField.focalLength.Override(dofDefValue);
        StartCoroutine(WinParticles(flag));
        Debug.Log("Level Completed");
        // SfxManager.Instance.PlaySfx(victorySFX);
    }

    IEnumerator WinParticles(GameObject flag)
    {
        // Array of particle prefabs
        GameObject[] particlePrefabs = { firedParticles, hitParticles, destroyParticles };
        float duration = 1.25f;        // Total duration for fireworks
        float interval = 0.05f;       // Time between particle spawns
        float halfwayPoint = duration / 2f; // Halfway point for lens distortion
        

        float elapsedTime = 0f;      // Track elapsed time
        Vector3 particleScale = new Vector3(4, 4, 1);  // Desired scale for particles
        
        Time.timeScale = 0.85f;

        // Start particle fireworks
        while (elapsedTime < duration)
        {
            foreach (var particle in particlePrefabs)
            {
                GameObject instantiatedParticle = Instantiate(particle, flag.transform.position, Quaternion.identity);
                instantiatedParticle.transform.localScale = particleScale;
                yield return new WaitForSeconds(interval);
            }

            elapsedTime += interval * 2;

            // When halfway point is reached, start the lens distortion lerp
            if (elapsedTime >= halfwayPoint && lensDistortion != null && !isLensDistortionLerping)
            {
                StartCoroutine(LerpLensDistortion(-1f, duration));
                // After particles are done, activate victory UI
                ResetManager.Instance.victoryUI.SetActive(true);
            }
        }
        Time.timeScale = 0.5f;
    }

    IEnumerator LerpLensDistortion(float targetIntensity, float duration)
    {
        isLensDistortionLerping = true; // Flag to prevent multiple lerps
        float startIntensity = lensDistortion.intensity.value;
        float elapsed = 0f;
        float startScale = lensDistortion.scale.value;
        float targetScale = 1.5f;

        while (elapsed < duration)
        {
            float lerpedIntensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
            float lerpedScale = Mathf.Lerp(startScale, targetScale, elapsed / duration);
            lensDistortion.intensity.Override(lerpedIntensity);
            lensDistortion.scale.Override(lerpedScale);
            elapsed += Time.deltaTime;
            yield return null;
        }

        lensDistortion.intensity.Override(targetIntensity);
        lensDistortion.scale.Override(targetScale);
        isLensDistortionLerping = false;
    }

}
