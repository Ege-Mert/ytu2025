using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CourtSceneController : MonoBehaviour
{
    [Header("Scene Components")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Transform lawyerTransform;
    [SerializeField] private Transform judgeTransform;
    [SerializeField] private List<Transform> jurorTransforms;
    [SerializeField] private Transform playerTransform;
    
    [Header("Character Sprites")]
    [SerializeField] private SpriteRenderer lawyerRenderer;
    [SerializeField] private SpriteRenderer judgeRenderer;
    [SerializeField] private List<SpriteRenderer> jurorRenderers;
    [SerializeField] private Sprite lawyerNormalSprite;
    [SerializeField] private Sprite lawyerShockedSprite;
    [SerializeField] private Sprite judgeNormalSprite;
    [SerializeField] private Sprite judgeHammerSprite;
    [SerializeField] private Sprite jurorNormalSprite;
    [SerializeField] private Sprite jurorShockedSprite;
    
    [Header("Chat Bubbles")]
    [SerializeField] private GameObject lawyerChatBubble;
    [SerializeField] private TextMeshProUGUI lawyerChatText;
    [SerializeField] private GameObject judgeChatBubble;
    [SerializeField] private TextMeshProUGUI judgeChatText;
    
    [Header("Final Scene Bubbles")]
    [SerializeField, Tooltip("Separate speech bubble for lawyer's shocked reaction")]
    private GameObject lawyerShockedBubble;
    [SerializeField] private TextMeshProUGUI lawyerShockedText;
    [SerializeField, Tooltip("Second speech bubble for lawyer's plea")]
    private GameObject lawyerPleaBubble;
    [SerializeField] private TextMeshProUGUI lawyerPleaText;
    
    [Header("Dialogue Text")]
    [SerializeField, Tooltip("First dialogue from lawyer")]
    private string lawyerIntroText = "Repeat after me.";
    [SerializeField, Tooltip("Lawyer's first shocked response after final twist")]
    private string lawyerShockedMessage = "Wait, that's not what I said!";
    [SerializeField, Tooltip("Lawyer's second shocked response after final twist")]
    private string lawyerPleaMessage = "Your Honor, there must be some mistake!";
    [SerializeField, Tooltip("Judge's response after each round")]
    private string judgeRegularResponse = "I dismiss your defense.";
    [SerializeField, Tooltip("Judge's final sentence")]
    private string judgeFinalSentence = "You are sentenced to life imprisonment.";
    
    [Header("Camera Settings")]
    [SerializeField] private Vector3 lawyerCloseupPosition;
    [SerializeField] private Vector3 courtRoomPosition;
    [SerializeField] private float cameraPullbackDuration = 1f;
    
    [Header("Timing Settings")]
    [SerializeField, Tooltip("Initial fade-in duration")]
    private float initialFadeDuration = 0.5f;
    [SerializeField, Tooltip("Delay after fade-in before showing lawyer dialogue")]
    private float postFadeDelay = 0.5f;
    [SerializeField, Tooltip("Duration to show lawyer's intro text")]
    private float lawyerIntroTextDuration = 2.0f;
    [SerializeField, Tooltip("Pause after judge dismisses defense before next round")]
    private float pauseBeforeNextRound = 1.5f;
    [SerializeField, Tooltip("Delay between judge hammer hits")]
    private float hammerHitInterval = 0.5f;
    [SerializeField, Tooltip("Duration of screen shake for each hammer hit")]
    private float hammerShakeDuration = 0.2f;
    [SerializeField, Tooltip("Intensity of screen shake for each hammer hit")]
    private float hammerShakeIntensity = 0.3f;
    [SerializeField, Tooltip("Duration of first shocked speech bubble")]
    private float firstShockedBubbleDuration = 2.0f;
    [SerializeField, Tooltip("Duration of second shocked speech bubble")]
    private float secondShockedBubbleDuration = 2.0f;
    [SerializeField, Tooltip("Delay after final sentence before fade begins")]
    private float sentenceToFadeDelay = 3.0f;
    [SerializeField, Tooltip("Final fade to black duration")]
    private float finalFadeDuration = 1.0f;
    [SerializeField, Tooltip("Delay after fade to black before scene transition")]
    private float postFinalFadeDelay = 1.0f;
    
    [Header("Character Animation")]
    [SerializeField, Tooltip("Stretch factor for the bounce effect (1.0 = no stretch)")]
    private float bounceStretchFactor = 1.2f;
    [SerializeField, Tooltip("Squash factor for the bounce effect (1.0 = no squash)")]
    private float bounceSquashFactor = 0.8f;
    [SerializeField, Tooltip("Initial bounce cycle duration")]
    private float initialBounceCycleDuration = 0.5f;
    [SerializeField, Tooltip("Fastest bounce cycle duration when timer is low")]
    private float fastestBounceCycleDuration = 0.2f;
    [SerializeField, Tooltip("Curve controlling the bounce motion")]
    private AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, Tooltip("Whether to squash the character horizontally while stretching vertically")]
    private bool squashAndStretch = true;
    [SerializeField, Tooltip("Threshold (0-1) of timer where bounce speed starts increasing")]
    private float bounceSpeedupThreshold = 0.5f;
    [SerializeField] private List<Transform> allBouncingCharacters = new List<Transform>();
    
    [Header("Between-Round Transition")]
    [SerializeField] private GameObject roundTransitionImage;
    [SerializeField] private float transitionImageDuration = 1f;
    [SerializeField] private float transitionImageInEaseFactor = 0.3f;
    [SerializeField] private float transitionImageOutEaseFactor = 0.7f;
    [SerializeField] private bool useRoundTransition = true;
    [SerializeField] private Vector2 transitionStartPosition = new Vector2(-1000, 0);
    [SerializeField] private Vector2 transitionCenterPosition = new Vector2(0, 0);
    [SerializeField] private Vector2 transitionEndPosition = new Vector2(1000, 0);
    
    [Header("Court Rounds")]
    [SerializeField] private List<CourtRoundConfig> rounds;
    
    [Header("Typing Game")]
    [SerializeField] private CourtTypingSystem typingSystem;
    
    [Header("Audio")]
    [SerializeField] private string gavelSoundName = "gavel";
    [SerializeField] private string sirenSoundName = "sirens";
    [SerializeField] private string gaspSoundName = "gasp";
    [SerializeField] private string transitionSoundName = "transition";
    
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private bool restartSceneOnEnd = true;
    
    // State tracking
    private int currentRoundIndex = -1;
    private Dictionary<Transform, Sequence> characterBounces = new Dictionary<Transform, Sequence>();
    private Dictionary<Transform, Vector3> characterOriginalScales = new Dictionary<Transform, Vector3>();
    private float currentBounceCycleDuration;
    
    private void Start()
    {
        // Ensure we have a reference to the typing system
        if (typingSystem == null)
        {
            typingSystem = FindObjectOfType<CourtTypingSystem>();
            if (typingSystem == null)
            {
                Debug.LogError("CourtTypingSystem not found! Please add it to the scene.");
                return;
            }
        }

        // Set up callbacks from the typing system
        typingSystem.OnWordComplete += HandleWordComplete;
        typingSystem.OnRoundComplete += HandleRoundComplete;
        typingSystem.OnRoundFailed += HandleRoundFailed;
        typingSystem.OnSentenceDisplayFinished += HandleSentenceDisplayFinished;
        typingSystem.OnTimerUpdated += UpdateBounceSpeedBasedOnTimer;
        
        // Hide transition image if it exists
        if (roundTransitionImage != null)
        {
            roundTransitionImage.SetActive(false);
        }
        
        // Hide final scene speech bubbles
        if (lawyerShockedBubble != null) lawyerShockedBubble.SetActive(false);
        if (lawyerPleaBubble != null) lawyerPleaBubble.SetActive(false);
        
        // Initialize bounce duration
        currentBounceCycleDuration = initialBounceCycleDuration;
        
        // Set initial sprite states
        SetCharacterSprites(false);
        
        // Start with the scene fade-in sequence
        fadeCanvasGroup.alpha = 1f;
        StartCoroutine(IntroSequence());
    }
    
    private void SetCharacterSprites(bool shocked)
    {
        // Set lawyer sprite
        if (lawyerRenderer != null) 
        {
            lawyerRenderer.sprite = shocked ? lawyerShockedSprite : lawyerNormalSprite;
        }
        
        // Set judge to normal sprite (hammer sprite is only used during animation)
        if (judgeRenderer != null)
        {
            judgeRenderer.sprite = judgeNormalSprite;
        }
        
        // Set all juror sprites
        foreach (SpriteRenderer jurorRenderer in jurorRenderers)
        {
            if (jurorRenderer != null)
            {
                jurorRenderer.sprite = shocked ? jurorShockedSprite : jurorNormalSprite;
            }
        }
    }
    
    private IEnumerator IntroSequence()
    {
        // Start with the police sirens
        PlaySound(sirenSoundName);
        
        // Initial fade from black
        fadeCanvasGroup.DOFade(0, initialFadeDuration);
        
        // Gavel sound as fade begins
        PlaySound(gavelSoundName);
        
        yield return new WaitForSeconds(postFadeDelay);
        
        // Close-up on lawyer's face
        mainCamera.transform.position = lawyerCloseupPosition;
        
        // Show lawyer chat bubble with intro text
        ShowChatBubble(lawyerChatBubble, lawyerChatText, lawyerIntroText);
        
        yield return new WaitForSeconds(lawyerIntroTextDuration);
        
        // Hide lawyer chat bubble BEFORE camera starts moving
        HideChatBubble(lawyerChatBubble);
        
        // Camera pull-back to reveal full courtroom
        mainCamera.transform.DOMove(courtRoomPosition, cameraPullbackDuration);
        
        yield return new WaitForSeconds(cameraPullbackDuration);
        
        // Initialize character list if needed
        InitializeCharacterList();
        
        // Start the bouncing animation for all characters
        StartAllCharactersBouncing();
        
        // Start first round
        StartNextRound();
    }
    
    private void InitializeCharacterList()
    {
        // Clear the existing list
        if (allBouncingCharacters.Count == 0)
        {
            // Add all main characters if they exist
            if (playerTransform != null) allBouncingCharacters.Add(playerTransform);
            if (lawyerTransform != null) allBouncingCharacters.Add(lawyerTransform);
            if (judgeTransform != null) allBouncingCharacters.Add(judgeTransform);
            
            // Add all jurors
            if (jurorTransforms != null)
            {
                foreach (Transform juror in jurorTransforms)
                {
                    if (juror != null) allBouncingCharacters.Add(juror);
                }
            }
        }
        
        // Store original scales
        foreach (Transform character in allBouncingCharacters)
        {
            if (character != null)
            {
                characterOriginalScales[character] = character.localScale;
            }
        }
    }
    
    // Centralized method to start all characters bouncing
    public void StartAllCharactersBouncing()
    {
        foreach (Transform character in allBouncingCharacters)
        {
            StartCharacterBounce(character);
        }
    }
    
    // Method to start the squash and stretch bounce effect for a single character
    private void StartCharacterBounce(Transform characterTransform)
    {
        if (characterTransform == null) return;
        
        // Stop any existing bounce
        StopCharacterBounce(characterTransform);
        
        // Get original scale
        Vector3 originalScale = characterOriginalScales.ContainsKey(characterTransform) ? 
            characterOriginalScales[characterTransform] : characterTransform.localScale;
        
        // Create a bounce sequence
        Sequence bounceSequence = DOTween.Sequence();
        
        if (squashAndStretch)
        {
            // Stretch vertically, squash horizontally (conserving volume)
            Vector3 stretchScale = new Vector3(
                originalScale.x * Mathf.Sqrt(1f / bounceStretchFactor), // Reduce width to conserve "volume"
                originalScale.y * bounceStretchFactor,                  // Stretch height
                originalScale.z);
            
            // Squash vertically, stretch horizontally
            Vector3 squashScale = new Vector3(
                originalScale.x * Mathf.Sqrt(1f / bounceSquashFactor), // Increase width 
                originalScale.y * bounceSquashFactor,                  // Squash height
                originalScale.z);
            
            // Add the stretch phase (1/3 of cycle)
            bounceSequence.Append(characterTransform.DOScale(stretchScale, currentBounceCycleDuration / 3f)
                .SetEase(bounceCurve));
            
            // Add the squash phase (1/3 of cycle)
            bounceSequence.Append(characterTransform.DOScale(squashScale, currentBounceCycleDuration / 3f)
                .SetEase(bounceCurve));
            
            // Return to original scale (1/3 of cycle)
            bounceSequence.Append(characterTransform.DOScale(originalScale, currentBounceCycleDuration / 3f)
                .SetEase(bounceCurve));
        }
        else
        {
            // Simple vertical scaling only (maintain x scale)
            Vector3 stretchScale = new Vector3(
                originalScale.x,
                originalScale.y * bounceStretchFactor, 
                originalScale.z);
            
            Vector3 squashScale = new Vector3(
                originalScale.x,
                originalScale.y * bounceSquashFactor,
                originalScale.z);
            
            // Add the stretch phase (1/3 of cycle)
            bounceSequence.Append(characterTransform.DOScale(stretchScale, currentBounceCycleDuration / 3f)
                .SetEase(bounceCurve));
            
            // Add the squash phase (1/3 of cycle)
            bounceSequence.Append(characterTransform.DOScale(squashScale, currentBounceCycleDuration / 3f)
                .SetEase(bounceCurve));
            
            // Return to original scale (1/3 of cycle)
            bounceSequence.Append(characterTransform.DOScale(originalScale, currentBounceCycleDuration / 3f)
                .SetEase(bounceCurve));
        }
        
        // Set the sequence to loop infinitely
        bounceSequence.SetLoops(-1, LoopType.Restart);
        bounceSequence.SetId("CourtBounce");
        
        // Store the sequence for later reference
        characterBounces[characterTransform] = bounceSequence;
    }
    
    // Method to update the bounce speed based on timer
    private void UpdateBounceSpeedBasedOnTimer(float normalizedTimeRemaining)
    {
        // Only speed up when timer is below threshold
        if (normalizedTimeRemaining <= bounceSpeedupThreshold)
        {
            // Remap [0, threshold] to [0, 1] for interpolation
            float t = 1f - (normalizedTimeRemaining / bounceSpeedupThreshold);
            
            // Calculate new bounce cycle duration using lerp
            float newDuration = Mathf.Lerp(initialBounceCycleDuration, fastestBounceCycleDuration, t);
            
            // Only update if significantly different
            if (Mathf.Abs(newDuration - currentBounceCycleDuration) > 0.01f)
            {
                currentBounceCycleDuration = newDuration;
                
                // Restart all bouncing with new speed
                RestartAllBouncing();
            }
        }
        else if (currentBounceCycleDuration != initialBounceCycleDuration)
        {
            // Reset to normal speed if we're above threshold and not already at normal speed
            currentBounceCycleDuration = initialBounceCycleDuration;
            RestartAllBouncing();
        }
    }
    
    // Method to restart all bouncing animations (preserving character list)
    private void RestartAllBouncing()
    {
        StopAllBounces();
        StartAllCharactersBouncing();
    }
    
    // Method to stop a single character from bouncing
    public void StopCharacterBounce(Transform characterTransform)
    {
        if (characterTransform == null) return;
        
        // If we have an active sequence for this character, kill it
        if (characterBounces.ContainsKey(characterTransform) && 
            characterBounces[characterTransform] != null && 
            characterBounces[characterTransform].IsActive())
        {
            characterBounces[characterTransform].Kill();
            characterBounces.Remove(characterTransform);
        }
        
        // Reset to original scale if we know it
        if (characterOriginalScales.ContainsKey(characterTransform))
        {
            characterTransform.localScale = characterOriginalScales[characterTransform];
        }
    }
    
    // Method to stop all characters from bouncing
    public void StopAllBounces()
    {
        // Kill all tweens with our identifier
        DOTween.Kill("CourtBounce");
        
        // Reset all characters to their original scales
        foreach (Transform character in allBouncingCharacters)
        {
            if (character != null && characterOriginalScales.ContainsKey(character))
            {
                character.localScale = characterOriginalScales[character];
            }
        }
        
        // Clear the collection
        characterBounces.Clear();
    }
    
    // Method to adjust bounce parameters and restart animations
    public void UpdateBounceParameters(float newStretchFactor, float newSquashFactor)
    {
        bounceStretchFactor = newStretchFactor;
        bounceSquashFactor = newSquashFactor;
        
        // Restart all bounces with new parameters
        RestartAllBouncing();
    }
    
    private void StartNextRound()
    {
        currentRoundIndex++;
        
        if (currentRoundIndex >= rounds.Count)
        {
            // All rounds complete, should not happen normally as the game
            // should end after Round 5's twist
            Debug.LogError("Attempted to start round beyond the configured rounds count!");
            return;
        }
        
        // Get the current round config
        CourtRoundConfig round = rounds[currentRoundIndex];
        
        // Reset bounce cycle duration
        currentBounceCycleDuration = initialBounceCycleDuration;
        RestartAllBouncing();
        
        // Configure and start the typing system for this round
        typingSystem.StartRound(round);
    }
    
    private void HandleWordComplete(string word, bool isCorrect)
    {
        // This could play feedback sounds, animations, etc.
        Debug.Log($"Word complete: {word}, correct: {isCorrect}");
    }
    
    private void HandleRoundComplete(bool isLastRound)
    {
        // This is now just a notification that the round is complete
        // We'll handle subsequent actions after the sentence display finishes
        Debug.Log($"Round {currentRoundIndex + 1} complete. isLastRound: {isLastRound}");
    }
    
    private void HandleSentenceDisplayFinished(bool isLastRound)
    {
        // Now show appropriate response after the sentence display is finished
        if (isLastRound)
        {
            // This is the final round with the twist
            StartCoroutine(FinalTwistSequence());
        }
        else
        {
            // Regular round - start judge's hammer animation before dismissing defense
            StartCoroutine(JudgeHammerSequence());
        }
    }
    
    // Judge hammering animation between rounds
    private IEnumerator JudgeHammerSequence()
    {
        // Prepare for hammer hits
        int hammerHits = 3;
        
        for (int i = 0; i < hammerHits; i++)
        {
            // Change to hammer sprite
            if (judgeRenderer != null)
            {
                judgeRenderer.sprite = judgeHammerSprite;
            }
            
            // Play gavel sound
            PlaySound(gavelSoundName);
            
            // Shake the camera
            ShakeCamera(hammerShakeIntensity, hammerShakeDuration);
            
            // Wait a moment
            yield return new WaitForSeconds(hammerHitInterval);
            
            // Change back to normal sprite
            if (judgeRenderer != null)
            {
                judgeRenderer.sprite = judgeNormalSprite;
            }
            
            // Wait before next hit if not the last one
            if (i < hammerHits - 1)
            {
                yield return new WaitForSeconds(hammerHitInterval * 0.5f);
            }
        }
        
        // Show judge's dismissal text
        ShowChatBubble(judgeChatBubble, judgeChatText, judgeRegularResponse);
        
        // Wait before next round
        yield return new WaitForSeconds(pauseBeforeNextRound);
        
        // Hide judge chat bubble
        HideChatBubble(judgeChatBubble);
        
        // If we're using round transitions, play the transition animation
        if (useRoundTransition && roundTransitionImage != null)
        {
            yield return StartCoroutine(PlayRoundTransition());
        }
        
        // Start next round
        StartNextRound();
    }
    
    // Helper method to shake the camera
    private void ShakeCamera(float intensity, float duration)
    {
        if (mainCamera == null) return;
        
        // Store original position
        Vector3 originalPos = mainCamera.transform.position;
        
        // Create shake sequence
        Sequence shakeSeq = DOTween.Sequence();
        
        // Add random shake movements
        int shakeSteps = 10;
        for (int i = 0; i < shakeSteps; i++)
        {
            float progress = (float)i / shakeSteps;
            float decreaseFactor = 1f - progress; // Decrease intensity over time
            
            Vector3 randomOffset = new Vector3(
                Random.Range(-intensity, intensity) * decreaseFactor,
                Random.Range(-intensity, intensity) * decreaseFactor,
                0
            );
            
            shakeSeq.Append(mainCamera.transform.DOMove(
                originalPos + randomOffset, 
                duration / shakeSteps));
        }
        
        // Return to original position
        shakeSeq.Append(mainCamera.transform.DOMove(originalPos, duration / shakeSteps));
        
        // Play the sequence
        shakeSeq.Play();
    }
    
    private IEnumerator PlayRoundTransition()
    {
        // Setup the transition image
        RectTransform transitionRect = roundTransitionImage.GetComponent<RectTransform>();
        if (transitionRect == null) yield break;
        
        // Position the image off-screen to the left
        transitionRect.anchoredPosition = transitionStartPosition;
        roundTransitionImage.SetActive(true);
        
        // Play transition sound
        PlaySound(transitionSoundName);
        
        // Animate from left to center
        transitionRect.DOAnchorPos(transitionCenterPosition, transitionImageDuration * transitionImageInEaseFactor)
            .SetEase(Ease.OutQuint);
        
        // Wait while it's in the center
        yield return new WaitForSeconds(transitionImageDuration * (1 - transitionImageInEaseFactor - transitionImageOutEaseFactor));
        
        // Animate from center to right
        transitionRect.DOAnchorPos(transitionEndPosition, transitionImageDuration * transitionImageOutEaseFactor)
            .SetEase(Ease.InQuint);
        
        // Wait for animation to complete
        yield return new WaitForSeconds(transitionImageDuration * transitionImageOutEaseFactor);
        
        // Hide the image
        roundTransitionImage.SetActive(false);
    }
    
    private void HandleRoundFailed()
    {
        // Reset the scene (could implement a more elegant retry system)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private IEnumerator FinalTwistSequence()
    {
        // Play gasp sound
        PlaySound(gaspSoundName);
        
        // Change lawyer and jury to shocked sprites
        SetCharacterSprites(true);
        
        // Show lawyer's first shocked response (now using the separate bubble)
        if (lawyerShockedBubble != null && lawyerShockedText != null)
        {
            lawyerShockedText.text = lawyerShockedMessage;
            lawyerShockedBubble.SetActive(true);
        }
        
        // Wait for the first shocked bubble to be read
        yield return new WaitForSeconds(firstShockedBubbleDuration);
        
        // Hide first shocked bubble
        if (lawyerShockedBubble != null)
        {
            lawyerShockedBubble.SetActive(false);
        }
        
        // Show lawyer's second plea (using separate bubble)
        if (lawyerPleaBubble != null && lawyerPleaText != null)
        {
            lawyerPleaText.text = lawyerPleaMessage;
            lawyerPleaBubble.SetActive(true);
        }
        
        // Wait for the second shocked bubble to be read
        yield return new WaitForSeconds(secondShockedBubbleDuration);
        
        // Hide second plea bubble
        if (lawyerPleaBubble != null)
        {
            lawyerPleaBubble.SetActive(false);
        }
        
        // Stop all bounce animations
        StopAllBounces();
        
        // Start judge's hammer animation
        yield return StartCoroutine(JudgeFinalHammerHit());
        
        // Show judge's final sentence
        ShowChatBubble(judgeChatBubble, judgeChatText, judgeFinalSentence);
        
        // Wait longer before starting the fade
        yield return new WaitForSeconds(sentenceToFadeDelay);
        
        // Fade to black
        fadeCanvasGroup.DOFade(1, finalFadeDuration);
        
        yield return new WaitForSeconds(postFinalFadeDelay);
        
        // Transition to the next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // Load specified next scene
            SceneManager.LoadScene(nextSceneName);
        }
        else if (restartSceneOnEnd)
        {
            // Restart current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        // If neither is true, the scene just ends with black screen
    }
    
    // Special single hammer hit for final sentence
    private IEnumerator JudgeFinalHammerHit()
    {
        // Change to hammer sprite
        if (judgeRenderer != null)
        {
            judgeRenderer.sprite = judgeHammerSprite;
        }
        
        // Play gavel sound
        PlaySound(gavelSoundName);
        
        // Shake the camera with more intensity
        ShakeCamera(hammerShakeIntensity * 1.5f, hammerShakeDuration * 1.5f);
        
        // Wait a moment
        yield return new WaitForSeconds(hammerHitInterval);
        
        // Change back to normal sprite
        if (judgeRenderer != null)
        {
            judgeRenderer.sprite = judgeNormalSprite;
        }
    }
    
    private void ShowChatBubble(GameObject bubble, TextMeshProUGUI textComponent, string message)
    {
        if (bubble == null || textComponent == null) return;
        
        textComponent.text = message;
        bubble.SetActive(true);
    }
    
    private void HideChatBubble(GameObject bubble)
    {
        if (bubble == null) return;
        
        bubble.SetActive(false);
    }
    
    private void PlaySound(string soundName)
    {
        if (string.IsNullOrEmpty(soundName)) return;
        
        // Use the game's SoundManager to play the sound
        SoundManager soundManager = SoundManager.instance;
        if (soundManager != null)
        {
            soundManager.Play(soundName);
        }
        else
        {
            Debug.LogWarning("SoundManager not found! Cannot play sound: " + soundName);
            
            // Try the GameManager as fallback
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.PlaySound(soundName);
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up any remaining tweens
        StopAllBounces();
        DOTween.Kill(roundTransitionImage);
        DOTween.Kill(mainCamera.transform);
        
        // Remove event listeners
        if (typingSystem != null)
        {
            typingSystem.OnWordComplete -= HandleWordComplete;
            typingSystem.OnRoundComplete -= HandleRoundComplete;
            typingSystem.OnRoundFailed -= HandleRoundFailed;
            typingSystem.OnSentenceDisplayFinished -= HandleSentenceDisplayFinished;
            typingSystem.OnTimerUpdated -= UpdateBounceSpeedBasedOnTimer;
        }
    }
    
    // Public methods to control bouncing from other scripts if needed
    public void SetBounceStretchFactor(float stretchFactor)
    {
        bounceStretchFactor = stretchFactor;
        UpdateBounceParameters(bounceStretchFactor, bounceSquashFactor);
    }
    
    public void SetBounceSquashFactor(float squashFactor)
    {
        bounceSquashFactor = squashFactor;
        UpdateBounceParameters(bounceStretchFactor, bounceSquashFactor);
    }
    
    // Optional: Add characters to bounce list at runtime
    public void AddCharacterToBounce(Transform character)
    {
        if (character != null && !allBouncingCharacters.Contains(character))
        {
            allBouncingCharacters.Add(character);
            characterOriginalScales[character] = character.localScale;
            StartCharacterBounce(character);
        }
    }
    
    // Optional: Remove characters from bounce list at runtime
    public void RemoveCharacterFromBounce(Transform character)
    {
        if (character != null && allBouncingCharacters.Contains(character))
        {
            StopCharacterBounce(character);
            allBouncingCharacters.Remove(character);
            if (characterOriginalScales.ContainsKey(character))
            {
                characterOriginalScales.Remove(character);
            }
        }
    }
    
    // For debugging purposes
    public void ToggleSquashAndStretch()
    {
        squashAndStretch = !squashAndStretch;
        RestartAllBouncing();
    }
}
