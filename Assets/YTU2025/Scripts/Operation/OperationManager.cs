using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using System;

public class OperationManager : MonoBehaviour
{
    [Header("Operation Configuration")]
    [SerializeField] private OperationConfig config;
    
    [Header("Scene References")]
    [SerializeField] private Transform socketContainer;    // Parent object for sockets
    [SerializeField] private Transform wireContainer;      // Parent object for wire ends
    [SerializeField] private OperationTimer operationTimer; // Reference to the timer
    [SerializeField] private Image fadeImage;              // For scene transitions
    [SerializeField] private CanvasGroup canvasGroup;      // For fading out the entire UI
    
    [Header("Prefabs")]
    [SerializeField] private GameObject socketPrefab;      // Prefab for sockets
    [SerializeField] private GameObject wirePrefab;        // Prefab for wire ends
    
    [Header("Audio")]
    [SerializeField] private string scalpelHitSFX = "scalpel_hit";
    [SerializeField] private string correctConnectionSFX = "connection_correct";
    [SerializeField] private string incorrectConnectionSFX = "connection_incorrect";
    [SerializeField] private string heartbeatStabilizeSFX = "heartbeat_stabilize";
    [SerializeField] private string flatlineSFX = "flatline";
    
    [Header("Scene Transitions")]
    [SerializeField] private string successSceneName = "SuccessScene";  // Scene to load on success
    [SerializeField] private string failureSceneName = "FailureScene";  // Scene to load on failure
    [SerializeField] private float fadeOutDuration = 1f;  // Duration of the fade out transition
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;        // Enable debug logs
    [SerializeField] private float snapRadiusMultiplier = 3f; // Increase snap radius for testing
    
    // Events
    public event Action OnOperationSuccess;
    public event Action OnOperationFailure;
    
    // Private variables
    private IWireController[] wires;
    private SocketController[] sockets;
    private bool[] isConnected;
    private bool operationActive = false;
    private bool operationComplete = false;
    private GameManager gameManager;
    private UIManager uiManager;
    private Canvas canvas;
    private Camera canvasCamera;
    
    // Properties
    public OperationConfig Config => config;
    
    private void Awake()
    {
        // Find managers
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        
        // Get canvas camera
        canvasCamera = canvas != null ? canvas.worldCamera : null;
        
        // Initialize arrays based on config
        int connectionCount = config.leadColors.Length;
        wires = new IWireController[connectionCount];
        sockets = new SocketController[connectionCount];
        isConnected = new bool[connectionCount];
    }
    
    private void Start()
    {
        if (debugMode)
        {
            Debug.Log($"OperationManager initialized with {config.leadColors.Length} connections");
            Debug.Log($"Snap radius: {config.snapRadius * snapRadiusMultiplier}");
            Debug.Log($"Canvas render mode: {canvas.renderMode}");
        }
        
        // Initialize the scene with a black screen, then fade in
        if (uiManager != null)
        {
            uiManager.FadeFromBlack(0.5f, () => {
                // Play the scalpel hit sound effect
                PlaySound(scalpelHitSFX);
                
                // Start the operation after a short delay
                StartCoroutine(StartOperationAfterDelay(1f));
            });
        }
        else
        {
            // No UI manager, just start directly
            StartCoroutine(StartOperationAfterDelay(1f));
        }
    }
    
    private IEnumerator StartOperationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Start the timer
        if (operationTimer != null)
        {
            operationTimer.OnTimerComplete += HandleOperationFailure;
            operationTimer.StartTimer();
        }
        
        operationActive = true;
        
        if (debugMode)
        {
            Debug.Log("Operation active, timer started");
        }
    }
    
    #region Socket and Wire Registration
    
    public void RegisterSocket(SocketController socket, int index)
    {
        if (index >= 0 && index < sockets.Length)
        {
            sockets[index] = socket;
            if (debugMode)
            {
                Debug.Log($"Registered socket index {index} with color {ColorToHex(socket.GetColor())}");
            }
        }
        else
        {
            Debug.LogError($"Invalid socket index: {index}. Max index should be {sockets.Length - 1}");
        }
    }
    
    public void RegisterWire(IWireController wire, int index)
    {
        if (index >= 0 && index < wires.Length)
        {
            wires[index] = wire;
            if (debugMode)
            {
                Debug.Log($"Registered wire index {index}");
            }
        }
        else
        {
            Debug.LogError($"Invalid wire index: {index}. Max index should be {wires.Length - 1}");
        }
    }
    
    #endregion
    
    #region Interaction Handling
    
    public SocketController FindClosestSocket(Vector3 pointerWorldPos, int wireIndex)
    {
        // Don't check if operation is not active
        if (!operationActive || operationComplete)
        {
            if (debugMode) Debug.Log("Operation not active or complete, not finding socket");
            return null;
        }
        
        if (debugMode)
        {
            Debug.Log($"Finding closest socket to position {pointerWorldPos} for wire index {wireIndex}");
        }
        
        float closestDistance = float.MaxValue;
        SocketController closestSocket = null;
        float snapRadius = config.snapRadius * snapRadiusMultiplier;
        
        foreach (SocketController socket in sockets)
        {
            if (socket == null) continue;
            
            // Skip sockets that are already connected
            if (socket.IsConnected())
            {
                if (debugMode) Debug.Log($"Socket {socket.GetSocketIndex()} is already connected, skipping");
                continue;
            }
            
            Vector3 socketPosition = socket.GetPosition();
            float distance = Vector3.Distance(pointerWorldPos, socketPosition);
            
            if (debugMode)
            {
                Debug.Log($"Socket {socket.GetSocketIndex()} is at position {socketPosition}, distance: {distance}, snap radius: {snapRadius}");
            }
            
            // Check if within snap radius and closer than previous best match
            if (distance < snapRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestSocket = socket;
                
                if (debugMode)
                {
                    Debug.Log($"Socket {socket.GetSocketIndex()} is now the closest at distance {distance}");
                }
            }
        }
        
        if (closestSocket != null && debugMode)
        {
            Debug.Log($"Found closest socket: index {closestSocket.GetSocketIndex()}, color {ColorToHex(closestSocket.GetColor())}");
        }
        else if (debugMode)
        {
            Debug.Log("No socket found within snap radius");
        }
        
        return closestSocket;
    }
    
    public void OnWireDragStart(IWireController wire)
    {
        // Add any logic for when a wire starts being dragged
    }
    
    public void OnWireDragEnd(IWireController wire)
    {
        // Add any logic for when a wire stops being dragged
    }
    
    public void OnWireConnected(int wireIndex)
    {
        if (debugMode)
        {
            Debug.Log($"Wire {wireIndex} connected");
        }
        
        // Mark this connection as complete
        isConnected[wireIndex] = true;
        
        // Mark the socket as connected too
        if (sockets[wireIndex] != null)
        {
            sockets[wireIndex].SetConnected(true);
            
            if (debugMode)
            {
                Debug.Log($"Socket {wireIndex} marked as connected");
            }
        }
        else
        {
            Debug.LogError($"Socket at index {wireIndex} is null");
        }
        
        // Check if all connections are complete
        CheckOperationCompletion();
    }
    
    public void OnCorrectConnection()
    {
        // Play success sound
        PlaySound(correctConnectionSFX);
    }
    
    public void OnIncorrectConnection()
    {
        if (debugMode)
        {
            Debug.Log("Incorrect connection detected");
        }
        
        // Play error sound
        PlaySound(incorrectConnectionSFX);
        
        // Apply time penalty
        if (operationTimer != null)
        {
            operationTimer.DeductTime(config.incorrectTimePenalty);
        }
    }
    
    private void CheckOperationCompletion()
    {
        // Check if all wires are connected
        bool allConnected = true;
        
        for (int i = 0; i < isConnected.Length; i++)
        {
            if (!isConnected[i])
            {
                allConnected = false;
                break;
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Operation completion check: all connected = {allConnected}");
        }
        
        // If all are connected, the operation is successful
        if (allConnected)
        {
            HandleOperationSuccess();
        }
    }
    
    #endregion
    
    #region Operation Outcomes
    
    private void HandleOperationSuccess()
    {
        if (operationComplete) return;
        
        operationComplete = true;
        operationActive = false;
        
        if (debugMode)
        {
            Debug.Log("Operation completed successfully!");
        }
        
        // Stop the timer
        if (operationTimer != null)
        {
            operationTimer.PauseTimer();
        }
        
        // Play success sound
        PlaySound(heartbeatStabilizeSFX);
        
        // Visual feedback
        ShowSuccessFeedback();
        
        // Invoke the success event
        OnOperationSuccess?.Invoke();
        
        // Transition to next scene after a delay
        StartCoroutine(TransitionAfterDelay(true, 2f));
    }
    
    private void HandleOperationFailure()
    {
        if (operationComplete) return;
        
        operationComplete = true;
        operationActive = false;
        
        if (debugMode)
        {
            Debug.Log("Operation failed - time ran out!");
        }
        
        // Play failure sound
        PlaySound(flatlineSFX);
        
        // Visual feedback
        ShowFailureFeedback();
        
        // Invoke the failure event
        OnOperationFailure?.Invoke();
        
        // Transition to next scene after a delay
        StartCoroutine(TransitionAfterDelay(false, 2f));
    }
    
    private void ShowSuccessFeedback()
    {
        // Could add visual effects or animations here for success
        // For example, a green overlay fade, particle effects, etc.
    }
    
    private void ShowFailureFeedback()
    {
        // Could add visual effects or animations here for failure
        // For example, a red overlay fade, screen shake, etc.
    }
    
    private IEnumerator TransitionAfterDelay(bool success, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Determine which scene to load
        string nextScene = success ? successSceneName : failureSceneName;
        
        if (debugMode)
        {
            Debug.Log($"Transitioning to {nextScene} scene after delay");
        }
        
        // Fade out using canvas group
        if (canvasGroup != null)
        {
            if (debugMode) Debug.Log("Starting canvas group fade out");
            
            // Disable interaction during fade
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            // Fade out the canvas group
            float elapsedTime = 0;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeOutDuration);
                yield return null;
            }
            
            // Ensure alpha is fully at 0
            canvasGroup.alpha = 1f;
            
            if (debugMode) Debug.Log($"Fade out complete, loading scene: {nextScene}");
        }
        else if (debugMode)
        {
            Debug.LogWarning("No canvas group assigned for fade out effect");
        }
        
        // Load the next scene directly using SceneManager
        SceneManager.LoadScene(nextScene);
    }
    
    #endregion
    
    #region Utility Methods
    
    private void PlaySound(string soundName)
    {
        if (gameManager != null)
        {
            gameManager.PlaySound(soundName);
        }
    }
    
    // Helper method to convert color to hex for debugging
    private string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }
    
    // Can be called from inspector buttons for testing
    public void ResetOperation()
    {
        if (debugMode)
        {
            Debug.Log("Resetting operation");
        }
        
        // Reset all connections
        for (int i = 0; i < isConnected.Length; i++)
        {
            isConnected[i] = false;
            
            if (wires[i] != null)
            {
                wires[i].Reset();
            }
            
            if (sockets[i] != null)
            {
                sockets[i].Reset();
            }
        }
        
        // Reset the timer
        if (operationTimer != null)
        {
            operationTimer.ResetTimer();
        }
        
        operationComplete = false;
        operationActive = true;
        
        // Start the timer again
        if (operationTimer != null)
        {
            operationTimer.StartTimer();
        }
    }
    
    #endregion
}
