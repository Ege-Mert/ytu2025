using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class FinalSceneController : MonoBehaviour
{
    [System.Serializable]
    public class CharacterVariant
    {
        public Sprite characterSprite;
        public Sprite backgroundSprite;
        [Tooltip("Optional - text that appears during this variant")]
        public string variantText;
    }
    
    [Header("Scene Setup")]
    [SerializeField] private List<CharacterVariant> characterVariants = new List<CharacterVariant>();
    [SerializeField] private float initialDelay = 0.5f; // Short delay before everything starts
    [SerializeField] private float swapStartDelay = 2f; // Delay before swapping starts (zoom starts immediately)
    [SerializeField] private float minSwapDelay = 0.1f;
    [SerializeField] private float swapDelayDecrement = 0.5f;
    [SerializeField] private float fastSwapDuration = 1.5f; // Total duration for fast swaps
    [SerializeField] private float fastSwapDelay = 0.05f; // Delay between each fast swap
    
    [Header("Zoom Settings")]
    [SerializeField] private bool zoomInToFace = true; // If true, zoom into face; if false, zoom out from face
    [SerializeField] private float characterZoomDuration = 10f; // Character zoom duration
    [SerializeField] private float backgroundZoomDuration = 12f; // Background zoom duration (slightly slower)
    [SerializeField] private Vector2 startPosition = Vector2.zero; // Starting position
    [SerializeField] private Vector2 facePosition = new Vector2(0, 50); // Face position
    [SerializeField] private Vector2 normalSize = new Vector2(800, 600); // Normal size
    [SerializeField] private Vector2 zoomedSize = new Vector2(1200, 900); // Zoomed size
    
    [Header("Ending Sequence")]
    [SerializeField] private float waitAfterZoom = 2.0f; // Time to wait after zoom completes
    [SerializeField] private float blackFadeDuration = 2.0f; // How long the screen takes to fade to black
    [SerializeField] private float finalAudioDelay = 0.5f; // Delay before playing final audio
    [SerializeField] private float endingDelay = 3.0f; // Time to wait after final audio before ending
    
    [Header("Audio")]
    [SerializeField] private string backgroundMusicName = "FinalMusic";
    [SerializeField] private float musicFadeOutDuration = 3f;
    [SerializeField] private string finalAudioCueName = "FinalCue";
    
    [Header("UI References")]
    [SerializeField] private RectTransform characterRect; // Character's RectTransform
    [SerializeField] private RectTransform backgroundRect; // Background's RectTransform
    [SerializeField] private Image characterImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI variantText;
    [SerializeField] private CanvasGroup blackScreenFade; // Black screen for fade out
    
    private bool isAnimating = false;
    private int currentVariantIndex = 0;
    private float currentSwapDelay;
    
    void Start()
    {
        // Initialize
        currentSwapDelay = swapStartDelay;
        
        // Hide variant text initially
        if (variantText != null)
        {
            variantText.gameObject.SetActive(false);
        }
        
        // Ensure fade group is clear
        if (blackScreenFade != null)
        {
            blackScreenFade.alpha = 0f;
            blackScreenFade.gameObject.SetActive(true);
        }
        
        // Start the sequence
        StartCoroutine(FinalSequence());
    }
    
    IEnumerator FinalSequence()
    {
        if (isAnimating || characterVariants.Count == 0) yield break;
        
        isAnimating = true;
        
        Debug.Log("Starting final sequence");
        
        // Very short initial delay
        yield return new WaitForSeconds(initialDelay);
        
        // Play background music
        if (!string.IsNullOrEmpty(backgroundMusicName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(backgroundMusicName);
        }
        
        // Show initial character and background immediately
        if (characterImage != null && backgroundImage != null && characterVariants.Count > 0)
        {
            // Set initial sprites
            characterImage.sprite = characterVariants[0].characterSprite;
            backgroundImage.sprite = characterVariants[0].backgroundSprite;
            
            // Make sure they're fully visible
            characterImage.color = Color.white;
            backgroundImage.color = Color.white;
            
            // Show initial text if specified
            if (variantText != null && !string.IsNullOrEmpty(characterVariants[0].variantText))
            {
                variantText.text = characterVariants[0].variantText;
                variantText.gameObject.SetActive(true);
                variantText.alpha = 1f;
            }
        }
        
        // Set up initial position and size for character and background
        if (zoomInToFace)
        {
            // Starting normal, zooming to face
            if (characterRect != null)
            {
                characterRect.sizeDelta = normalSize;
                characterRect.anchoredPosition = startPosition;
            }
            
            if (backgroundRect != null)
            {
                backgroundRect.sizeDelta = normalSize;
                backgroundRect.anchoredPosition = startPosition;
            }
        }
        else
        {
            // Starting zoomed in, going back to normal
            if (characterRect != null)
            {
                characterRect.sizeDelta = zoomedSize;
                characterRect.anchoredPosition = facePosition;
            }
            
            if (backgroundRect != null)
            {
                backgroundRect.sizeDelta = zoomedSize;
                backgroundRect.anchoredPosition = facePosition;
            }
        }
        
        Debug.Log("Starting zoom animation immediately");
        
        // Start the zoom animations with different durations - IMMEDIATELY
        if (zoomInToFace)
        {
            // Zoom in to face (character)
            if (characterRect != null)
            {
                characterRect.DOSizeDelta(zoomedSize, characterZoomDuration).SetEase(Ease.InOutQuad);
                characterRect.DOAnchorPos(facePosition, characterZoomDuration).SetEase(Ease.InOutQuad);
            }
            
            // Zoom in to face (background - slightly slower)
            if (backgroundRect != null)
            {
                backgroundRect.DOSizeDelta(zoomedSize, backgroundZoomDuration).SetEase(Ease.InOutQuad);
                backgroundRect.DOAnchorPos(facePosition, backgroundZoomDuration).SetEase(Ease.InOutQuad);
            }
        }
        else
        {
            // Zoom out from face (character)
            if (characterRect != null)
            {
                characterRect.DOSizeDelta(normalSize, characterZoomDuration).SetEase(Ease.InOutQuad);
                characterRect.DOAnchorPos(startPosition, characterZoomDuration).SetEase(Ease.InOutQuad);
            }
            
            // Zoom out from face (background - slightly slower)
            if (backgroundRect != null)
            {
                backgroundRect.DOSizeDelta(normalSize, backgroundZoomDuration).SetEase(Ease.InOutQuad);
                backgroundRect.DOAnchorPos(startPosition, backgroundZoomDuration).SetEase(Ease.InOutQuad);
            }
        }
        
        // Wait before starting swapping (separate from zoom start)
        yield return new WaitForSeconds(swapStartDelay);
        
        Debug.Log("Starting character swap sequence");
        
        // Start the swap sequence
        StartCoroutine(SwapSequence());
        
        // Calculate total sequence time
        float longerZoomDuration = Mathf.Max(characterZoomDuration, backgroundZoomDuration);
        
        // Wait for the zoom to complete
        float remainingZoomTime = longerZoomDuration - swapStartDelay;
        if (remainingZoomTime > 0)
        {
            yield return new WaitForSeconds(remainingZoomTime);
        }
        
        // Wait additional time after zoom
        yield return new WaitForSeconds(waitAfterZoom);
        
        Debug.Log("Starting final fade sequence");
        
        // Begin final fade out sequence
        StartCoroutine(EndSequence());
    }
    
    IEnumerator SwapSequence()
    {
        if (characterVariants.Count <= 1) yield break;
        
        // First part - gradually speeding up swaps
        while (currentSwapDelay > minSwapDelay)
        {
            // Swap to the next variant
            currentVariantIndex = (currentVariantIndex + 1) % characterVariants.Count;
            
            // Update sprites instantly
            if (characterImage != null && backgroundImage != null)
            {
                // Instant swap - no fade
                characterImage.sprite = characterVariants[currentVariantIndex].characterSprite;
                backgroundImage.sprite = characterVariants[currentVariantIndex].backgroundSprite;
                
                // Update text if specified - also instant
                if (variantText != null)
                {
                    if (!string.IsNullOrEmpty(characterVariants[currentVariantIndex].variantText))
                    {
                        variantText.text = characterVariants[currentVariantIndex].variantText;
                        variantText.gameObject.SetActive(true);
                    }
                    else
                    {
                        variantText.gameObject.SetActive(false);
                    }
                }
            }
            
            // Wait for the current swap delay
            yield return new WaitForSeconds(currentSwapDelay);
            
            // Decrease the delay for the next swap
            currentSwapDelay -= swapDelayDecrement;
        }
        
        // Calculate how many fast swaps to do based on the fastSwapDuration
        int fastSwapCount = Mathf.FloorToInt(fastSwapDuration / fastSwapDelay);
        
        Debug.Log("Starting fast swap sequence, count: " + fastSwapCount);
        
        // Second part - very fast swaps at the end
        for (int i = 0; i < fastSwapCount; i++)
        {
            // Swap to the next variant quickly
            currentVariantIndex = (currentVariantIndex + 1) % characterVariants.Count;
            
            // Update sprites instantly without animation
            if (characterImage != null && backgroundImage != null)
            {
                characterImage.sprite = characterVariants[currentVariantIndex].characterSprite;
                backgroundImage.sprite = characterVariants[currentVariantIndex].backgroundSprite;
            }
            
            // Very short delay between fast swaps
            yield return new WaitForSeconds(fastSwapDelay);
        }
    }
    
    IEnumerator EndSequence()
    {
        Debug.Log("Fading out music");
        
        // Fade out background music if playing
        if (!string.IsNullOrEmpty(backgroundMusicName) && SoundManager.instance != null)
        {
            SoundManager.instance.FadeOut(backgroundMusicName, musicFadeOutDuration);
        }
        
        Debug.Log("Fading to black, duration: " + blackFadeDuration);
        
        // Fade the screen to black
        if (blackScreenFade != null)
        {
            // Make sure it's active and reset alpha
            blackScreenFade.gameObject.SetActive(true);
            blackScreenFade.alpha = 0f;
            
            // Fade to black
            Tween fadeTween = blackScreenFade.DOFade(1f, blackFadeDuration);
            
            // Wait for fade to complete
            yield return fadeTween.WaitForCompletion();
            
            Debug.Log("Black fade complete");
        }
        else
        {
            Debug.LogWarning("No black screen fade canvas group assigned!");
            yield return new WaitForSeconds(blackFadeDuration);
        }
        
        // Small delay before final audio
        yield return new WaitForSeconds(finalAudioDelay);
        
        // Play final audio cue
        if (!string.IsNullOrEmpty(finalAudioCueName) && SoundManager.instance != null)
        {
            Debug.Log("Playing final audio cue");
            SoundManager.instance.Play(finalAudioCueName);
            
            // Try to get the length of the audio clip
            float audioLength = endingDelay;
            if (SoundManager.instance.sounds != null)
            {
                SoundManager.Sound finalSound = System.Array.Find(SoundManager.instance.sounds, 
                    sound => sound.name == finalAudioCueName);
                
                if (finalSound != null && finalSound.clip != null)
                {
                    audioLength = finalSound.clip.length + 0.5f;
                    Debug.Log("Audio length: " + audioLength);
                }
            }
            
            // Wait for the audio to finish
            yield return new WaitForSeconds(audioLength);
        }
        else
        {
            // No audio cue, just wait the default ending delay
            yield return new WaitForSeconds(endingDelay);
        }
        
        Debug.Log("End sequence complete");
        
        // End the game or return to menu
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_STANDALONE
        Application.Quit();
        #endif
    }
}
