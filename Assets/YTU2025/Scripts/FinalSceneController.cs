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
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private float initialSwapDelay = 5f;
    [SerializeField] private float minSwapDelay = 0.1f;
    [SerializeField] private float swapDelayDecrement = 0.5f;
    [SerializeField] private int fastSwapLoopCount = 10; // Number of fast swaps at the end
    [SerializeField] private float fastSwapDelay = 0.05f; // Delay for fast swaps
    
    [Header("Camera Animation")]
    [SerializeField] private float zoomInDuration = 10f;
    [SerializeField] private float zoomOutDuration = 5f;
    [SerializeField] private float zoomTargetScale = 3f;
    [SerializeField] private Vector2 zoomTargetPosition = new Vector2(0, 50); // Position to zoom into (face)
    
    [Header("Audio")]
    [SerializeField] private string backgroundMusicName = "FinalMusic";
    [SerializeField] private float musicFadeOutDuration = 3f;
    [SerializeField] private string finalAudioCueName = "FinalCue";
    
    [Header("UI References")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI variantText;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private RectTransform zoomContainer; // Container that will be scaled/moved
    
    private bool isAnimating = false;
    private int currentVariantIndex = 0;
    private float currentSwapDelay;
    
    void Start()
    {
        // Initialize
        currentSwapDelay = initialSwapDelay;
        
        // Hide variant text initially
        if (variantText != null)
        {
            variantText.gameObject.SetActive(false);
        }
        
        // Ensure fade group is clear
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
        }
        
        // Start by hiding character and background
        if (characterImage != null)
        {
            characterImage.color = new Color(1, 1, 1, 0);
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(1, 1, 1, 0);
        }
        
        // Save original position and scale of the zoom container
        if (zoomContainer != null)
        {
            zoomContainer.localScale = Vector3.one;
        }
        
        // Start the sequence
        StartCoroutine(FinalSequence());
    }
    
    IEnumerator FinalSequence()
    {
        if (isAnimating || characterVariants.Count == 0) yield break;
        
        isAnimating = true;
        
        // Initial delay
        yield return new WaitForSeconds(initialDelay);
        
        // Play background music
        if (!string.IsNullOrEmpty(backgroundMusicName) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(backgroundMusicName);
        }
        
        // Show initial character and background with fade
        if (characterImage != null && backgroundImage != null && characterVariants.Count > 0)
        {
            // Set initial sprites
            characterImage.sprite = characterVariants[0].characterSprite;
            backgroundImage.sprite = characterVariants[0].backgroundSprite;
            
            // Fade in
            characterImage.DOFade(1f, 1f);
            backgroundImage.DOFade(1f, 1f);
            
            // Show initial text if specified
            if (variantText != null && !string.IsNullOrEmpty(characterVariants[0].variantText))
            {
                variantText.text = characterVariants[0].variantText;
                variantText.gameObject.SetActive(true);
                variantText.alpha = 0f;
                variantText.DOFade(1f, 1f);
            }
        }
        
        // Initiate camera zoom in
        if (zoomContainer != null)
        {
            // Start zooming in slowly
            Sequence zoomSequence = DOTween.Sequence();
            
            // Zoom in to face
            zoomSequence.Append(zoomContainer.DOScale(zoomTargetScale, zoomInDuration).SetEase(Ease.InOutQuad));
            zoomSequence.Join(zoomContainer.DOAnchorPos(zoomTargetPosition, zoomInDuration).SetEase(Ease.InOutQuad));
            
            // After zoom in completes, zoom out quickly
            zoomSequence.Append(zoomContainer.DOScale(Vector3.one, zoomOutDuration).SetEase(Ease.OutQuad));
            zoomSequence.Join(zoomContainer.DOAnchorPos(Vector2.zero, zoomOutDuration).SetEase(Ease.OutQuad));
            
            // Play the sequence
            zoomSequence.Play();
        }
        
        // Wait before starting the swapping
        yield return new WaitForSeconds(initialSwapDelay);
        
        // Begin the sprite swapping sequence
        StartCoroutine(SwapSequence());
    }
    
    IEnumerator SwapSequence()
    {
        if (characterVariants.Count <= 1) yield break;
        
        // First part - gradually speeding up swaps
        while (currentSwapDelay > minSwapDelay)
        {
            // Swap to the next variant
            currentVariantIndex = (currentVariantIndex + 1) % characterVariants.Count;
            
            // Update sprites
            if (characterImage != null && backgroundImage != null)
            {
                // Apply with a quick fade transition
                characterImage.DOFade(0.5f, currentSwapDelay * 0.3f).OnComplete(() => {
                    characterImage.sprite = characterVariants[currentVariantIndex].characterSprite;
                    characterImage.DOFade(1f, currentSwapDelay * 0.3f);
                });
                
                backgroundImage.DOFade(0.8f, currentSwapDelay * 0.3f).OnComplete(() => {
                    backgroundImage.sprite = characterVariants[currentVariantIndex].backgroundSprite;
                    backgroundImage.DOFade(1f, currentSwapDelay * 0.3f);
                });
                
                // Update text if specified
                if (variantText != null)
                {
                    if (!string.IsNullOrEmpty(characterVariants[currentVariantIndex].variantText))
                    {
                        variantText.DOFade(0f, currentSwapDelay * 0.3f).OnComplete(() => {
                            variantText.text = characterVariants[currentVariantIndex].variantText;
                            variantText.gameObject.SetActive(true);
                            variantText.DOFade(1f, currentSwapDelay * 0.3f);
                        });
                    }
                    else
                    {
                        variantText.DOFade(0f, currentSwapDelay * 0.3f).OnComplete(() => {
                            variantText.gameObject.SetActive(false);
                        });
                    }
                }
            }
            
            // Wait for the current swap delay
            yield return new WaitForSeconds(currentSwapDelay);
            
            // Decrease the delay for the next swap
            currentSwapDelay -= swapDelayDecrement;
        }
        
        // Second part - very fast swaps at the end
        for (int i = 0; i < fastSwapLoopCount; i++)
        {
            // Swap to the next variant quickly
            currentVariantIndex = (currentVariantIndex + 1) % characterVariants.Count;
            
            // Update sprites without animation for speed
            if (characterImage != null && backgroundImage != null)
            {
                characterImage.sprite = characterVariants[currentVariantIndex].characterSprite;
                backgroundImage.sprite = characterVariants[currentVariantIndex].backgroundSprite;
            }
            
            // Very short delay between fast swaps
            yield return new WaitForSeconds(fastSwapDelay);
        }
        
        // After the swapping sequence, initiate the final fade out
        StartCoroutine(FinalFadeOut());
    }
    
    IEnumerator FinalFadeOut()
    {
        // Wait for the zoom out to complete
        // Calculate when the zoom animation should be finishing
        float waitTime = initialDelay + initialSwapDelay + zoomInDuration + zoomOutDuration - (Time.time - Time.timeSinceLevelLoad);
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }
        
        // Fade out background music if playing
        if (!string.IsNullOrEmpty(backgroundMusicName) && SoundManager.instance != null)
        {
            SoundManager.instance.FadeOut(backgroundMusicName, musicFadeOutDuration);
        }
        
        // Fade to black
        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true);
            fadeGroup.DOFade(1f, 2f).OnComplete(() => {
                // Play the final audio cue
                if (!string.IsNullOrEmpty(finalAudioCueName) && SoundManager.instance != null)
                {
                    SoundManager.instance.Play(finalAudioCueName);
                    
                    // Wait for the audio to play before potentially ending the game
                    if (SoundManager.instance.IsPlaying(finalAudioCueName))
                    {
                        // Get the length of the audio clip
                        float clipLength = 0f;
                        SoundManager.Sound finalSound = System.Array.Find(SoundManager.instance.sounds, 
                            sound => sound.name == finalAudioCueName);
                        
                        if (finalSound != null && finalSound.clip != null)
                        {
                            clipLength = finalSound.clip.length;
                        }
                        
                        // Add slight delay to ensure clip finishes
                        StartCoroutine(WaitAndEnd(clipLength + 0.5f));
                    }
                    else
                    {
                        // If we can't determine if it's playing, just add a reasonable delay
                        StartCoroutine(WaitAndEnd(3f));
                    }
                }
                else
                {
                    // No audio cue, just end after a short delay
                    StartCoroutine(WaitAndEnd(2f));
                }
            });
        }
    }
    
    IEnumerator WaitAndEnd(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Here you could either:
        // 1. Load a credits scene
        // 2. Return to the main menu
        // 3. Quit the application
        
        // For example, to return to a main menu:
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        
        // Or to quit (in standalone builds):
        #if UNITY_STANDALONE
        Application.Quit();
        #endif
    }
}
