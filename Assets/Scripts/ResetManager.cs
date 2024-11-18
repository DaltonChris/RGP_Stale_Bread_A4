using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class ResetManager : MonoBehaviour
{
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private float holdTime = 2f;  // How long R must be held to reset the scene
    private float holdTimer = 0f;
    public GameObject resetUI;
    public GameObject victoryUI;
    public AudioClip resetSFX;
    public AudioSource longResetPlayer;
    private bool justReset;
    public Slider resetIndicator;
    Animator resetAnimator;
    Animator victoryAnimator;
    public static ResetManager Instance;
    public bool isWinning;
    public GameObject levelTextObj;
    TextMeshProUGUI levelText;


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
        
        longResetPlayer = GetComponent<AudioSource>();

        levelTextObj = GameObject.FindWithTag("LevelText");
        if (levelTextObj != null)
            levelText = levelTextObj.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (levelText != null)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                levelText.text = "Start Menu";
            }
            else
            {
                levelText.text = $"Level: {SceneManager.GetActiveScene().buildIndex}";
            }
        }
        // Store the original positions of all draggable objects
        foreach (var obj in FindObjectsOfType<Draggable>())
        {
            originalPositions[obj.transform] = obj.transform.position;
        }
    }

    private void Update()
    {
        if (PauseMenu.IsPaused) return;

        // Handle both tapping and holding R
        if (Input.GetKey(KeyCode.R))
        {
            if (justReset == false)
            {
                if (holdTimer == 0)
                {
                    if (longResetPlayer != null){longResetPlayer.Play();}
                }
                holdTimer += Time.deltaTime;
            }

            if (holdTimer >= holdTime)
            {
                ResetScene();  // Reset scene if R is held long enough
                holdTimer = 0f;  // Reset the timer
                justReset = true;
            }
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            if (holdTimer < holdTime)
            {
                ResetBall();  // Tap R to reset the ball
            }
            holdTimer = 0f;  // Reset the timer if R is released early
            if (longResetPlayer != null){longResetPlayer.Stop();}
            justReset = false;
        }

        if (resetIndicator != null){resetIndicator.value = holdTimer;}

        if(victoryUI.activeInHierarchy && Input.GetKey(KeyCode.Space)) // change scene
        {
            LoadNextScene();
        }

        
        if(Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex); // Load the next scene
        }
        else
        {
            SceneManager.LoadScene(0); // Optionally loop back to the first scene
        }
        Time.timeScale = 1.0f; //Set the time scale back to normal
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
