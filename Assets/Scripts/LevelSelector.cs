using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public GameObject LevelCanvasPrefab; // The prefab for the entire level canvas
    public GameObject LevelButtonPrefab; // The prefab for each level button
    public int _TotalLevels = 27;         // Total number of levels
    public int _LevelsPerMenu = 9;        // Levels per menu
    public List<GameObject> _Menus = new List<GameObject>(); // Store references to the instantiated menus

    public Color _Panel_2_Colour;
    public Color _Panel_3_Colour;

    // Start is called before the first frame update
    void Start()
    {
        GenerateLevelCanvases();
        ShowMenu(0); // Show the first menu by default
    }

    // Generate canvases and populate them with level buttons
    private void GenerateLevelCanvases()
    {
        int menuCount = Mathf.CeilToInt((float)_TotalLevels / _LevelsPerMenu);
        int levelCounter = 1;

        for (int i = 0; i < menuCount; i++)
        {
            // Instantiate a new canvas from the prefab
            GameObject canvas = Instantiate(LevelCanvasPrefab, transform);
            canvas.name = "MenuCanvas" + (i + 1);
            _Menus.Add(canvas);
            // canvas in the root of the scene
            canvas.transform.SetParent(null);

            // Find the Image component on the Canvas
            Image canvasImage = canvas.GetComponent<Image>();
            if (canvasImage != null)// Change canvas background color for specific menus
            {
                if (i == 1) // Second menu (index 1)
                {
                    canvasImage.color = _Panel_2_Colour;
                }
                else if (i == 2) // Third menu (index 2)
                {
                    canvasImage.color = _Panel_3_Colour;
                }
            }
            else
            {
                Debug.LogWarning("No Image component on the Canvas prefab!");
            }

            // Find the GridLayoutGroup in the prefab
            GridLayoutGroup grid = canvas.GetComponentInChildren<GridLayoutGroup>();
            if (grid == null)
            {
                Debug.LogError("No GridLayoutGroup found in the canvas prefab!");
                return;
            }

            // Populate the grid with buttons
            for (int j = 0; j < _LevelsPerMenu; j++)
            {
                if (levelCounter > _TotalLevels) break;

                // Instantiate a button
                GameObject button = Instantiate(LevelButtonPrefab, grid.transform);
                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

                if (buttonText != null)
                {
                    buttonText.text = "Level " + levelCounter; // Set button text
                }

                int levelIndex = levelCounter; // Capture the current level index for the button click
                button.GetComponent<Button>().onClick.AddListener(() => OnLevelButtonClicked(levelIndex));

                levelCounter++;
            }

            canvas.SetActive(false); // Hide all menus
        }
    }


    // Show a specific menu
    public void ShowMenu(int menuIndex)
    {
        if (menuIndex == -1)
        {
            // Find the currently active menu
            int currentMenuIndex = -1;
            for (int i = 0; i < _Menus.Count; i++)
            {
                if (_Menus[i].activeSelf)
                {
                    currentMenuIndex = i;
                    break;
                }
            }

            // Calculate the next menu index
            menuIndex = (currentMenuIndex + 1) % _Menus.Count;
        }

        // Activate the selected menu and deactivate others
        for (int i = 0; i < _Menus.Count; i++)
        {
            _Menus[i].SetActive(i == menuIndex);
        }
    }

    // Handle level button click
    private void OnLevelButtonClicked(int levelIndex)
    {
        Debug.Log("Level " + levelIndex + " selected!");


        if (levelIndex+1 < SceneManager.sceneCountInBuildSettings)
        {
            int levelToLoad = levelIndex + 1;
            SceneManager.LoadScene(levelToLoad); // Load the scene by build index
        }
        else
        {
            Debug.LogError("Level " + levelIndex + " does not exist in the Build Settings.");
        }

    }
}
