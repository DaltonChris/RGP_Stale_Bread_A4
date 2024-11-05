using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private float holdTime = 2f;  // How long R must be held to reset the scene
    private float holdTimer = 0f;
    public GameObject resetUI;
    public GameObject victoryUI;
    Animator resetAnimator;
    Animator victoryAnimator;
    public static ResetManager Instance;

    int sceneIndex = 1;

    private void Awake()
    {
        // Ensure a single instance
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find and cache the resetUI at the start, even if disabled
        resetUI = GameObject.FindWithTag("ResetUI");
        if (resetUI != null)
            resetAnimator = resetUI.GetComponent<Animator>();
            resetUI.SetActive(false); // Ensure it's disabled initially


        // Find and cache the victoryUI at the start, even if disabled
        victoryUI = GameObject.FindWithTag("winUI");
        if (victoryUI != null)
            victoryAnimator = victoryUI.GetComponent<Animator>();
            victoryUI.SetActive(false); // Ensure it's disabled initially
    }
    private void Start()
    {
        // Store the original positions of all draggable objects
        foreach (var obj in FindObjectsOfType<Draggable>())
        {
            originalPositions[obj.transform] = obj.transform.position;
        }
    }

    private void Update()
    {
        // Handle both tapping and holding R
        if (Input.GetKey(KeyCode.R))
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                ResetScene();  // Reset scene if R is held long enough
                holdTimer = 0f;  // Reset the timer
            }
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            if (holdTimer < holdTime)
            {
                ResetBall();  // Tap R to reset the ball
            }
            holdTimer = 0f;  // Reset the timer if R is released early
        }
        if(victoryUI.activeInHierarchy && Input.GetKey(KeyCode.Space)) // change scene
        {
            //SceneManager.LoadScene(1); // reload
            sceneIndex++;
            SceneManager.LoadScene(sceneIndex); // next scene
        }
    }

    void ResetBall()
    {
        if (resetUI != null)
            if (resetAnimator != null)
                StartCoroutine(ResetFadeOut());
        // Find the active Ball instance and initiate the lerp reset
        Ball activeBall = FindObjectOfType<Ball>();
        if (activeBall != null)
        {
            StartCoroutine(activeBall.DestroyAfterLerp());
        }
    }


    private void ResetScene()
    {
        if (resetUI != null)
            if (resetUI.activeInHierarchy) resetUI.SetActive(false);
        // Reset all draggable objects to their original positions
        foreach (var entry in originalPositions)
        {
            entry.Key.position = entry.Value;
        }

        ResetBall();  // Also reset the ball during scene reset
    }

    public IEnumerator ResetFadeOut()
    {
        resetAnimator.SetTrigger("Fade");
        yield return new WaitForSeconds(1f);
        resetUI.SetActive(false);
    }
}
