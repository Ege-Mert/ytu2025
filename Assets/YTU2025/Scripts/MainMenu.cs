using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu UI")]
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private string firstLevelScene = "ForestScene"; // The scene to load when Start is pressed
    
    [Header("Logo and Title")]
    [SerializeField] private RectTransform logoRect;
    [SerializeField] private TextMeshProUGUI gameTitle;
    
    [Header("Animation Settings")]
    [SerializeField] private float titleAnimationDuration = 1.5f;
    [SerializeField] private float buttonFadeInDelay = 0.5f;
    [SerializeField] private float buttonFadeInDuration = 0.8f;
    
    [Header("Transition Settings")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private float fadeOutDuration = 1f;
    
    [Header("Audio")]
    [SerializeField] private string menuMusicName = "MenuMusic";
    [SerializeField] private string buttonClickSoundName = "ButtonClick";
    [SerializeField] private string startGameSoundName = "StartGame";
    
    [Header("Pause Menu Reference")]
    [SerializeField] private GameObject pauseMenuPrefab; // Reference to the PauseMenu prefab
    
    private PauseMenu pauseMenu;
    
    void Awake()
    {
        // Initialize the main menu
        InitializeMainMenu();
        
        // Make sure cursor is visible and unlocked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Ensure time scale is set to 1 (normal)
        Time.timeScale = 1f;
    }
    
    void Start()
    {
        // Play menu music if available
        if (!string.IsNullOrEmpty(menuMusicName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(menuMusicName);
        }
        
        // Check if PauseMenu already exists
        pauseMenu = FindObjectOfType<PauseMenu>();
        
        // If the PauseMenu doesn't exist, create it from the prefab
        if (pauseMenu == null && pauseMenuPrefab != null)
        {
            GameObject pauseMenuObj = Instantiate(pauseMenuPrefab);
            pauseMenu = pauseMenuObj.GetComponent<PauseMenu>();
            
            // Make sure the pause menu is hidden initially
            if (pauseMenu != null)
            {
                Canvas menuCanvas = pauseMenu.GetComponent<Canvas>();
                if (menuCanvas != null)
                {
                    menuCanvas.gameObject.SetActive(false);
                }
            }
        }
        
        // Start with fade overlay visible, then fade it out
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 1f;
            fadeOverlay.DOFade(0f, 1f);
        }
        
        // Start menu animations
        StartCoroutine(PlayMenuAnimations());
    }
    
    void OnDestroy()
    {
        // Clean up DOTween animations when destroyed
        DOTween.Kill(logoRect);
        DOTween.Kill(gameTitle?.transform);
        if (fadeOverlay != null) DOTween.Kill(fadeOverlay);
    }
    
    private void InitializeMainMenu()
    {
        // Set up button listeners
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            
            // Start with buttons faded out
            CanvasGroup startBtnGroup = startButton.GetComponent<CanvasGroup>();
            if (startBtnGroup == null)
            {
                startBtnGroup = startButton.gameObject.AddComponent<CanvasGroup>();
            }
            startBtnGroup.alpha = 0f;
        }
        
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OpenOptions);
            
            // Start with buttons faded out
            CanvasGroup optionsBtnGroup = optionsButton.GetComponent<CanvasGroup>();
            if (optionsBtnGroup == null)
            {
                optionsBtnGroup = optionsButton.gameObject.AddComponent<CanvasGroup>();
            }
            optionsBtnGroup.alpha = 0f;
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
            
            // Start with buttons faded out
            CanvasGroup exitBtnGroup = exitButton.GetComponent<CanvasGroup>();
            if (exitBtnGroup == null)
            {
                exitBtnGroup = exitButton.gameObject.AddComponent<CanvasGroup>();
            }
            exitBtnGroup.alpha = 0f;
        }
    }
    
    private IEnumerator PlayMenuAnimations()
    {
        // Animate logo if available
        if (logoRect != null)
        {
            // Start slightly above final position and scaled down
            Vector2 finalPosition = logoRect.anchoredPosition;
            logoRect.anchoredPosition = new Vector2(finalPosition.x, finalPosition.y + 50f);
            logoRect.localScale = Vector3.one * 0.8f;
            
            // Animate to final position
            logoRect.DOAnchorPos(finalPosition, titleAnimationDuration).SetEase(Ease.OutBack);
            logoRect.DOScale(Vector3.one, titleAnimationDuration).SetEase(Ease.OutBack);
        }
        
        // Animate title if available
        if (gameTitle != null)
        {
            gameTitle.alpha = 0f;
            gameTitle.DOFade(1f, titleAnimationDuration).SetEase(Ease.OutCubic);
        }
        
        // Wait before showing buttons
        yield return new WaitForSeconds(buttonFadeInDelay);
        
        // Fade in buttons
        CanvasGroup[] buttonGroups = new CanvasGroup[3];
        if (startButton != null) buttonGroups[0] = startButton.GetComponent<CanvasGroup>();
        if (optionsButton != null) buttonGroups[1] = optionsButton.GetComponent<CanvasGroup>();
        if (exitButton != null) buttonGroups[2] = exitButton.GetComponent<CanvasGroup>();
        
        for (int i = 0; i < buttonGroups.Length; i++)
        {
            if (buttonGroups[i] != null)
            {
                buttonGroups[i].DOFade(1f, buttonFadeInDuration)
                    .SetDelay(i * 0.2f) // Stagger the button appearances
                    .SetEase(Ease.OutCubic);
            }
        }
    }
    
    public void StartGame()
    {
        // Play click sound
        if (!string.IsNullOrEmpty(buttonClickSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(buttonClickSoundName);
        }
        
        // Start the transition effect
        StartCoroutine(TransitionToGame());
    }
    
    private IEnumerator TransitionToGame()
    {
        // Fade out music if playing
        if (!string.IsNullOrEmpty(menuMusicName) && SoundManager.instance != null)
        {
            SoundManager.instance.FadeOut(menuMusicName, fadeOutDuration);
        }
        
        // Play start game sound if available
        if (!string.IsNullOrEmpty(startGameSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(startGameSoundName);
        }
        
        // Animate fade overlay
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.DOFade(1f, fadeOutDuration);
        }
        
        // Wait for the fade to complete
        yield return new WaitForSeconds(fadeOutDuration);
        
        // Load the first level
        SceneManager.LoadScene(firstLevelScene);
    }
    
    public void OpenOptions()
    {
        // Play click sound
        if (!string.IsNullOrEmpty(buttonClickSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(buttonClickSoundName);
        }
        
        // Use the PauseMenu to show options
        if (pauseMenu != null)
        {
            pauseMenu.ShowMenu();
            
            // Hide the main menu canvas while options are open
            if (mainMenuCanvas != null)
            {
                mainMenuCanvas.gameObject.SetActive(false);
            }
            
            // Start a coroutine to check when options are closed
            StartCoroutine(WaitForOptionsToClose());
        }
    }
    
    private IEnumerator WaitForOptionsToClose()
    {
        // Wait until the pause menu is no longer active
        while (pauseMenu != null && pauseMenu.IsMenuActive())
        {
            yield return null;
        }
        
        // Show the main menu canvas again
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.gameObject.SetActive(true);
        }
    }
    
    public void ExitGame()
    {
        // Play click sound
        if (!string.IsNullOrEmpty(buttonClickSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(buttonClickSoundName);
        }
        
        // Quit the game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
