using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class WireController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IWireController
{
    [Header("Wire Settings")]
    [SerializeField] private int wireIndex;
    [SerializeField] private Image wireImage;
    
    [Header("Line Settings")]
    [SerializeField] private float lineThickness = 5f;   // Thickness of the wire line
    [Range(0.1f, 20f)]
    [SerializeField] private float lineThicknessMultiplier = 1f; // Multiplier for easy thickness adjustment
    
    // Debug
    [SerializeField] private bool debugMode = true;
    
    // Automatically assigned references
    private RectTransform rectTransform;
    private Canvas canvas;
    private Camera canvasCamera;
    private OperationManager operationManager;
    
    // Runtime variables
    private bool isDragging = false;
    private bool isConnected = false;
    private SocketController connectedSocket = null;
    private Color wireColor;
    
    // UI Line variables
    private GameObject lineObject;
    private RectTransform lineRectTransform;
    private Image lineImage;
    
    // Property to get effective line thickness
    private float EffectiveLineThickness => lineThickness * lineThicknessMultiplier;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasCamera = canvas.worldCamera; // This will be null for ScreenSpaceOverlay
        operationManager = FindObjectOfType<OperationManager>();
        
        // Create UI line
        CreateUILine();
    }
    
    private void CreateUILine()
    {
        // Create a UI line object
        lineObject = new GameObject("WireLine_" + wireIndex);
        lineObject.transform.SetParent(canvas.transform);
        
        lineRectTransform = lineObject.AddComponent<RectTransform>();
        lineImage = lineObject.AddComponent<Image>();
        
        // Set initial properties
        lineRectTransform.pivot = new Vector2(0, 0.5f); // Pivot at left center for rotation
        lineRectTransform.sizeDelta = new Vector2(0, EffectiveLineThickness); // No length initially
        lineImage.enabled = false;
    }
    
    private void Start()
    {
        // Get color from config
        if (operationManager != null && operationManager.Config != null)
        {
            if (wireIndex < operationManager.Config.leadColors.Length)
            {
                wireColor = operationManager.Config.leadColors[wireIndex];
                
                // Set wire image color
                if (wireImage != null)
                {
                    wireImage.color = wireColor;
                }
                
                // Set line colors
                if (lineImage != null)
                {
                    lineImage.color = wireColor;
                }
            }
        }
        
        // Register with operation manager
        operationManager.RegisterWire(this, wireIndex);
        
        if (debugMode) Debug.Log($"Wire {wireIndex} initialized with color {wireColor}, thickness: {EffectiveLineThickness}");
    }
    
    // Update is called once per frame to handle any runtime changes to thickness
    private void Update()
    {
        // If the thickness has changed and we're connected, update the line
        if (isConnected && lineRectTransform != null)
        {
            // Update only the thickness (Y component), keep the length (X component)
            lineRectTransform.sizeDelta = new Vector2(lineRectTransform.sizeDelta.x, EffectiveLineThickness);
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isConnected) return;
        
        isDragging = true;
        if (debugMode) Debug.Log($"Started dragging wire {wireIndex}");
        
        // Position line at wire's anchor
        Vector2 wireAnchoredPos = rectTransform.anchoredPosition;
        lineRectTransform.anchoredPosition = wireAnchoredPos;
        lineRectTransform.sizeDelta = new Vector2(0, EffectiveLineThickness); // Start with zero length
        lineImage.enabled = true;
        
        // Notify manager
        operationManager.OnWireDragStart(this);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // Get mouse position in canvas space
        Vector2 pointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvasCamera, 
            out pointerPos
        );
        
        // Get wire position in canvas space
        Vector2 wirePos = rectTransform.anchoredPosition;
        
        // Calculate direction and length for the line
        Vector2 direction = pointerPos - wirePos;
        float length = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Update line size, position and rotation
        lineRectTransform.anchoredPosition = wirePos;
        lineRectTransform.sizeDelta = new Vector2(length, EffectiveLineThickness);
        lineRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        
        if (debugMode && Time.frameCount % 30 == 0) // Log only every 30 frames to reduce spam
        {
            Debug.Log($"Wire {wireIndex} drag - Wire pos: {wirePos}, Pointer pos: {pointerPos}, Length: {length}");
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        if (debugMode) Debug.Log($"Ended dragging wire {wireIndex}");
        
        // Get final mouse position in canvas space
        Vector2 pointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvasCamera,
            out pointerPos
        );
        
        // Convert to world position for socket detection
        Vector3 pointerWorldPos = canvas.transform.TransformPoint(pointerPos);
        if (debugMode) Debug.Log($"Wire {wireIndex} drop position (canvas local): {pointerPos}, world: {pointerWorldPos}");
        
        // Find closest socket
        SocketController closestSocket = operationManager.FindClosestSocket(pointerWorldPos, wireIndex);
        
        if (closestSocket != null)
        {
            if (debugMode) Debug.Log($"Found closest socket with index {closestSocket.GetSocketIndex()}");
            ConnectToSocket(closestSocket);
        }
        else
        {
            if (debugMode) Debug.Log($"No socket found for wire {wireIndex}");
            
            // Hide line
            lineImage.enabled = false;
        }
        
        // Notify manager
        operationManager.OnWireDragEnd(this);
    }
    
    public void ConnectToSocket(SocketController socket)
    {
        if (debugMode) 
        {
            Debug.Log($"Attempting to connect wire {wireIndex} (color {ColorToHex(wireColor)}) to socket {socket.GetSocketIndex()} (color {ColorToHex(socket.GetColor())})");
        }
        
        // Check if colors match - use a tolerance for floating point comparison
        bool colorsMatch = ColorMatch(wireColor, socket.GetColor());
        
        if (!colorsMatch)
        {
            // Colors don't match - incorrect connection
            if (debugMode) 
            {
                Debug.Log($"Colors don't match! Wire: {ColorToHex(wireColor)}, Socket: {ColorToHex(socket.GetColor())}");
            }
            
            operationManager.OnIncorrectConnection();
            lineImage.enabled = false;
            return;
        }
        
        // Colors match - correct connection
        if (debugMode) 
        {
            Debug.Log($"Successful connection between wire {wireIndex} and socket {socket.GetSocketIndex()}");
        }
        
        isConnected = true;
        connectedSocket = socket;
        
        // Get socket position in canvas space
        Vector3 socketWorldPos = socket.GetPosition();
        Vector2 socketCanvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            RectTransformUtility.WorldToScreenPoint(canvasCamera, socketWorldPos),
            canvasCamera,
            out socketCanvasPos
        );
        
        // Get wire position in canvas space
        Vector2 wireCanvasPos = rectTransform.anchoredPosition;
        
        // Update line to connect wire and socket
        Vector2 direction = socketCanvasPos - wireCanvasPos;
        float length = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        lineRectTransform.anchoredPosition = wireCanvasPos;
        lineRectTransform.sizeDelta = new Vector2(length, EffectiveLineThickness);
        lineRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Notify manager
        operationManager.OnWireConnected(wireIndex);
        operationManager.OnCorrectConnection();
    }
    
    // Helper method to compare colors with tolerance
    private bool ColorMatch(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
    
    // Helper method to convert color to hex for debugging
    private string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }
    
    public bool IsConnected()
    {
        return isConnected;
    }
    
    public void Reset()
    {
        isConnected = false;
        connectedSocket = null;
        lineImage.enabled = false;
    }
    
    public int GetWireIndex()
    {
        return wireIndex;
    }
}
