using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class TutorialSystem : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private Image[] tutorialPages; // Array of tutorial page images
    [SerializeField] private float inactivityTimeForHint = 3f; // Seconds before showing the hint
    [SerializeField] private bool pauseGameDuringTutorial = true;
    [SerializeField] private bool showTutorialOnce = true; // If true, tutorial will only show once using PlayerPrefs
    [SerializeField] private string tutorialKey; // PlayerPrefs key for this tutorial - set to scene name by default

    [Header("Audio")]
    [SerializeField] private string tutorialOpenSoundName = ""; // Optional sound when tutorial opens
    
    [Header("References")]
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private TextMeshProUGUI clickToContinueText;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private int currentPage = 0;
    private float inactivityTimer = 0f;
    private bool isTutorialActive = false;
    private bool wasGamePaused = false;
    
    void Awake()
    {
        // If tutorial key isn't set, use the scene name
        if (string.IsNullOrEmpty(tutorialKey))
        {
            tutorialKey = "Tutorial_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
        
        // Make sure the tutorial canvas is initially inactive
        if (tutorialCanvas != null)
        {
            tutorialCanvas.gameObject.SetActive(false);
        }
        
        // Hide all pages initially except the first one
        if (tutorialPages != null && tutorialPages.Length > 0)
        {
            for (int i = 0; i < tutorialPages.Length; i++)
            {
                if (tutorialPages[i] != null)
                {
                    tutorialPages[i].gameObject.SetActive(false);
                }
            }
        }
        
        // Hide "click to continue" text initially
        if (clickToContinueText != null)
        {
            clickToContinueText.gameObject.SetActive(false);
        }
    }
    
    void Start()
    {
        // Short delay to ensure everything is initialized
        StartCoroutine(DelayedStart());
    }
    
    IEnumerator DelayedStart()
    {
        // Wait a brief moment to ensure PauseMenu is initialized first
        yield return new WaitForSecondsRealtime(0.1f);
        
        // Check if we should show the tutorial
        if (showTutorialOnce && PlayerPrefs.GetInt(tutorialKey, 0) == 1)
        {
            // Tutorial already shown, don't show again
            yield break;
        }
        
        // Show the tutorial immediately
        ShowTutorial();
    }
    
    void Update()
    {
        // Only handle input if tutorial is active
        if (isTutorialActive)
        {
            // Check for any input to advance/dismiss the tutorial
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                inactivityTimer = 0f;
                
                // Hide the hint if it's showing
                if (clickToContinueText != null && clickToContinueText.gameObject.activeInHierarchy)
                {
                    clickToContinueText.gameObject.SetActive(false);
                    DOTween.Kill(clickToContinueText.transform);
                }
                
                // Advance to next page or close tutorial
                NextPage();
            }
            else
            {
                // Increment the inactivity timer
                inactivityTimer += Time.unscaledDeltaTime;
                
                // Show the hint if inactive for too long
                if (inactivityTimer >= inactivityTimeForHint && clickToContinueText != null)
                {
                    if (!clickToContinueText.gameObject.activeInHierarchy)
                    {
                        clickToContinueText.gameObject.SetActive(true);
                        // Optional: Animate the hint
                        clickToContinueText.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                    }
                }
            }
        }
    }
    
    public void ShowTutorial()
    {
        // If we already have a canvas active, don't do anything
        if (isTutorialActive) return;
        
        if (debugMode)
        {
            Debug.Log("ShowTutorial called. Current timeScale: " + Time.timeScale);
        }
        
        // Check if the game is already paused (by PauseMenu or other systems)
        wasGamePaused = Time.timeScale == 0f;
        
        // Pause the game if needed
        if (pauseGameDuringTutorial && !wasGamePaused)
        {
            Time.timeScale = 0f;
            
            if (debugMode)
            {
                Debug.Log("Setting timeScale to 0");
            }
            
            // Disable player control
            DisablePlayerControls();
        }
        
        // Reset current page
        currentPage = 0;
        
        // Show the first page
        if (tutorialPages != null && tutorialPages.Length > 0)
        {
            for (int i = 0; i < tutorialPages.Length; i++)
            {
                if (tutorialPages[i] != null)
                {
                    tutorialPages[i].gameObject.SetActive(i == 0);
                }
            }
        }
        
        // Show the tutorial canvas
        if (tutorialCanvas != null)
        {
            tutorialCanvas.gameObject.SetActive(true);
            
            // Add a fade-in effect
            CanvasGroup canvasGroup = tutorialCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tutorialCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
        }
        
        // Play the sound effect if specified
        if (!string.IsNullOrEmpty(tutorialOpenSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(tutorialOpenSoundName);
        }
        
        // Set the flag
        isTutorialActive = true;
        
        // Reset inactivity timer
        inactivityTimer = 0f;
        
        // Make sure cursor is visible during tutorial
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void NextPage()
    {
        // Increment the current page
        currentPage++;
        
        // Check if we've reached the end
        if (tutorialPages == null || currentPage >= tutorialPages.Length)
        {
            CloseTutorial();
            return;
        }
        
        // Hide all pages
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            if (tutorialPages[i] != null)
            {
                tutorialPages[i].gameObject.SetActive(i == currentPage);
            }
        }
        
        // Add a page flip effect
        if (tutorialPages[currentPage] != null)
        {
            tutorialPages[currentPage].transform.localScale = Vector3.zero;
            tutorialPages[currentPage].transform.DOScale(1f, 0.3f).SetUpdate(true);
        }
        
        // Reset inactivity timer
        inactivityTimer = 0f;
        
        // Hide the hint if it's showing
        if (clickToContinueText != null && clickToContinueText.gameObject.activeInHierarchy)
        {
            clickToContinueText.gameObject.SetActive(false);
            DOTween.Kill(clickToContinueText.transform);
        }
    }
    
    public void CloseTutorial()
    {
        if (debugMode)
        {
            Debug.Log("CloseTutorial called. Current timeScale: " + Time.timeScale + ", wasGamePaused: " + wasGamePaused);
        }
        
        // Mark this tutorial as shown
        if (showTutorialOnce)
        {
            PlayerPrefs.SetInt(tutorialKey, 1);
            PlayerPrefs.Save();
        }
        
        // Stop any DOTween animations
        if (clickToContinueText != null)
        {
            DOTween.Kill(clickToContinueText.transform);
            clickToContinueText.gameObject.SetActive(false);
        }
        
        // Add a fade-out effect
        if (tutorialCanvas != null)
        {
            CanvasGroup canvasGroup = tutorialCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tutorialCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() => {
                tutorialCanvas.gameObject.SetActive(false);
                
                // Restore time scale only if we paused it and game wasn't already paused
                if (pauseGameDuringTutorial && !wasGamePaused)
                {
                    // Check if PauseMenu is active before restoring time scale
                    bool pauseMenuActive = PauseMenu.Instance != null && PauseMenu.Instance.IsMenuActive();
                    
                    if (!pauseMenuActive)
                    {
                        Time.timeScale = 1f;
                        
                        if (debugMode)
                        {
                            Debug.Log("Setting timeScale to 1");
                        }
                    }
                    
                    // Re-enable player controls if pause menu is not active
                    if (!pauseMenuActive)
                    {
                        EnablePlayerControls();
                    }
                }
                
                // Clear the flag
                isTutorialActive = false;
                
                // Check if we need to restore cursor lock state
                RestoreCursorState();
            });
        }
        else
        {
            // No canvas group, do immediate cleanup
            
            // Restore time scale only if we paused it and game wasn't already paused
            if (pauseGameDuringTutorial && !wasGamePaused)
            {
                // Check if PauseMenu is active before restoring time scale
                bool pauseMenuActive = PauseMenu.Instance != null && PauseMenu.Instance.IsMenuActive();
                
                if (!pauseMenuActive)
                {
                    Time.timeScale = 1f;
                    
                    if (debugMode)
                    {
                        Debug.Log("Setting timeScale to 1");
                    }
                }
                
                // Re-enable player controls if pause menu is not active
                if (!pauseMenuActive)
                {
                    EnablePlayerControls();
                }
            }
            
            // Clear the flag
            isTutorialActive = false;
            
            // Check if we need to restore cursor lock state
            RestoreCursorState();
        }
    }
    
    private void DisablePlayerControls()
    {
        FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
        if (fpc != null)
        {
            fpc.playerCanMove = false;
            fpc.cameraCanMove = false;
        }
    }
    
    private void EnablePlayerControls()
    {
        FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
        if (fpc != null)
        {
            fpc.playerCanMove = true;
            fpc.cameraCanMove = true;
        }
    }
    
    private void RestoreCursorState()
    {
        // Check if PauseMenu is active
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsMenuActive())
        {
            // PauseMenu is active, let it handle cursor
            return;
        }
        
        // If not, restore based on player controller settings
        FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
        if (fpc != null && fpc.lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    // Method to allow other scripts to check if the tutorial is active
    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }
    
    // Method for manual control of showing tutorials
    public void ShowTutorialWithKey(string key)
    {
        // Set the key and show the tutorial
        tutorialKey = key;
        ShowTutorial();
    }
    
    // Check if tutorial has been shown before
    public bool HasTutorialBeenShown(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }
}
