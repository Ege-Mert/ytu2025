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
    
    [Header("Chat Bubbles")]
    [SerializeField] private GameObject lawyerChatBubble;
    [SerializeField] private TextMeshProUGUI lawyerChatText;
    [SerializeField] private GameObject judgeChatBubble;
    [SerializeField] private TextMeshProUGUI judgeChatText;
    
    [Header("Dialogue Text")]
    [SerializeField, Tooltip("First dialogue from lawyer")]
    private string lawyerIntroText = "Repeat after me.";
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
    [SerializeField, Tooltip("Delay after gasp before judge's final sentence")]
    private float gaspToSentenceDelay = 0.8f;
    [SerializeField, Tooltip("Final fade to black duration")]
    private float finalFadeDuration = 1.0f;
    [SerializeField, Tooltip("Delay after fade to black before scene transition")]
    private float postFinalFadeDelay = 1.0f;
    
    [Header("Character Animation")]
    [SerializeField] private float bounceAmplitude = 0.2f;
    [SerializeField] private float bounceInterval = 0.5f;
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
    private Dictionary<Transform, Tween> characterBounces = new Dictionary<Transform, Tween>();
    private Dictionary<Transform, Vector3> characterStartPositions = new Dictionary<Transform, Vector3>();
    
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
        
        // Hide transition image if it exists
        if (roundTransitionImage != null)
        {
            roundTransitionImage.SetActive(false);
        }
        
        // Start with the scene fade-in sequence
        fadeCanvasGroup.alpha = 1f;
        StartCoroutine(IntroSequence());
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
        
        // Store starting positions
        foreach (Transform character in allBouncingCharacters)
        {
            if (character != null)
            {
                characterStartPositions[character] = character.position;
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
    
    // Method to start bouncing for a single character
    private void StartCharacterBounce(Transform characterTransform)
    {
        if (characterTransform == null) return;
        
        // Stop any existing bounce
        StopCharacterBounce(characterTransform);
        
        // Get start position (use stored position or current if not stored)
        Vector3 startPos = characterStartPositions.ContainsKey(characterTransform) ? 
            characterStartPositions[characterTransform] : characterTransform.position;
        
        // Create the bounce tween
        Tween bounceTween = characterTransform.DOMoveY(
            startPos.y + bounceAmplitude, 
            bounceInterval / 2)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId("CourtBounce");
        
        // Store the tween for later access
        characterBounces[characterTransform] = bounceTween;
    }
    
    // Method to stop a single character from bouncing
    public void StopCharacterBounce(Transform characterTransform)
    {
        if (characterTransform == null) return;
        
        // If we have an active tween for this character, kill it
        if (characterBounces.ContainsKey(characterTransform) && 
            characterBounces[characterTransform] != null && 
            characterBounces[characterTransform].IsActive())
        {
            characterBounces[characterTransform].Kill();
            characterBounces.Remove(characterTransform);
        }
        
        // Reset to starting position if we know it
        if (characterStartPositions.ContainsKey(characterTransform))
        {
            characterTransform.position = characterStartPositions[characterTransform];
        }
    }
    
    // Method to stop all characters from bouncing
    public void StopAllBounces()
    {
        // Kill all tweens with our identifier
        DOTween.Kill("CourtBounce");
        
        // Reset all characters to their starting positions
        foreach (Transform character in allBouncingCharacters)
        {
            if (character != null && characterStartPositions.ContainsKey(character))
            {
                character.position = characterStartPositions[character];
            }
        }
        
        // Clear the collection
        characterBounces.Clear();
    }
    
    // Method to adjust bounce amplitude and interval for all characters
    public void AdjustBounceParameters(float newAmplitude, float newInterval)
    {
        bounceAmplitude = newAmplitude;
        bounceInterval = newInterval;
        
        // Restart all bounces with new parameters
        StopAllBounces();
        StartAllCharactersBouncing();
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
        
        // Configure and start the typing system for this round
        typingSystem.StartRound(round);
    }
    
    private void HandleWordComplete(string word, bool isCorrect)
    {
        // This could play feedback sounds, animations, etc.
        // For now, just logging
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
        // Now show judge's response after the sentence display is finished
        if (isLastRound)
        {
            // This is the final round with the twist
            StartCoroutine(FinalRoundSequence());
        }
        else
        {
            // Regular round - show judge's dismissal text
            ShowChatBubble(judgeChatBubble, judgeChatText, judgeRegularResponse);
            
            // Wait before next round
            StartCoroutine(WaitForNextRound());
        }
    }
    
    private IEnumerator WaitForNextRound()
    {
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
    
    private IEnumerator FinalRoundSequence()
    {
        // Stop all bounce animations
        StopAllBounces();
        
        // Play gasp sound
        PlaySound(gaspSoundName);
        
        // TODO: Swap lawyer & jurors to shocked sprites
        // This would be handled here if you have sprite swap functionality
        
        yield return new WaitForSeconds(gaspToSentenceDelay);
        
        // Show judge's final sentence
        ShowChatBubble(judgeChatBubble, judgeChatText, judgeFinalSentence);
        
        // Play gavel sound
        PlaySound(gavelSoundName);
        
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
        
        // Remove event listeners
        if (typingSystem != null)
        {
            typingSystem.OnWordComplete -= HandleWordComplete;
            typingSystem.OnRoundComplete -= HandleRoundComplete;
            typingSystem.OnRoundFailed -= HandleRoundFailed;
            typingSystem.OnSentenceDisplayFinished -= HandleSentenceDisplayFinished;
        }
    }
    
    // Public methods to control bouncing from other scripts if needed
    public void SetBounceAmplitude(float amplitude)
    {
        bounceAmplitude = amplitude;
        
        // Apply to active bounces
        if (characterBounces.Count > 0)
        {
            AdjustBounceParameters(amplitude, bounceInterval);
        }
    }
    
    public void SetBounceInterval(float interval)
    {
        bounceInterval = interval;
        
        // Apply to active bounces
        if (characterBounces.Count > 0)
        {
            AdjustBounceParameters(bounceAmplitude, interval);
        }
    }
    
    // Optional: Add characters to bounce list at runtime
    public void AddCharacterToBounce(Transform character)
    {
        if (character != null && !allBouncingCharacters.Contains(character))
        {
            allBouncingCharacters.Add(character);
            characterStartPositions[character] = character.position;
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
            if (characterStartPositions.ContainsKey(character))
            {
                characterStartPositions.Remove(character);
            }
        }
    }
}
