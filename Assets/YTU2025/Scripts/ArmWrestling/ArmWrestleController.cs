using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class ArmWrestleController : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private ArmWrestleOpponentConfig[] opponents;
    [SerializeField] private int currentOpponentIndex = 0;
    
    [Header("UI References")]
    [SerializeField] private RectTransform sweetSpotBar;
    [SerializeField] private RectTransform playerBar;
    [SerializeField] private RectTransform staminaMeter;
    [SerializeField] private Image staminaFillImage;  // Changed to Image for fillAmount
    [SerializeField] private Image vignetteEffect;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI roundNumberText;
    [SerializeField] private GameObject roundCompletePanel;
    [SerializeField] private RectTransform roundCompleteImage;
    [SerializeField] private Image opponentImage;  // Added for opponent state changes
    
    [Header("Round Transition")]
    [SerializeField] private float roundCompleteShowDuration = 1.5f;
    [SerializeField] private float roundCompleteTweenDuration = 0.8f;
    [SerializeField] private Vector2 roundCompleteEntryPosition = new Vector2(-1000, 0);
    [SerializeField] private Vector2 roundCompleteHoldPosition = new Vector2(0, 0);
    [SerializeField] private Vector2 roundCompleteExitPosition = new Vector2(1000, 0);
    [SerializeField] private Ease roundCompleteEntryEase = Ease.OutBack;
    [SerializeField] private Ease roundCompleteExitEase = Ease.InBack;
    
    [Header("Countdown")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float countdownDuration = 0.7f;
    [SerializeField] private float countdownScaleStart = 2.0f;
    [SerializeField] private float countdownScaleEnd = 0.5f;
    [SerializeField] private string countdownGoText = "GO!";
    
    [Header("Screen Effects")]
    [SerializeField] private float maxVignetteAlpha = 0.8f;
    [SerializeField] private float vignetteThreshold = 0.3f;
    [SerializeField] private float screenShakeAmount = 0.5f;
    [SerializeField] private float screenShakeThreshold = 0.2f;
    
    [Header("Sound Effects")]
    [SerializeField] private string struggleSound = "ArmWrestleStruggle";
    [SerializeField] private string winSound = "ArmWrestleWin";
    [SerializeField] private string loseSound = "ArmWrestleLose";
    [SerializeField] private string twitchSound = "ArmWrestleTwitch";
    
    [Header("Visual Novel Transition")]
    [SerializeField] private float delayBeforeVN = 2.0f;
    
    [Header("Game Over")]
    [SerializeField] private float gameOverDelay = 2.0f; // Delay before reloading scene on loss
    
    [Header("Randomness Settings")]
    [SerializeField] private bool enableRandomness = true;
    [SerializeField] [Range(0f, 1f)] private float randomDriftChance = 0.05f; // Chance of random direction change
    [SerializeField] [Range(0f, 0.5f)] private float randomSpeedVariation = 0.2f; // Speed variation amount
    [SerializeField] [Range(0f, 0.2f)] private float randomMovementAmplitude = 0.05f; // Random movement amplitude
    
    // UI bounds parameters
    [Header("UI Bounds Settings")]
    [SerializeField] private bool useSharedBoundaries = false;
    [SerializeField] private bool visualizeBoundaries = false;
    [SerializeField] private Color boundaryVisualizationColor = new Color(1f, 0f, 0f, 0.3f);
    
    [Header("Sweet Spot Boundaries")]
    [SerializeField] private float sweetSpotMinY = 0.05f; // Minimum position for sweet spot
    [SerializeField] private float sweetSpotMaxY = 0.95f; // Maximum position for sweet spot
    [SerializeField] private float sweetSpotPadding = 0.02f; // Padding at sweet spot boundaries
    
    [Header("Player Bar Boundaries")]
    [SerializeField] private float playerMinY = 0.05f; // Minimum position for player bar
    [SerializeField] private float playerMaxY = 0.95f; // Maximum position for player bar
    [SerializeField] private float playerPadding = 0.02f; // Padding at player boundaries
    
    private float meterHeight;
    
    // Game state
    private bool isGameActive = false;
    private float sweetCenterY;
    private float playerY;
    private float opponentStamina = 0.5f;  // Start at 50% instead of full
    private float remainingTime;
    private float driftDir = 1f;
    private bool transitioningToVN = false;
    private bool isCountingDown = false;
    private int currentSpriteState = 0; // 0 = normal, 1 = near win, 2 = near loss
    
    // Twitching parameters
    private float nextTwitchTime;
    
    // Visual novel controller reference
    private VisualNovelController vnController;
    
    // References
    private Camera mainCamera;
    private UIManager uiManager;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        uiManager = FindObjectOfType<UIManager>();
        vnController = FindObjectOfType<VisualNovelController>();
        
        // Hide the round complete panel initially
        if (roundCompletePanel != null)
        {
            roundCompletePanel.SetActive(false);
        }
        
        // Initialize vignette effect
        if (vignetteEffect != null)
        {
            Color c = vignetteEffect.color;
            c.a = 0;
            vignetteEffect.color = c;
        }
    }
    
    private void Start()
    {
        // Initialize UI measurements
        if (staminaMeter != null)
        {
            meterHeight = staminaMeter.rect.height;
        }
        
        // Initialize the stamina fill image if using vertical fill
        if (staminaFillImage != null)
        {
            // Make sure the fill image is set to vertical fill
            staminaFillImage.type = Image.Type.Filled;
            staminaFillImage.fillMethod = Image.FillMethod.Vertical;
            staminaFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        }
        
        // If using shared boundaries, copy the sweet spot values to player values
        if (useSharedBoundaries)
        {
            playerMinY = sweetSpotMinY;
            playerMaxY = sweetSpotMaxY;
            playerPadding = sweetSpotPadding;
        }
        
        // Start the first round
        StartRound();
    }
    
    private void StartRound()
    {
        if (currentOpponentIndex >= opponents.Length)
        {
            // All opponents defeated, transition to VN
            TransitionToVisualNovel();
            return;
        }
        
        // Get current opponent config
        ArmWrestleOpponentConfig opponent = opponents[currentOpponentIndex];
        
        // Update round number display
        if (roundNumberText != null)
        {
            roundNumberText.text = $"Round {currentOpponentIndex + 1}";
        }
        
        // Reset game state
        isGameActive = false;
        isCountingDown = true;
        opponentStamina = 0.5f;  // Start at 50% instead of full
        sweetCenterY = 0.5f; // Start in the middle
        playerY = 0.5f;      // Start in the middle
        driftDir = 1f;
        
        // Start countdown animation
        StartCoroutine(StartCountdown());
        
        // Set time limit if applicable
        remainingTime = opponent.timeLimit;
        if (timerText != null)
        {
            timerText.gameObject.SetActive(opponent.timeLimit > 0);
            UpdateTimerDisplay();
        }
        
        // Reset visual elements
        UpdateSweetSpotUI();
        UpdatePlayerBarUI();
        UpdateStaminaUI();
        
        // Initialize twitching
        if (opponent.enableTwitching)
        {
            nextTwitchTime = Time.time + Random.Range(opponent.twitchInterval * 0.5f, opponent.twitchInterval * 1.5f);
        }
    }
    
    private void Update()
    {
        if ((!isGameActive && !isCountingDown) || transitioningToVN) return;
        
        // During countdown, only update the countdown animation, not game logic
        if (isCountingDown) return;
        
        ArmWrestleOpponentConfig opponent = opponents[currentOpponentIndex];
        
        // 1) Update sweet spot position (drift)
        float currentDriftSpeed = opponent.driftSpeed;
        
        // Apply random speed variation if enabled
        if (enableRandomness)
        {
            // Random speed variation
            currentDriftSpeed *= (1f + Random.Range(-randomSpeedVariation, randomSpeedVariation));
            
            // Random direction change
            if (Random.value < randomDriftChance * Time.deltaTime)
            {
                driftDir *= -1;
                Debug.Log("Random direction change!");
            }
            
            // Random movement jitter
            float randomJitter = Random.Range(-randomMovementAmplitude, randomMovementAmplitude) * Time.deltaTime;
            sweetCenterY += randomJitter;
        }
        
        // Apply the drift movement
        sweetCenterY += driftDir * currentDriftSpeed * Time.deltaTime;
        
        // Bounce at edges with padding for sweet spot
        float topBoundary = sweetSpotMaxY - (opponent.sweetSpotSize / 2) - sweetSpotPadding;
        float bottomBoundary = sweetSpotMinY + (opponent.sweetSpotSize / 2) + sweetSpotPadding;
        
        if (sweetCenterY > topBoundary || sweetCenterY < bottomBoundary)
        {
            driftDir *= -1;
            // Clamp to boundaries to prevent getting stuck
            sweetCenterY = Mathf.Clamp(sweetCenterY, bottomBoundary, topBoundary);
        }
        
        // 2) Handle twitching if enabled
        if (opponent.enableTwitching && Time.time >= nextTwitchTime)
        {
            // Perform a twitch
            float twitchDir = Random.value > 0.5f ? 1 : -1;
            sweetCenterY += twitchDir * opponent.twitchAmount;
            
            // Make sure we don't twitch out of bounds (using sweet spot boundaries)
            float topTwitchBoundary = sweetSpotMaxY - (opponent.sweetSpotSize / 2) - sweetSpotPadding;
            float bottomTwitchBoundary = sweetSpotMinY + (opponent.sweetSpotSize / 2) + sweetSpotPadding;
            sweetCenterY = Mathf.Clamp(sweetCenterY, bottomTwitchBoundary, topTwitchBoundary);
            
            // Play twitch sound
            GameManager.Instance?.PlaySound(twitchSound);
            
            // Schedule next twitch
            nextTwitchTime = Time.time + Random.Range(opponent.twitchInterval * 0.5f, opponent.twitchInterval * 1.5f);
        }
        
        // 3) Handle player input
        bool isInputPressed = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        
        if (isInputPressed)
        {
            playerY += opponent.riseSpeed * Time.deltaTime;
        }
        else
        {
            playerY -= opponent.fallSpeed * Time.deltaTime;
        }
        
        // Clamp player position to player boundaries
        playerY = Mathf.Clamp(playerY, playerMinY + playerPadding, playerMaxY - playerPadding);
        
        // 4) Calculate overlap and adjust stamina
        float playerSize = playerBar.rect.height / meterHeight;
        float overlap = CalculateVerticalOverlap(playerY, playerSize, sweetCenterY, opponent.sweetSpotSize);
        
        if (overlap > 0)
        {
            // Player is in sweet spot - drain opponent stamina
            opponentStamina -= opponent.staminaDrainInside * Time.deltaTime;
            
            // Play struggle sound if not already playing
            if (!SoundManager.instance.IsPlaying(struggleSound))
            {
                GameManager.Instance?.PlaySound(struggleSound);
            }
        }
        else
        {
            // Player is outside sweet spot - opponent recovers
            opponentStamina += opponent.staminaRecoverOutside * Time.deltaTime;
            
            // Stop struggle sound
            if (SoundManager.instance.IsPlaying(struggleSound))
            {
                SoundManager.instance.Stop(struggleSound);
            }
        }
        
        // Clamp stamina
        opponentStamina = Mathf.Clamp01(opponentStamina);
        
        // 5) Update timer if needed
        if (opponent.timeLimit > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerDisplay();
            
            if (remainingTime <= 0)
            {
                // Time's up - player loses
                LoseRound();
                return;
            }
        }
        
        // 6) Check win/loss condition
        if (opponentStamina <= 0)
        {
            WinRound();
            return;
        }
        else if (opponentStamina >= 1.0f)
        {
            LoseRound();
            return;
        }
        
        // 7) Visual effects based on stamina
        // Vignette effect when close to losing
        if (vignetteEffect != null)
        {
            float vignetteIntensity = 0;
            
            if (opponentStamina > 1.0f - vignetteThreshold)
            {
                // Map from (1-threshold -> 1) to (0 -> 1)
                vignetteIntensity = Mathf.InverseLerp(1.0f - vignetteThreshold, 1.0f, opponentStamina);
            }
            
            Color c = vignetteEffect.color;
            c.a = vignetteIntensity * maxVignetteAlpha;
            vignetteEffect.color = c;
            
            // Screen shake when close to losing
            if (opponentStamina > 1.0f - screenShakeThreshold && mainCamera != null)
            {
                float shakeIntensity = Mathf.InverseLerp(1.0f - screenShakeThreshold, 1.0f, opponentStamina);
                Vector3 originalPos = mainCamera.transform.position;
                
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * shakeIntensity * screenShakeAmount * 0.01f, 
                    Random.Range(-1f, 1f) * shakeIntensity * screenShakeAmount * 0.01f, 
                    0
                );
                
                mainCamera.transform.position = originalPos + shakeOffset;
            }
        }
        
        // 8) Update UI elements
        UpdateSweetSpotUI();
        UpdatePlayerBarUI();
        UpdateStaminaUI();
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText != null && remainingTime > 0)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
        }
    }
    
    private float CalculateVerticalOverlap(float playerY, float playerHeight, float sweetY, float sweetHeight)
    {
        // Calculate the bounds of each bar
        float playerTop = playerY + playerHeight / 2;
        float playerBottom = playerY - playerHeight / 2;
        float sweetTop = sweetY + sweetHeight / 2;
        float sweetBottom = sweetY - sweetHeight / 2;
        
        // Check for overlap
        if (playerBottom > sweetTop || playerTop < sweetBottom)
        {
            return 0; // No overlap
        }
        
        // Calculate overlap amount
        float overlap = Mathf.Min(playerTop, sweetTop) - Mathf.Max(playerBottom, sweetBottom);
        return overlap;
    }
    
    private void UpdateSweetSpotUI()
    {
        if (sweetSpotBar == null || staminaMeter == null) return;
        
        // Calculate the size and position of the sweet spot
        float sweetSpotHeight = opponents[currentOpponentIndex].sweetSpotSize * meterHeight;
        sweetSpotBar.sizeDelta = new Vector2(sweetSpotBar.sizeDelta.x, sweetSpotHeight);
        
        // Set position (from bottom of the meter)
        float posY = sweetCenterY * meterHeight;
        sweetSpotBar.anchoredPosition = new Vector2(sweetSpotBar.anchoredPosition.x, posY);
    }
    
    private void UpdatePlayerBarUI()
    {
        if (playerBar == null || staminaMeter == null) return;
        
        // Set position (from bottom of the meter)
        float posY = playerY * meterHeight;
        playerBar.anchoredPosition = new Vector2(playerBar.anchoredPosition.x, posY);
    }
    
    private void UpdateStaminaUI()
    {
        if (staminaFillImage == null) return;
        
        // Update the fill amount based on opponent stamina
        staminaFillImage.fillAmount = opponentStamina;
        
        // Update opponent sprite based on stamina
        UpdateOpponentSprite();
    }
    
    private void UpdateOpponentSprite()
    {
        if (opponentImage == null) return;
        
        ArmWrestleOpponentConfig opponent = opponents[currentOpponentIndex];
        int newSpriteState = 0; // Default to normal state
        
        // Determine which sprite to show based on stamina
        if (opponentStamina > 1.0f - opponent.nearLossThreshold)
        {   
            // Near loss state
            newSpriteState = 2;
            if (opponent.nearLossSprite != null)
                opponentImage.sprite = opponent.nearLossSprite;
        }
        else if (opponentStamina < opponent.nearWinThreshold)
        {
            // Near win state
            newSpriteState = 1;
            if (opponent.nearWinSprite != null)
                opponentImage.sprite = opponent.nearWinSprite;
        }
        else
        {
            // Normal state
            newSpriteState = 0;
            if (opponent.normalSprite != null)
                opponentImage.sprite = opponent.normalSprite;
        }
        
        // If state changed, play the appropriate sound
        if (newSpriteState != currentSpriteState)
        {
            // Play sound on state change
            if (newSpriteState == 1 && !string.IsNullOrEmpty(opponent.nearWinSound))
            {
                GameManager.Instance?.PlaySound(opponent.nearWinSound);
            }
            else if (newSpriteState == 2 && !string.IsNullOrEmpty(opponent.nearLossSound))
            {
                GameManager.Instance?.PlaySound(opponent.nearLossSound);
            }
            
            // Update current state
            currentSpriteState = newSpriteState;
        }
    }
    
    private IEnumerator StartCountdown()
    {
        if (countdownPanel == null || countdownText == null)
        {
            // No countdown UI, just start the game
            isGameActive = true;
            isCountingDown = false;
            yield break;
        }
        
        // Ensure the player and sweet spot bars aren't visible during countdown
        if (playerBar != null) playerBar.gameObject.SetActive(false);
        if (sweetSpotBar != null) sweetSpotBar.gameObject.SetActive(false);
        
        // Show countdown panel
        countdownPanel.SetActive(true);
        
        // 3...
        countdownText.text = "3";
        countdownText.transform.localScale = Vector3.one * countdownScaleStart;
        countdownText.transform.DOScale(countdownScaleEnd, countdownDuration).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(countdownDuration);
        
        // 2...
        countdownText.text = "2";
        countdownText.transform.localScale = Vector3.one * countdownScaleStart;
        countdownText.transform.DOScale(countdownScaleEnd, countdownDuration).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(countdownDuration);
        
        // 1...
        countdownText.text = "1";
        countdownText.transform.localScale = Vector3.one * countdownScaleStart;
        countdownText.transform.DOScale(countdownScaleEnd, countdownDuration).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(countdownDuration);
        
        // GO!
        countdownText.text = countdownGoText;
        countdownText.transform.localScale = Vector3.one * countdownScaleStart;
        countdownText.transform.DOScale(countdownScaleEnd, countdownDuration).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(countdownDuration);
        
        // Hide countdown panel
        countdownPanel.SetActive(false);
        
        // Show the bars after countdown
        if (playerBar != null) playerBar.gameObject.SetActive(true);
        if (sweetSpotBar != null) sweetSpotBar.gameObject.SetActive(true);
        
        // Start the game
        isGameActive = true;
        isCountingDown = false;
    }
    
    private void WinRound()
    {
        isGameActive = false;
        
        // Stop any ongoing sounds
        if (SoundManager.instance != null && SoundManager.instance.IsPlaying(struggleSound))
        {
            SoundManager.instance.Stop(struggleSound);
        }
        
        // Change to opponent's loss sprite
        if (opponentImage != null)
        {
            ArmWrestleOpponentConfig opponent = opponents[currentOpponentIndex];
            if (opponent.lossSprite != null)
            {
                opponentImage.sprite = opponent.lossSprite;
            }
            
            // Play opponent's loss sound if specified, otherwise use default win sound
            if (!string.IsNullOrEmpty(opponent.lossSound))
            {
                GameManager.Instance?.PlaySound(opponent.lossSound);
            }
            else
            {
                GameManager.Instance?.PlaySound(winSound);
            }
        }
        else
        {
            // Fallback to default win sound
            GameManager.Instance?.PlaySound(winSound);
        }
        
        // Check if this is the last opponent
        if (currentOpponentIndex >= opponents.Length - 1)
        {
            // Skip round complete animation for the last opponent
            AdvanceToNextRound();
        }
        else
        {
            // Show round complete transition for non-final opponents
            StartCoroutine(ShowRoundCompleteAnimation(true));
        }
    }
    
    private void LoseRound()
    {
        isGameActive = false;
        
        // Stop any ongoing sounds
        if (SoundManager.instance != null && SoundManager.instance.IsPlaying(struggleSound))
        {
            SoundManager.instance.Stop(struggleSound);
        }
        
        // Change to opponent's win sprite
        if (opponentImage != null)
        {
            ArmWrestleOpponentConfig opponent = opponents[currentOpponentIndex];
            if (opponent.winSprite != null)
            {
                opponentImage.sprite = opponent.winSprite;
            }
            
            // Play opponent's win sound if specified, otherwise use default lose sound
            if (!string.IsNullOrEmpty(opponent.winSound))
            {
                GameManager.Instance?.PlaySound(opponent.winSound);
            }
            else
            {
                GameManager.Instance?.PlaySound(loseSound);
            }
        }
        else
        {
            // Fallback to default lose sound
            GameManager.Instance?.PlaySound(loseSound);
        }
        
        // Restart the scene after delay
        StartCoroutine(GameOverDelayedRestart());
    }
    
    private IEnumerator GameOverDelayedRestart()
    {
        // Wait for the delay first
        yield return new WaitForSeconds(gameOverDelay);
        
        // Then fade out and restart
        if (uiManager != null)
        {
            uiManager.FadeToBlack(1.0f, () => {
                // Make sure we're actually reloading the scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            });
        }
        else
        {
            // Direct scene reload as fallback
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    
    private IEnumerator ShowRoundCompleteAnimation(bool isWin)
    {
        if (roundCompletePanel == null || roundCompleteImage == null) 
        {
            // No panel, just continue
            AdvanceToNextRound();
            yield break;
        }
        
        // Show the panel
        roundCompletePanel.SetActive(true);
        
        // Animate the image sliding in from entry position
        roundCompleteImage.anchoredPosition = roundCompleteEntryPosition;
        roundCompleteImage.DOAnchorPos(roundCompleteHoldPosition, roundCompleteTweenDuration).SetEase(roundCompleteEntryEase);
        
        // Wait for display duration
        yield return new WaitForSeconds(roundCompleteShowDuration);
        
        // Animate sliding out to exit position
        roundCompleteImage.DOAnchorPos(roundCompleteExitPosition, roundCompleteTweenDuration).SetEase(roundCompleteExitEase).OnComplete(() => {
            roundCompletePanel.SetActive(false);
            AdvanceToNextRound();
        });
    }
    
    private void AdvanceToNextRound()
    {
        // Move to the next opponent
        currentOpponentIndex++;
        
        // Start the next round or transition to VN if all opponents defeated
        StartRound();
    }
    
    private void TransitionToVisualNovel()
    {
        if (transitioningToVN) return;
        transitioningToVN = true;
        
        // Add delay before starting visual novel
        StartCoroutine(DelayedVisualNovelStart());
    }
    
    private IEnumerator DelayedVisualNovelStart()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delayBeforeVN);
        
        if (vnController != null)
        {
            // Get the last opponent for VN dialog
            if (opponents.Length > 0)
            {
                ArmWrestleOpponentConfig lastOpponent = opponents[opponents.Length - 1];
                vnController.StartVisualNovel(lastOpponent);
            }
            else
            {
                Debug.LogError("No opponents configured!");
            }
        }
        else
        {
            Debug.LogError("Visual Novel Controller not found!");
        }
    }
    
    // For debugging/testing - Add buttons to the inspector that call these methods
    public void DEBUG_WinRound()
    {
        opponentStamina = 0;
    }
    
    public void DEBUG_LoseRound()
    {
        opponentStamina = 1;
    }
}
