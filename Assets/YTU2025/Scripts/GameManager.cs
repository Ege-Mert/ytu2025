using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalRabbits = 5;
    
    [Header("Sound Settings")]
    [SerializeField] private bool useSoundManager = true;
    
    // Track kills (optional, if you still want to track overall progression)
    private int killCount = 0;
    
    // Make the instance accessible to other scripts
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // Simple singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // Method called by the RabbitController when a regular rabbit is killed
    public void RegisterKill()
    {
        killCount++;
        Debug.Log($"Kill registered: {killCount}/{totalRabbits}");
    }
    
    // Method to play sound effects
    public void PlaySound(string soundName)
    {
        if (!useSoundManager) return;
        
        SoundManager soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null)
        {
            soundManager.Play(soundName);
        }
        else
        {
            Debug.LogWarning("Sound Manager not found! Cannot play: " + soundName);
        }
    }
    
    // This is just a utility method to help implement pause/resume functionality if needed
    public void TogglePause(bool isPaused)
    {
        if (isPaused)
        {
            // Pause game
            Time.timeScale = 0f;
            
            // Optionally disable player controls
            FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
            if (fpc != null)
            {
                fpc.playerCanMove = false;
                fpc.cameraCanMove = false;
            }
        }
        else
        {
            // Resume game
            Time.timeScale = 1f;
            
            // Optionally re-enable player controls
            FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
            if (fpc != null)
            {
                fpc.playerCanMove = true;
                fpc.cameraCanMove = true;
            }
        }
    }
    
    // Utility function to restart the current scene
    public void RestartScene()
    {
        // Ensure time scale is normal
        Time.timeScale = 1f;
        
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    // Utility function to load a specific scene
    public void LoadScene(string sceneName)
    {
        // Ensure time scale is normal
        Time.timeScale = 1f;
        
        // Load the specified scene
        SceneManager.LoadScene(sceneName);
    }
}
