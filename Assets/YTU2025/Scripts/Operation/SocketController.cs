using UnityEngine;
using UnityEngine.UI;

public class SocketController : MonoBehaviour
{
    [Header("Socket Settings")]
    [SerializeField] private int socketIndex;     // Index of this socket in the OperationConfig
    [SerializeField] private Image socketImage;   // Reference to the Image component
    
    [Header("Visual Feedback")]
    [SerializeField] private Image highlightImage;    // Optional highlight when wire is hovering
    [SerializeField] private float pulseAmount = 0.2f;
    [SerializeField] private float pulseDuration = 0.5f;
    
    // Private variables
    private RectTransform rectTransform;
    private OperationManager operationManager;
    private Color socketColor;
    private bool isConnected = false;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        operationManager = FindObjectOfType<OperationManager>();
    }
    
    private void Start()
    {
        // Apply socket color from the config if available
        if (operationManager != null && operationManager.Config != null)
        {
            if (socketIndex < operationManager.Config.leadColors.Length)
            {
                socketColor = operationManager.Config.leadColors[socketIndex];
                socketImage.color = socketColor;
            }
        }
        
        // Register this socket with the operation manager
        operationManager.RegisterSocket(this, socketIndex);
        
        // Configure the highlight, if present
        if (highlightImage != null)
        {
            Color highlightColor = socketColor;
            highlightColor.a = 0.5f;
            highlightImage.color = highlightColor;
            highlightImage.enabled = false;
        }
    }
    
    public Color GetColor()
    {
        return socketColor;
    }
    
    public int GetSocketIndex()
    {
        return socketIndex;
    }
    
    public Vector3 GetPosition()
    {
        return rectTransform.position;
    }
    
    public void SetConnected(bool connected)
    {
        isConnected = connected;
        
        // Visual feedback for connection could go here
        if (connected)
        {
            // Visual effect when connected
            if (highlightImage != null)
            {
                highlightImage.enabled = true;
            }
        }
    }
    
    public bool IsConnected()
    {
        return isConnected;
    }
    
    public void ShowHighlight(bool show)
    {
        if (highlightImage != null && !isConnected)
        {
            highlightImage.enabled = show;
        }
    }
    
    public void PulseFeedback()
    {
        // Could implement a visual pulse effect here using DOTween
        // if we want additional feedback for hovering or connections
    }
    
    public void Reset()
    {
        isConnected = false;
        if (highlightImage != null)
        {
            highlightImage.enabled = false;
        }
    }
}
