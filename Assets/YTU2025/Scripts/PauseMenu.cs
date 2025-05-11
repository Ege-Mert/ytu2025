using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu Components")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;
    [SerializeField] private TextMeshProUGUI volumeValueText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button mainMenuButton; // Button to return to main menu
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Scene name for the main menu

    [Header("Settings")]
    [SerializeField] private float defaultSensitivity = 2f;
    [SerializeField] private float defaultVolume = 1f;
    [SerializeField] private KeyCode menuToggleKey = KeyCode.Escape;
    
    // PlayerPrefs keys
    private const string SENSITIVITY_KEY = "MouseSensitivity";
    private const string VOLUME_KEY = "MasterVolume";
    
    // Reference to the FirstPersonController
    private FirstPersonController playerController;
    
    // Singleton instance
    public static PauseMenu Instance { get; private set; }
    
    // Flag to track if we're handling input this frame
    private bool handlingKeyThisFrame = false;
    
    // Flag to track if we're in the main menu
    private bool isInMainMenu = false;
    
    // Debug flag to log issues
    [SerializeField] private bool debugMode = false;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initial setup
            InitializeUI();
            HideMenu(false); // Don't save settings on initial hide
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Find player controller in the scene
        FindPlayerController();
        
        // Load saved settings
        LoadSettings();
    }
    
    private void Update()
    {
        // Don't handle ESC key in main menu
        if (isInMainMenu)
        {
            // Still update UI texts
            UpdateUITexts();
            return;
        }
        
        // Reset handling flag at the beginning of the frame
        handlingKeyThisFrame = false;
        
        // Check for escape key press
        if (Input.GetKeyDown(menuToggleKey))
        {
            handlingKeyThisFrame = true;
            
            if (debugMode)
            {
                Debug.Log("ESC key detected, menu active: " + IsMenuActive());
            }
            
            // Toggle menu state
            if (IsMenuActive())
            {
                HideMenu(true); // True to save settings
            }
            else
            {
                TutorialSystem activeTutorial = FindObjectOfType<TutorialSystem>();
                bool tutorialActive = activeTutorial != null && Time.timeScale == 0f;
                
                // Only show menu if no tutorial is active
                if (!tutorialActive)
                {
                    ShowMenu();
                }
            }
        }
        
        // Update UI texts
        UpdateUITexts();
    }
    
    private void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if we're in main menu
        isInMainMenu = scene.name == mainMenuSceneName;
        
        // Find the player controller in the new scene
        FindPlayerController();
        
        // Apply settings
        ApplySettings();
    }
    
    private void InitializeUI()
    {
        // Set up slider listeners
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        // Set up button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() => HideMenu(true));
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }
    
    private void FindPlayerController()
    {
        playerController = FindObjectOfType<FirstPersonController>();
    }
    
    public void ShowMenu()
    {
        if (debugMode)
        {
            Debug.Log("ShowMenu called. Current timeScale: " + Time.timeScale);
        }
        
        // Don't pause if in main menu
        if (!isInMainMenu)
        {
            // Pause the game
            Time.timeScale = 0f;
            
            // Disable player controls
            if (playerController != null)
            {
                playerController.playerCanMove = false;
                playerController.cameraCanMove = false;
            }
        }
        
        // Enable cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Show menu
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(true);
        }
        
        // Refresh UI
        RefreshUI();
    }
    
    public void HideMenu(bool saveSettings)
    {
        if (debugMode)
        {
            Debug.Log("HideMenu called. Current timeScale: " + Time.timeScale);
        }
        
        // Hide menu canvas
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(false);
        }
        
        // Save settings if requested
        if (saveSettings)
        {
            SaveSettings();
        }
        
        // Check if a tutorial is active before resuming
        TutorialSystem activeTutorial = FindObjectOfType<TutorialSystem>();
        bool tutorialActive = activeTutorial != null && activeTutorial.IsTutorialActive();
        
        if (debugMode)
        {
            Debug.Log("Tutorial active: " + tutorialActive);
        }
        
        // Resume game if tutorial is not active
        if (!tutorialActive)
        {
            // Always set timeScale to 1 when closing menu
            Time.timeScale = 1f;
            
            if (debugMode)
            {
                Debug.Log("Setting timeScale to 1");
            }
            
            // Re-enable player controls
            if (playerController != null)
            {
                playerController.playerCanMove = true;
                playerController.cameraCanMove = true;
                
                // Lock cursor if needed
                if (playerController.lockCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }
    
    // Public method to check if menu is active
    public bool IsMenuActive()
    {
        return menuCanvas != null && menuCanvas.gameObject.activeInHierarchy;
    }
    
    private void LoadSettings()
    {
        // Load saved values or use defaults
        float savedSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, defaultSensitivity);
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, defaultVolume);
        
        // Update sliders
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
        }
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = savedVolume;
        }
        
        // Apply settings
        ApplySettings();
    }
    
    private void SaveSettings()
    {
        // Get current values
        float currentSensitivity = sensitivitySlider != null ? sensitivitySlider.value : defaultSensitivity;
        float currentVolume = masterVolumeSlider != null ? masterVolumeSlider.value : defaultVolume;
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, currentSensitivity);
        PlayerPrefs.SetFloat(VOLUME_KEY, currentVolume);
        PlayerPrefs.Save();
    }
    
    private void ApplySettings()
    {
        // Apply mouse sensitivity
        if (playerController != null)
        {
            float sensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, defaultSensitivity);
            playerController.mouseSensitivity = sensitivity;
        }
        
        // Apply volume to audio
        float volume = PlayerPrefs.GetFloat(VOLUME_KEY, defaultVolume);
        AudioListener.volume = volume;
    }
    
    private void RefreshUI()
    {
        // Update sensitivity slider
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat(SENSITIVITY_KEY, defaultSensitivity);
        }
        
        // Update volume slider
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat(VOLUME_KEY, defaultVolume);
        }
    }
    
    private void UpdateUITexts()
    {
        // Update sensitivity text
        if (sensitivityValueText != null && sensitivitySlider != null)
        {
            sensitivityValueText.text = sensitivitySlider.value.ToString("F1");
        }
        
        // Update volume text
        if (volumeValueText != null && masterVolumeSlider != null)
        {
            volumeValueText.text = (masterVolumeSlider.value * 100).ToString("F0") + "%";
        }
    }
    
    private void OnSensitivityChanged(float value)
    {
        // Apply sensitivity immediately
        if (playerController != null)
        {
            playerController.mouseSensitivity = value;
        }
    }
    
    private void OnVolumeChanged(float value)
    {
        // Apply volume immediately
        AudioListener.volume = value;
    }
    
    private void QuitGame()
    {
        // Save settings before quitting
        SaveSettings();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void ReturnToMainMenu()
    {
        // Save settings before returning to main menu
        SaveSettings();
        
        // Restore time scale to ensure proper scene transition
        Time.timeScale = 1f;
        
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
