using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CourtTypingSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI placeholderText;
    [SerializeField] private TextMeshProUGUI sentenceDisplay;
    [SerializeField] private Image radialTimer;
    [SerializeField] private RectTransform inputContainer;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField, Range(0f, 10f)] private float shakeIntensity = 5f;
    [SerializeField, Range(0.1f, 1f)] private float shakeDuration = 0.3f;
    [SerializeField, Range(5, 20)] private int shakeSteps = 10;
    
    [Header("Input Placeholder Settings")]
    [SerializeField, Range(0f, 1f)] private float placeholderOpacity = 0.5f;
    [SerializeField] private Color placeholderColor = new Color(0.5f, 0.5f, 0.5f);
    
    [Header("Timing Settings")]
    [SerializeField, Range(0.1f, 5f)] private float sentenceDisplayDuration = 1f;
    [SerializeField, Tooltip("If true, will use timePerWord from round config")]
    private bool useRoundTimerSettings = true;
    [SerializeField, Tooltip("Fallback timer if round has no timer setting")]
    private float defaultTimePerWord = 5f;
    
    [Header("Input Settings")]
    [SerializeField, Tooltip("Auto-submit when correct word is typed")]
    private bool autoSubmitCorrectWord = true;
    [SerializeField, Tooltip("Case sensitive matching")]
    private bool caseSensitiveMatching = false;
    
    [Header("Sound")]
    [SerializeField] private string correctSoundName = "correct";
    [SerializeField] private string incorrectSoundName = "incorrect";
    [SerializeField] private string timeoutSoundName = "timeout";
    
    [Header("On-Screen Text")]
    [SerializeField] private string defaultPlaceholderText = "Type the word...";
    
    // Events
    public event Action<string, bool> OnWordComplete;
    public event Action<bool> OnRoundComplete;
    public event Action OnRoundFailed;
    public event Action<bool> OnSentenceDisplayFinished;
    public event Action<float> OnTimerUpdated; // Sends normalized time remaining (0-1)
    
    // Current state
    private CourtRoundConfig currentRound;
    private int currentWordIndex;
    private List<string> typedWords = new List<string>();
    private Coroutine timerCoroutine;
    private bool isRoundActive = false;
    private float lastTypedTime;
    
    private void Start()
    {
        // Setup input field
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(OnInputChanged);
            inputField.onEndEdit.AddListener(OnInputSubmitted);
        }
        
        // Hide UI elements initially
        HideTypingUI();
    }
    
    public void StartRound(CourtRoundConfig round)
    {
        // Clear any previous state
        StopAllCoroutines();
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        currentRound = round;
        currentWordIndex = 0;
        typedWords.Clear();
        
        // Show typing UI
        ShowTypingUI();
        
        // Set placeholder to first word
        UpdatePlaceholder();
        
        // Focus input field
        inputField.text = "";
        inputField.ActivateInputField();
        
        // Reset and start timer
        if (radialTimer != null)
        {
            radialTimer.fillAmount = 1f;
            timerCoroutine = StartCoroutine(RunTimer());
        }
        
        isRoundActive = true;
    }
    
    private void UpdatePlaceholder()
    {
        if (placeholderText != null)
        {
            if (currentRound != null && currentWordIndex < currentRound.words.Length)
            {
                // Show the current word to type
                placeholderText.text = currentRound.words[currentWordIndex];
            }
            else
            {
                // Use default text if no word available
                placeholderText.text = defaultPlaceholderText;
            }
            
            // Apply placeholder color and opacity
            Color color = placeholderColor;
            color.a = placeholderOpacity;
            placeholderText.color = color;
        }
    }
    
    private void OnInputChanged(string text)
    {
        if (!isRoundActive) return;
        
        // Get the target word
        string targetWord = currentRound.words[currentWordIndex];
        
        // Check if input is correct so far (considering case sensitivity setting)
        bool isCorrectSoFar;
        if (caseSensitiveMatching)
        {
            isCorrectSoFar = text.Length <= targetWord.Length && targetWord.StartsWith(text);
        }
        else
        {
            isCorrectSoFar = text.Length <= targetWord.Length && 
                              targetWord.StartsWith(text, StringComparison.OrdinalIgnoreCase);
        }
        
        // Visual feedback
        inputField.textComponent.color = isCorrectSoFar ? correctColor : incorrectColor;
        
        // If incorrect and input changed from correct to incorrect, shake
        if (!isCorrectSoFar && text.Length > 0 && text.Length <= targetWord.Length)
        {
            // Only shake and play sound if we haven't just done so (avoid rapid firing)
            if (Time.time - lastTypedTime > 0.1f)
            {
                // Play error sound
                PlaySound(incorrectSoundName);
                
                // Shake effect
                ShakeInputField();
                
                lastTypedTime = Time.time;
            }
        }
        
        // Auto-submit if enabled and the word is fully correct
        if (autoSubmitCorrectWord && text.Length > 0)
        {
            bool isFullyCorrect;
            if (caseSensitiveMatching)
            {
                isFullyCorrect = text.Equals(targetWord);
            }
            else
            {
                isFullyCorrect = text.Equals(targetWord, StringComparison.OrdinalIgnoreCase);
            }
            
            if (isFullyCorrect)
            {
                // Use invoke to prevent modifying during callback
                Invoke("AutoSubmitWord", 0.05f);
            }
        }
    }
    
    private void AutoSubmitWord()
    {
        if (isRoundActive && inputField != null)
        {
            string text = inputField.text;
            OnInputSubmitted(text);
        }
    }
    
    private void OnInputSubmitted(string text)
    {
        if (!isRoundActive) return;
        
        string targetWord = currentRound.words[currentWordIndex];
        
        // Check if word is correct (considering case sensitivity)
        bool isCorrect;
        if (caseSensitiveMatching)
        {
            isCorrect = text.Equals(targetWord);
        }
        else
        {
            isCorrect = text.Equals(targetWord, StringComparison.OrdinalIgnoreCase);
        }
        
        // Special case for the final word in the final round (the twist)
        bool isFinalTwist = currentRound.isFinalRound && currentWordIndex == currentRound.words.Length - 1;
        
        if (isCorrect || isFinalTwist)
        {
            // For the final twist, we force "yaptım" regardless of what was typed
            string wordToAdd = isFinalTwist ? currentRound.overrideLastWord : text;
            typedWords.Add(wordToAdd);
            
            // Play success sound
            PlaySound(correctSoundName);
            
            // Notify word complete
            OnWordComplete?.Invoke(wordToAdd, isCorrect);
            
            // Move to next word or end round
            ProcessCorrectWord(isFinalTwist);
        }
        else
        {
            // Return focus to input field to try again
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }
    
    private void ProcessCorrectWord(bool isFinalTwist)
    {
        currentWordIndex++;
        
        // Check if this was the last word
        if (currentWordIndex >= currentRound.words.Length)
        {
            // Round is complete
            CompleteRound();
        }
        else
        {
            // Reset for next word
            inputField.text = "";
            UpdatePlaceholder();
            inputField.ActivateInputField();
            
            // Reset timer
            if (radialTimer != null)
            {
                StopCoroutine(timerCoroutine);
                radialTimer.fillAmount = 1f;
                timerCoroutine = StartCoroutine(RunTimer());
            }
        }
    }
    
    private void CompleteRound()
    {
        isRoundActive = false;
        
        // Stop timer
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        // First notify that round is complete
        OnRoundComplete?.Invoke(currentRound.isFinalRound);
        
        // Display full sentence
        ShowFullSentence();
    }
    
    private void FailRound()
    {
        isRoundActive = false;
        
        // Stop timer
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        // Play timeout sound
        PlaySound(timeoutSoundName);
        
        // Notify listeners
        OnRoundFailed?.Invoke();
        
        // Hide UI
        HideTypingUI();
    }
    
    private IEnumerator RunTimer()
    {
        // Get time allowed for this word
        float timeForWord = useRoundTimerSettings && currentRound != null ? 
            currentRound.timePerWord : defaultTimePerWord;
        
        float timeRemaining = timeForWord;
        
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            
            // Update radial fill
            if (radialTimer != null)
            {
                float normalizedTimeRemaining = Mathf.Clamp01(timeRemaining / timeForWord);
                radialTimer.fillAmount = normalizedTimeRemaining;
                
                // Notify listeners about timer update
                OnTimerUpdated?.Invoke(normalizedTimeRemaining);
            }
            
            yield return null;
        }
        
        // Time's up!
        FailRound();
    }
    
    private void ShowFullSentence()
    {
        if (sentenceDisplay != null)
        {
            sentenceDisplay.text = string.Join(" ", typedWords);
            sentenceDisplay.gameObject.SetActive(true);
            
            // Start timer to hide the sentence display
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(HideSentenceAfterDelay());
            }
        }
    }
    
    private IEnumerator HideSentenceAfterDelay()
    {
        // Use actual WaitForSeconds for reliability
        yield return new WaitForSeconds(sentenceDisplayDuration);
        
        // Hide the sentence display
        if (sentenceDisplay != null)
        {
            sentenceDisplay.gameObject.SetActive(false);
        }
        
        // Notify that sentence display is finished
        OnSentenceDisplayFinished?.Invoke(currentRound.isFinalRound);
        
        // Hide typing UI if we're done with all rounds
        if (currentRound.isFinalRound)
        {
            HideTypingUI();
        }
    }
    
    private void ShakeInputField()
    {
        if (inputContainer != null)
        {
            // Kill any existing shake animation
            inputContainer.DOKill(true);
            
            // Get the original position
            Vector3 originalPos = inputContainer.localPosition;
            
            // Create a shake sequence
            Sequence shakeSequence = DOTween.Sequence();
            
            // Add shake movements
            for (int i = 0; i < shakeSteps; i++)
            {
                float direction = (i % 2 == 0) ? 1 : -1;
                float intensity = shakeIntensity * (1f - (i / (float)shakeSteps)); // Reduce intensity over time
                
                shakeSequence.Append(inputContainer.DOLocalMoveX(
                    originalPos.x + (direction * intensity), 
                    shakeDuration / shakeSteps));
            }
            
            // Return to original position
            shakeSequence.Append(inputContainer.DOLocalMoveX(originalPos.x, shakeDuration / shakeSteps));
            
            // Play the sequence
            shakeSequence.Play();
        }
    }
    
    private void ShowTypingUI()
    {
        if (inputField != null) inputField.gameObject.SetActive(true);
        if (placeholderText != null) placeholderText.gameObject.SetActive(true);
        if (radialTimer != null) radialTimer.gameObject.SetActive(true);
        if (inputContainer != null) inputContainer.gameObject.SetActive(true);
        
        // Make sure sentence display is hidden
        if (sentenceDisplay != null) sentenceDisplay.gameObject.SetActive(false);
    }
    
    private void HideTypingUI()
    {
        if (inputField != null) inputField.gameObject.SetActive(false);
        if (placeholderText != null) placeholderText.gameObject.SetActive(false);
        if (radialTimer != null) radialTimer.gameObject.SetActive(false);
        if (sentenceDisplay != null) sentenceDisplay.gameObject.SetActive(false);
        if (inputContainer != null) inputContainer.gameObject.SetActive(false);
    }
    
    private void PlaySound(string soundName)
    {
        if (string.IsNullOrEmpty(soundName)) return;
        
        // Try the SoundManager first
        SoundManager soundManager = SoundManager.instance;
        if (soundManager != null)
        {
            soundManager.Play(soundName);
            return;
        }
        
        // Try the GameManager as fallback
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.PlaySound(soundName);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnInputChanged);
            inputField.onEndEdit.RemoveListener(OnInputSubmitted);
        }
    }
    
    // Public methods to adjust settings at runtime if needed
    
    public void SetCorrectColor(Color color)
    {
        correctColor = color;
    }
    
    public void SetIncorrectColor(Color color)
    {
        incorrectColor = color;
    }
    
    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = intensity;
    }
    
    public void SetShakeDuration(float duration)
    {
        shakeDuration = duration;
    }
    
    public void SetSentenceDisplayDuration(float duration)
    {
        sentenceDisplayDuration = duration;
    }
    
    public void SetDefaultTimePerWord(float time)
    {
        defaultTimePerWord = time;
    }
    
    public void SetUseRoundTimerSettings(bool useRoundSettings)
    {
        useRoundTimerSettings = useRoundSettings;
    }
    
    public void SetAutoSubmitCorrectWord(bool autoSubmit)
    {
        autoSubmitCorrectWord = autoSubmit;
    }
    
    public void SetCaseSensitiveMatching(bool caseSensitive)
    {
        caseSensitiveMatching = caseSensitive;
    }
}
