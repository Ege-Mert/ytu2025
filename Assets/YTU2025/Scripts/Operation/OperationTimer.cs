using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class OperationTimer : MonoBehaviour
{
    [Header("Timer References")]
    [SerializeField] private Image timerFillImage;    // Radial fill image
    
    [Header("Timer Appearance")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float pulseSpeed = 2f;    // Speed of warning pulse
    
    // Events
    public event Action OnTimerComplete;
    public event Action<float> OnTimerTick;    // Sends current time remaining percentage
    
    // Private variables
    private float maxTime;                 // Total time for the operation
    private float currentTime;             // Current time remaining
    private float warningThreshold;        // When to start warning (percentage)
    private bool isRunning = false;
    private bool isWarning = false;
    private Camera mainCamera;
    private OperationManager operationManager;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        operationManager = FindObjectOfType<OperationManager>();
    }
    
    private void Start()
    {
        // Get timer settings from the operation config
        if (operationManager != null && operationManager.Config != null)
        {
            maxTime = operationManager.Config.operationTime;
            warningThreshold = operationManager.Config.warningThreshold;
        }
        else
        {
            // Default values if config not found
            maxTime = 30f;
            warningThreshold = 0.2f;
        }
        
        // Initialize the timer
        currentTime = maxTime;
        UpdateFillAmount(1);
        
        // Initialize appearance
        if (timerFillImage != null)
        {
            timerFillImage.color = normalColor;
        }
    }
    
    private void Update()
    {
        if (!isRunning) return;
        
        // Update the timer
        currentTime -= Time.deltaTime;
        
        // Clamp to 0
        currentTime = Mathf.Max(0, currentTime);
        
        // Calculate fill percentage
        float fillPercentage = currentTime / maxTime;
        
        // Update the fill amount
        UpdateFillAmount(fillPercentage);
        
        // Check for warning threshold
        if (fillPercentage <= warningThreshold && !isWarning)
        {
            StartWarning();
        }
        
        // Invoke the tick event
        OnTimerTick?.Invoke(fillPercentage);
        
        // Check for timer completion
        if (currentTime <= 0)
        {
            isRunning = false;
            OnTimerComplete?.Invoke();
        }
    }
    
    private void UpdateFillAmount(float percentage)
    {
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = percentage;
        }
    }
    
    public void StartTimer()
    {
        isRunning = true;
    }
    
    public void PauseTimer()
    {
        isRunning = false;
    }
    
    public void ResetTimer()
    {
        currentTime = maxTime;
        isRunning = false;
        isWarning = false;
        
        // Reset appearance
        if (timerFillImage != null)
        {
            timerFillImage.color = normalColor;
            UpdateFillAmount(1);
            
            // Stop any running animations
            timerFillImage.transform.DOKill();
        }
    }
    
    public void DeductTime(float amount)
    {
        currentTime -= amount;
        currentTime = Mathf.Max(0, currentTime);
        
        // Show screen shake effect for penalty
        if (mainCamera != null && operationManager.Config != null)
        {
            mainCamera.transform.DOShakePosition(
                operationManager.Config.screenShakeDuration, 
                operationManager.Config.screenShakeAmount
            );
        }
    }
    
    private void StartWarning()
    {
        isWarning = true;
        
        // Change timer color
        if (timerFillImage != null)
        {
            // Pulse the timer color between normal and warning
            timerFillImage.DOColor(warningColor, 0.5f / pulseSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
            // Add a subtle pulse scale effect
            timerFillImage.transform.DOScale(1.05f, 0.5f / pulseSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
            // Add screen shake
            if (mainCamera != null && operationManager.Config != null)
            {
                DOTween.Sequence()
                    .AppendInterval(1f)
                    .AppendCallback(() => {
                        if (isWarning && isRunning) {
                            mainCamera.transform.DOShakePosition(
                                operationManager.Config.screenShakeDuration, 
                                operationManager.Config.screenShakeAmount * 0.7f
                            );
                        }
                    })
                    .SetLoops(-1);
            }
        }
    }
    
    public float GetRemainingTime()
    {
        return currentTime;
    }
    
    public float GetRemainingPercentage()
    {
        return currentTime / maxTime;
    }
}
