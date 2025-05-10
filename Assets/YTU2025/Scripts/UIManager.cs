using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private RectTransform gunContainer; // Parent container for gun image
    
    [Header("UI Animation Settings")]
    [SerializeField] private float gunRecoilAmount = 30f;
    [SerializeField] private float gunRecoilDuration = 0.1f;
    [SerializeField] private float gunReturnDuration = 0.2f;
    
    [Header("Fade Settings")]
    [SerializeField] private bool startWithBlackScreen = false;
    [SerializeField] private float initialFadeInDuration = 1.5f;
    
    private Vector2 initialGunPosition;
    
    void Start()
    {
        // Store initial gun position if available
        if (gunContainer != null)
        {
            initialGunPosition = gunContainer.anchoredPosition;
        }
        
        // Setup the fade image (black overlay)
        if (fadeImage != null)
        {
            // Set initial state
            Color fadeColor = fadeImage.color;
            fadeColor.a = startWithBlackScreen ? 1f : 0f;
            fadeImage.color = fadeColor;
            fadeImage.gameObject.SetActive(true);
            
            // If starting with black screen, fade in
            if (startWithBlackScreen)
            {
                FadeFromBlack(initialFadeInDuration);
            }
        }
        
        // If in transition scene, hide ammo and gun
        if (IsTransitionScene())
        {
            if (ammoText != null) ammoText.gameObject.SetActive(false);
            if (gunContainer != null) gunContainer.gameObject.SetActive(false);
        }
    }
    
    // Simple check to see if we're in the transition scene
    private bool IsTransitionScene()
    {
        return FindObjectOfType<TransitionSceneController>() != null;
    }
    
    public void UpdateAmmoUI(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{current}/{max}";
        }
    }
    
    public void TriggerGunRecoil()
    {
        if (gunContainer == null) return;
        
        // Apply quick recoil animation
        Sequence recoilSequence = DOTween.Sequence();
        
        // Move back (recoil)
        recoilSequence.Append(gunContainer.DOAnchorPosY(initialGunPosition.y - gunRecoilAmount, gunRecoilDuration)
            .SetEase(Ease.OutQuad));
        
        // Return to position
        recoilSequence.Append(gunContainer.DOAnchorPosY(initialGunPosition.y, gunReturnDuration)
            .SetEase(Ease.OutBack));
    }
    
    public void FadeToBlack(float duration, System.Action onComplete = null)
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image not assigned in UIManager!");
            
            // Still call the complete action if it was provided
            if (onComplete != null)
            {
                onComplete.Invoke();
            }
            
            return;
        }
        
        // Disable gun controls during fade
        GunController gunController = FindObjectOfType<GunController>();
        if (gunController != null)
        {
            gunController.SetCanShoot(false);
        }
        
        // Make sure the fade image is active
        fadeImage.gameObject.SetActive(true);
        
        // Fade to black - setting update mode to make it work with timescale changes
        fadeImage.DOFade(1, duration)
            .SetUpdate(true)
            .OnComplete(() => {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }
            });
    }
    
    public void FadeFromBlack(float duration, System.Action onComplete = null)
    {
        if (fadeImage == null) return;
        
        // Make sure the fade image is active and visible
        fadeImage.gameObject.SetActive(true);
        Color startColor = fadeImage.color;
        startColor.a = 1;
        fadeImage.color = startColor;
        
        // Fade from black - setting update mode to make it work with timescale changes
        fadeImage.DOFade(0, duration)
            .SetUpdate(true)
            .OnComplete(() => {
                // Re-enable gun controls after fade (if in forest scene)
                if (!IsTransitionScene())
                {
                    GunController gunController = FindObjectOfType<GunController>();
                    if (gunController != null)
                    {
                        gunController.SetCanShoot(true);
                    }
                }
                
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }
            });
    }
    
    public RectTransform GetGunContainer()
    {
        return gunContainer;
    }
    
    // Method to hide/show the gun UI
    public void SetGunVisible(bool visible)
    {
        if (gunContainer != null)
        {
            gunContainer.gameObject.SetActive(visible);
        }
    }
}
