using UnityEngine;

[CreateAssetMenu(fileName = "OperationConfig", menuName = "YTU2025/Operation Config")]
public class OperationConfig : ScriptableObject
{
    [Header("Socket & Lead Configuration")]
    public Color[] leadColors = new Color[] { Color.red, Color.green, Color.yellow, Color.blue, Color.magenta };
    
    [Header("References (Set at Runtime)")]
    public RectTransform[] socketTransforms; // Set in inspector or at runtime
    public RectTransform[] leadTransforms;   // Set in inspector or at runtime
    
    [Header("Timer Settings")]
    public float operationTime = 30f;           // Seconds allowed for the operation
    public float warningThreshold = 0.2f;       // Percentage (0-1) at which timer turns red
    
    [Header("Connection Settings")]
    public float snapRadius = 50f;              // Distance within which wires will snap to sockets (in pixels)
    
    [Header("Effects")]
    public float incorrectTimePenalty = 1f;     // Seconds deducted for incorrect connections
    public float screenShakeAmount = 0.1f;      // Amount of screen shake when timer is low
    public float screenShakeDuration = 0.2f;    // Duration of screen shake
    
    // Validate that our arrays are the same length (or will be)
    private void OnValidate()
    {
        // Make sure the arrays are initialized
        if (socketTransforms == null) socketTransforms = new RectTransform[leadColors.Length];
        if (leadTransforms == null) leadTransforms = new RectTransform[leadColors.Length];
        
        // Resize arrays if needed
        if (socketTransforms.Length != leadColors.Length)
        {
            System.Array.Resize(ref socketTransforms, leadColors.Length);
        }
        
        if (leadTransforms.Length != leadColors.Length)
        {
            System.Array.Resize(ref leadTransforms, leadColors.Length);
        }
    }
}
