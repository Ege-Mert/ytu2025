using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class VisualNovelController : MonoBehaviour
{
    [Header("Visual Novel UI")]
    [SerializeField] private GameObject visualNovelCanvas; // Changed to Canvas
    [SerializeField] private GameObject visualNovelPanel;
    [SerializeField] private Image opponentSprite;
    [SerializeField] private Image playerSprite;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject clickToContinueIcon;
    [SerializeField] private float textSpeed = 0.05f;
    
    [Header("Character Sprite Animation")]
    [SerializeField] private float characterFadeInDuration = 1.0f;
    [SerializeField] private Ease characterFadeInEase = Ease.InOutQuad;
    
    [Header("Choice UI")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Button choiceA;
    [SerializeField] private Button choiceB;
    [SerializeField] private TextMeshProUGUI choiceAText;
    [SerializeField] private TextMeshProUGUI choiceBText;
    
    [Header("Click Indicator")]
    [SerializeField] private float clickIndicatorDelay = 3f;
    [SerializeField] private float clickIndicatorPulseDuration = 1f;
    [SerializeField] private float clickIndicatorPulseScale = 1.2f;
    
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "EndingScene";
    [SerializeField] private Image transitionFadeImage; // Dedicated fade image for transitions
    [SerializeField] private float fadeOutDuration = 1.0f;
    
    // State variables
    private ArmWrestleOpponentConfig currentOpponent;
    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool waitingForInput = false;
    private Coroutine clickIndicatorCoroutine;
    private bool hasChoiceMade = false;
    
    private UIManager uiManager;
    private CanvasGroup visualNovelCanvasGroup;
    
    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        
        // Setup canvas group for fading
        if (visualNovelCanvas != null)
        {
            // Get or add CanvasGroup component
            if (visualNovelCanvasGroup == null)
            {
                visualNovelCanvasGroup = visualNovelCanvas.GetComponent<CanvasGroup>();
                if (visualNovelCanvasGroup == null)
                {
                    visualNovelCanvasGroup = visualNovelCanvas.AddComponent<CanvasGroup>();
                }
            }
            visualNovelCanvasGroup.alpha = 0f; // Start fully transparent
            visualNovelCanvas.SetActive(false);
        }
        
        if (visualNovelPanel != null)
        {
            visualNovelPanel.SetActive(false);
        }
        
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
        
        if (clickToContinueIcon != null)
        {
            clickToContinueIcon.SetActive(false);
        }
        
        // Set up transition fade image if assigned
        if (transitionFadeImage != null)
        {
            // Initialize as transparent
            Color fadeColor = transitionFadeImage.color;
            fadeColor.a = 0f;
            transitionFadeImage.color = fadeColor;
            transitionFadeImage.gameObject.SetActive(true);
        }
    }
    
    public void StartVisualNovel(ArmWrestleOpponentConfig opponent)
    {
        // Initialize the visual novel
        currentOpponent = opponent;
        dialogueLines = opponent.dialogLines;
        currentLineIndex = 0;
        
        // Setup sprite colors with zero alpha but don't fade in yet
        if (opponentSprite != null && opponent.normalSprite != null)
        {
            opponentSprite.sprite = opponent.normalSprite;
            Color spriteColor = opponentSprite.color;
            spriteColor.a = 0f;
            opponentSprite.color = spriteColor;
        }
        
        if (playerSprite != null && opponent.playerNormalSprite != null)
        {
            playerSprite.sprite = opponent.playerNormalSprite;
            Color spriteColor = playerSprite.color;
            spriteColor.a = 0f;
            playerSprite.color = spriteColor;
        }
        
        // First activate the canvas and panel
        if (visualNovelCanvas != null)
        {
            visualNovelCanvas.SetActive(true);
        }
        
        if (visualNovelPanel != null)
        {
            visualNovelPanel.SetActive(true);
        }
        
        // Wait a frame to ensure everything is activated before starting animations
        StartCoroutine(DelayedFadeIn());
        
        // Set up the choice buttons
        if (choiceA != null && choiceAText != null)
        {
            choiceAText.text = opponent.choiceA;
            choiceA.onClick.RemoveAllListeners();
            choiceA.onClick.AddListener(() => OnChoiceMade(true));
        }
        
        if (choiceB != null && choiceBText != null)
        {
            choiceBText.text = opponent.choiceB;
            choiceB.onClick.RemoveAllListeners();
            choiceB.onClick.AddListener(() => OnChoiceMade(false));
        }
        
        // Start displaying dialogue
        DisplayNextLine();
    }
    
    private IEnumerator DelayedFadeIn()
    {
        // Wait one frame for the canvas to be properly activated
        yield return null;
        
        // Debug log to verify this is running
        Debug.Log("Starting UI and character fade-in...");
        
        // Fade in the entire visual novel UI
        if (visualNovelCanvasGroup != null)
        {
            visualNovelCanvasGroup.DOFade(1f, characterFadeInDuration).SetEase(characterFadeInEase);
            Debug.Log("Fading in visual novel canvas");
        }
        
        // Now do the fade in animations for individual sprites
        if (opponentSprite != null)
        {
            opponentSprite.DOFade(1f, characterFadeInDuration).SetEase(characterFadeInEase);
            Debug.Log($"Fading in opponent sprite. Current alpha: {opponentSprite.color.a}");
        }
        
        if (playerSprite != null)
        {
            playerSprite.DOFade(1f, characterFadeInDuration).SetEase(characterFadeInEase);
            Debug.Log($"Fading in player sprite. Current alpha: {playerSprite.color.a}");
        }
    }
    
    private void Update()
    {
        // Skip input processing if a choice has already been made
        if (hasChoiceMade)
            return;
        
        // Check for player input to advance dialogue
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Skip typing animation - instantly show full text
                SkipTypewriterEffect();
            }
            else if (waitingForInput)
            {
                // Move to the next line
                currentLineIndex++;
                DisplayNextLine();
            }
        }
    }
    
    // Method to skip the typewriter effect and show the full text immediately
    private void SkipTypewriterEffect()
    {
        // Stop any ongoing typing coroutines
        StopAllCoroutines();
        
        // Display the full text immediately
        if (dialogueText != null && currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
        
        // Update state variables
        isTyping = false;
        waitingForInput = true;
        
        // Restart the click indicator coroutine
        if (clickIndicatorCoroutine != null)
        {
            StopCoroutine(clickIndicatorCoroutine);
        }
        clickIndicatorCoroutine = StartCoroutine(ShowClickIndicator());
    }
    
    private void DisplayNextLine()
    {
        // Hide click indicator
        if (clickToContinueIcon != null)
        {
            clickToContinueIcon.SetActive(false);
        }
        
        // Check if we've reached the choice point and haven't made a choice yet
        if (currentLineIndex >= dialogueLines.Length && !hasChoiceMade)
        {
            // Show the choices
            ShowChoicePanel();
            return;
        }
        
        // Reset state
        waitingForInput = false;
        
        // Start typing animation
        StartCoroutine(TypeDialogue(dialogueLines[currentLineIndex]));
    }
    
    private IEnumerator TypeDialogue(string line)
    {
        // Set typing state to true so we know we're in the process of displaying text
        isTyping = true;
        waitingForInput = false; // Allow input to skip text even if not finished
        
        if (dialogueText != null)
        {
            dialogueText.text = "";
            
            foreach (char c in line)
            {
                // Check if typing has been interrupted (by skipping)
                if (!isTyping)
                    break;
                    
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }
        }
        
        // Only set these if we finished typing naturally (not skipped)
        if (isTyping)
        {
            isTyping = false;
            waitingForInput = true;
            
            // Show click indicator after delay
            if (clickIndicatorCoroutine != null)
            {
                StopCoroutine(clickIndicatorCoroutine);
            }
            clickIndicatorCoroutine = StartCoroutine(ShowClickIndicator());
        }
    }
    
    private IEnumerator ShowClickIndicator()
    {
        yield return new WaitForSeconds(clickIndicatorDelay);
        
        if (clickToContinueIcon != null)
        {
            clickToContinueIcon.SetActive(true);
            
            // Create pulse animation
            Sequence pulseSequence = DOTween.Sequence();
            
            pulseSequence.Append(clickToContinueIcon.transform.DOScale(clickIndicatorPulseScale, clickIndicatorPulseDuration / 2)
                .SetEase(Ease.InOutQuad));
            pulseSequence.Append(clickToContinueIcon.transform.DOScale(1f, clickIndicatorPulseDuration / 2)
                .SetEase(Ease.InOutQuad));
            
            pulseSequence.SetLoops(-1);
        }
    }
    
    private void ShowChoicePanel()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
        }
    }
    
    private void OnChoiceMade(bool isChoiceA)
    {
        // Set our flag to indicate a choice has been made
        hasChoiceMade = true;
        
        // Immediately prevent further choices by disabling button interactability
        if (choiceA != null) choiceA.interactable = false;
        if (choiceB != null) choiceB.interactable = false;
        
        // Store original texts before swapping
        string originalTextA = choiceAText.text;
        string originalTextB = choiceBText.text;
        
        // Completely swap the texts between options
        if (choiceAText != null && choiceBText != null)
        {
            // Swap texts completely
            choiceAText.text = originalTextB;
            choiceBText.text = originalTextA;
            
            // Add a brief visual emphasis to the choice that was made
            Transform buttonTransform = isChoiceA ? choiceA.transform : choiceB.transform;
            buttonTransform.DOScale(1.1f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
        
        // Add a small delay before hiding the panel to let the swap effect be visible
        StartCoroutine(DelayedChoicePanel());
        
        // Change player sprite to choice sprite
        if (playerSprite != null && currentOpponent.playerChoiceSprite != null)
        {
            // Log sprite change for debugging
            Debug.Log($"Changing player sprite from {playerSprite.sprite?.name} to {currentOpponent.playerChoiceSprite.name}");
            playerSprite.sprite = currentOpponent.playerChoiceSprite;
        }
        else
        {
            // Debug why sprite change failed
            if (playerSprite == null)
                Debug.LogError("Player sprite reference is null! Cannot change sprite.");
            else if (currentOpponent.playerChoiceSprite == null)
                Debug.LogError("PlayerChoiceSprite is not set in the ArmWrestleOpponentConfig! Set it in the inspector.");
        }
        
        // Show the opposite choice text as the response
        if (dialogueText != null)
        {
            // If A was clicked, show what was originally in option B
            // If B was clicked, show what was originally in option A
            string responseText = isChoiceA ? originalTextB : originalTextA;
            dialogueText.text = responseText;
        }
        
        // Wait and then transition to next scene
        StartCoroutine(TransitionAfterDelay(currentOpponent.responseDuration));
    }
    
    // New coroutine to delay hiding the choice panel
    private IEnumerator DelayedChoicePanel()
    {
        // Wait a short time to let the text swap be visible
        yield return new WaitForSeconds(0.5f);
        
        // Hide the choice panel after the delay
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
            Debug.Log("Choice panel disabled after showing text swap effect");
        }
    }
    
    private IEnumerator TransitionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Fade to black and transition to next scene
        if (transitionFadeImage != null)
        {
            // Use our own fade image
            FadeToBlackAndTransition();
        }
        else if (uiManager != null)
        {
            // Fallback to UIManager if available
            uiManager.FadeToBlack(fadeOutDuration, () => {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            });
        }
        else
        {
            // Just load the scene if no fade options available
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
    
    private void FadeToBlackAndTransition()
    {
        if (transitionFadeImage == null) return;
        
        // Make sure the fade image is active and in front
        transitionFadeImage.gameObject.SetActive(true);
        transitionFadeImage.transform.SetAsLastSibling(); // bring to front
        
        // Fade to black
        transitionFadeImage.DOFade(1f, fadeOutDuration)
            .SetUpdate(true) // Make it time-scale independent
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                // Load the next scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            });
    }
}
