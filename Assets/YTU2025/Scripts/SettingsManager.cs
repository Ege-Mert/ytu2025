using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Audio;
using DG.Tweening;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings References")]
    [SerializeField] private Canvas settingsCanvas;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;
    [SerializeField] private TextMeshProUGUI volumeValueText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetToDefaultsButton;
    
    [Header("Default Settings")]
    [SerializeField] private float defaultMouseSensitivity = 2f;
    [SerializeField] private float defaultMasterVolume = 1f;
    [SerializeField] private KeyCode settingsToggleKey = KeyCode.Escape;
    
    [Header("Audio")]
    [SerializeField] private string uiClickSoundName = "UIClick";
    [SerializeField] private string uiOpenSoundName = "UIOpen";
    [SerializeField] private string uiCloseSoundName = "UIClose";
    
    // PlayerPrefs keys
    private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    
    // Reference to the FirstPersonController
    private FirstPersonController playerController;
    
    // Cached original time scale
    private float originalTimeScale;
    
    // Singleton pattern
    public static SettingsManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize UI elements
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(false);
        }
        
        // Set up button listeners
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }
        
        if (resetToDefaultsButton != null)
        {
            resetToDefaultsButton.onClick.AddListener(ResetToDefaults);
        }
        
        // Set up slider listeners
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        // Load saved settings
        LoadSettings();
    }
    
    void Update()
    {
        // Toggle settings menu with key press
        if (Input.GetKeyDown(settingsToggleKey))
        {
            if (settingsCanvas != null && settingsCanvas.gameObject.activeInHierarchy)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
        
        // Update slider value texts
        UpdateUITexts();
    }
    
    void OnEnable()
    {
        // Subscribe to scene loaded event to find player controller
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        // Unsubscribe from scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Get a reference to the player controller in the new scene
        FindPlayerController();
        
        // Apply settings to the new scene
        ApplySettings();
    }
    
    private void FindPlayerController()
    {
        // Find the player controller in the scene
        playerController = FindObjectOfType<FirstPersonController>();
    }
    
    public void OpenSettings()
    {
        // Cache the original time scale
        originalTimeScale = Time.timeScale;
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Disable player controls
        DisablePlayerControls();
        
        // Show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Show the settings canvas with animation
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(true);
            
            // Add a scale animation
            settingsCanvas.transform.localScale = Vector3.zero;
            settingsCanvas.transform.DOScale(1f, 0.3f).SetUpdate(true);
            
            // Play open sound
            if (!string.IsNullOrEmpty(uiOpenSoundName) && SoundManager.instance != null)
            {
                SoundManager.instance.Play(uiOpenSoundName);
            }
        }
        
        // Refresh UI elements
        RefreshUI();
    }
    
    public void CloseSettings()
    {
        // Save settings
        SaveSettings();
        
        // Animate closing
        if (settingsCanvas != null)
        {
            // Play close sound
            if (!string.IsNullOrEmpty(uiCloseSoundName) && SoundManager.instance != null)
            {
                SoundManager.instance.Play(uiCloseSoundName);
            }
            
            // Scale down animation
            settingsCanvas.transform.DOScale(0f, 0.2f).SetUpdate(true).OnComplete(() => {
                settingsCanvas.gameObject.SetActive(false);
                
                // Restore time scale
                Time.timeScale = originalTimeScale;
                
                // Re-enable player controls
                EnablePlayerControls();
                
                // Check if we need to lock the cursor again
                if (playerController != null && playerController.lockCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            });
        }
        else
        {
            // Restore time scale
            Time.timeScale = originalTimeScale;
            
            // Re-enable player controls
            EnablePlayerControls();
            
            // Check if we need to lock the cursor again
            if (playerController != null && playerController.lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    public void ResetToDefaults()
    {
        // Play UI click sound
        if (!string.IsNullOrEmpty(uiClickSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(uiClickSoundName);
        }
        
        // Reset to default values
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = defaultMouseSensitivity;
        }
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = defaultMasterVolume;
        }
        
        // Apply the settings
        ApplySettings();
        
        // Save the settings
        SaveSettings();
    }
    
    private void LoadSettings()
    {
        // Load saved values or use defaults
        float savedSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, defaultMouseSensitivity);
        float savedVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume);
        
        // Update sliders with saved values
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = savedSensitivity;
        }
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = savedVolume;
        }
        
        // Apply the settings
        ApplySettings();
    }
    
    private void SaveSettings()
    {
        // Get current values
        float currentSensitivity = mouseSensitivitySlider != null ? mouseSensitivitySlider.value : defaultMouseSensitivity;
        float currentVolume = masterVolumeSlider != null ? masterVolumeSlider.value : defaultMasterVolume;
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, currentSensitivity);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, currentVolume);
        PlayerPrefs.Save();
    }
    
    private void ApplySettings()
    {
        // Apply mouse sensitivity to player controller
        if (playerController != null)
        {
            float sensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, defaultMouseSensitivity);
            playerController.mouseSensitivity = sensitivity;
        }
        
        // Apply master volume to all audio sources
        float volume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume);
        AudioListener.volume = volume;
        
        // Also apply to SoundManager if it exists
        if (SoundManager.instance != null)
        {
            SoundManager.Sound[] sounds = SoundManager.instance.sounds;
            if (sounds != null)
            {
                foreach (SoundManager.Sound sound in sounds)
                {
                    if (sound.source != null)
                    {
                        // Maintain the relative volume between sounds
                        sound.source.volume = sound.volume * volume;
                    }
                }
            }
        }
    }
    
    private void RefreshUI()
    {
        // Update mouse sensitivity slider
        if (mouseSensitivitySlider != null)
        {
            float savedSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, defaultMouseSensitivity);
            mouseSensitivitySlider.value = savedSensitivity;
        }
        
        // Update master volume slider
        if (masterVolumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume);
            masterVolumeSlider.value = savedVolume;
        }
        
        // Update text values
        UpdateUITexts();
    }
    
    private void UpdateUITexts()
    {
        // Update sensitivity value text
        if (sensitivityValueText != null && mouseSensitivitySlider != null)
        {
            sensitivityValueText.text = mouseSensitivitySlider.value.ToString("F1");
        }
        
        // Update volume value text
        if (volumeValueText != null && masterVolumeSlider != null)
        {
            // Display as percentage
            volumeValueText.text = (masterVolumeSlider.value * 100).ToString("F0") + "%";
        }
    }
    
    private void OnMouseSensitivityChanged(float value)
    {
        // Play UI click sound
        if (!string.IsNullOrEmpty(uiClickSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(uiClickSoundName);
        }
        
        // Apply the setting immediately
        if (playerController != null)
        {
            playerController.mouseSensitivity = value;
        }
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        // Play UI click sound
        if (!string.IsNullOrEmpty(uiClickSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(uiClickSoundName);
        }
        
        // Apply the volume setting immediately
        AudioListener.volume = value;
        
        // Also apply to SoundManager if it exists
        if (SoundManager.instance != null)
        {
            SoundManager.Sound[] sounds = SoundManager.instance.sounds;
            if (sounds != null)
            {
                foreach (SoundManager.Sound sound in sounds)
                {
                    if (sound.source != null)
                    {
                        // Maintain the relative volume between sounds
                        sound.source.volume = sound.volume * value;
                    }
                }
            }
        }
    }
    
    private void DisablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.playerCanMove = false;
            playerController.cameraCanMove = false;
        }
    }
    
    private void EnablePlayerControls()
    {
        if (playerController != null && Time.timeScale > 0)
        {
            playerController.playerCanMove = true;
            playerController.cameraCanMove = true;
        }
    }
}
