using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GunController : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private RectTransform gunImage; // Changed to RectTransform for UI
    [SerializeField] private float swayAmount = 5f;
    [SerializeField] private float swayCycleDuration = 0.5f;
    [SerializeField] private float movementSwayMultiplier = 2f; // New multiplier for movement intensity
    [SerializeField] private float reloadDuration = 1.0f;
    [SerializeField] private float reloadDownAmount = 300f; // Changed to UI units (pixels)
    [SerializeField] private float screenShakeAmount = 0.2f;
    [SerializeField] private float screenShakeDuration = 0.2f;
    
    [Header("Ammo")]
    [SerializeField] private int maxAmmo = 2;
    private int currentAmmo;
    
    [Header("Controls")]
    [SerializeField] private bool canShoot = true;
    
    private bool isReloading = false;
    private Vector2 initialGunPosition; // Changed to Vector2 for UI
    private Sequence swaySequence;
    private Camera mainCamera;
    private UIManager uiManager;
    private GameManager gameManager;
    private FirstPersonController playerController; // Reference to player controller for movement
    
    void Start()
    {
        // Get references
        mainCamera = Camera.main;
        uiManager = FindObjectOfType<UIManager>();
        gameManager = FindObjectOfType<GameManager>();
        playerController = FindObjectOfType<FirstPersonController>();
        
        // If gunImage is not set directly, try to get it from UIManager
        if (gunImage == null && uiManager != null)
        {
            gunImage = uiManager.GetGunContainer();
        }
        
        // Initialize
        currentAmmo = maxAmmo;
        
        if (gunImage)
        {
            initialGunPosition = gunImage.anchoredPosition; // Using anchoredPosition for UI elements
            
            // Start gun sway
            StartGunSway();
        }
        else
        {
            Debug.LogError("Gun Image not found or assigned! Gun animations will not work.");
        }
        
        // Update UI
        UpdateAmmoUI();
    }
    
    void Update()
    {
        // Handle shooting
        if (Input.GetMouseButtonDown(0) && !isReloading && canShoot)
        {
            Shoot();
        }
        
        // Update sway intensity based on player movement
        if (playerController != null && !isReloading && swaySequence != null)
        {
            // Calculate player movement velocity for intensifying sway
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            bool isMoving = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
            
            if (isMoving)
            {
                // Intensify sway when moving
                float intensity = Vector2.SqrMagnitude(new Vector2(horizontalInput, verticalInput));
                swaySequence.timeScale = 1f + (intensity * movementSwayMultiplier);
            }
            else
            {
                // Normal sway when standing still
                swaySequence.timeScale = 1f;
            }
        }
    }
    
    // Public method to enable/disable shooting
    public void SetCanShoot(bool enabled)
    {
        canShoot = enabled;
    }
    
    private void Shoot()
    {
        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }
        
        // Reduce ammo
        currentAmmo--;
        
        // Update UI
        UpdateAmmoUI();
        
        // Screen shake for recoil
        ScreenShake();
        
        // Trigger gun recoil animation through UI Manager
        if (uiManager != null)
        {
            uiManager.TriggerGunRecoil();
        }
        
        // Play gunshot sound
        if (gameManager != null)
        {
            gameManager.PlaySound("gunshot");
        }
        
        // Perform raycast to check hit
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit))
        {
            // Check if we hit a rabbit
            RabbitController rabbit = hit.collider.GetComponent<RabbitController>();
            if (rabbit != null)
            {
                rabbit.Die();
            }
        }
        
        // Auto reload when empty
        if (currentAmmo <= 0)
        {
            Reload();
        }
    }
    
    private void UpdateAmmoUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateAmmoUI(currentAmmo, maxAmmo);
        }
    }
    
    private void Reload()
    {
        if (isReloading) return;
        
        isReloading = true;
        
        // Stop sway during reload
        if (swaySequence != null)
        {
            swaySequence.Kill();
        }
        
        // Original position before reload animation
        Vector2 startPos = gunImage.anchoredPosition;
        
        // Play reload animation - modified for UI element
        gunImage.DOAnchorPosY(startPos.y - reloadDownAmount, reloadDuration / 2)
            .OnComplete(() => {
                // Play reload sound
                if (gameManager != null)
                {
                    gameManager.PlaySound("reload");
                }
                
                // Restore ammo
                currentAmmo = maxAmmo;
                
                // Update UI
                UpdateAmmoUI();
                
                // Return gun to original position
                gunImage.DOAnchorPosY(initialGunPosition.y, reloadDuration / 2)
                    .OnComplete(() => {
                        isReloading = false;
                        StartGunSway();
                    });
            });
    }
    
    private void StartGunSway()
    {
        if (gunImage == null) return;
        
        // Kill any existing sequence
        if (swaySequence != null)
        {
            swaySequence.Kill();
        }
        
        // Create new sway sequence
        swaySequence = DOTween.Sequence();
        
        // For UI, we'll use rotation on the Z axis for sway
        swaySequence.Append(gunImage.DORotate(new Vector3(0, 0, swayAmount), swayCycleDuration / 2));
        swaySequence.Append(gunImage.DORotate(new Vector3(0, 0, -swayAmount), swayCycleDuration));
        swaySequence.Append(gunImage.DORotate(new Vector3(0, 0, swayAmount), swayCycleDuration / 2));
        
        // Add subtle position sway as well
        Sequence positionSway = DOTween.Sequence();
        positionSway.Append(gunImage.DOAnchorPos(
            new Vector2(initialGunPosition.x + 5, initialGunPosition.y - 2), 
            swayCycleDuration * 0.7f
        ));
        positionSway.Append(gunImage.DOAnchorPos(
            new Vector2(initialGunPosition.x - 5, initialGunPosition.y + 2), 
            swayCycleDuration * 1.4f
        ));
        positionSway.Append(gunImage.DOAnchorPos(
            initialGunPosition, 
            swayCycleDuration * 0.7f
        ));
        
        // Make the position sway happen in parallel
        swaySequence.Join(positionSway);
        
        // Set as looping
        swaySequence.SetLoops(-1, LoopType.Restart);
    }
    
    private void ScreenShake()
    {
        if (mainCamera == null) return;
        
        // Create screen shake effect using DOTween
        mainCamera.transform.DOShakePosition(screenShakeDuration, screenShakeAmount);
    }
    
    // Public method to stop all animations (for use during scene transitions)
    public void StopAllAnimations()
    {
        if (swaySequence != null)
        {
            swaySequence.Kill();
        }
        
        // Kill any active DOTween animations on the gun
        DOTween.Kill(gunImage);
    }
}
