using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NotAllNeighbours.Interaction;



namespace NotAllNeighbours.Camera

{

  /// <summary>

  /// Manages fixed camera positions and smooth transitions in first-person perspective.

  /// Handles 360-degree rotation at each fixed position.

  /// Uses Unity's New Input System.

  /// </summary>

  [RequireComponent(typeof(UnityEngine.Camera))]

  public class CameraManager : MonoBehaviour

  {

    [Header("Camera References")]

    [Tooltip("Main camera component (auto-assigned if null)")]

    [SerializeField] private UnityEngine.Camera mainCamera;

    [Tooltip("Investigation zoom component (optional, prevents position switching when zoomed)")]

    [SerializeField] private InvestigationZoom investigationZoom;
    [Header("Input Actions")]
    [Tooltip("Input Actions asset for camera controls")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Camera Positions")]
    [Tooltip("All available camera positions in this room")]
    [SerializeField] private List<CameraPosition> cameraPositions = new List<CameraPosition>();

    [Tooltip("Starting camera position index")]
    [SerializeField] private int startingPositionIndex = 0;

    [Header("Transition Settings")]
    [Tooltip("Speed of camera transitions between positions (seconds)")]
    [Range(0.1f, 2f)]
    [SerializeField] private float transitionSpeed = 0.4f;

    [Tooltip("Animation curve for smooth transitions")]
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Rotation Settings")]
    [Tooltip("Enable/disable camera rotation at current position")]
    [SerializeField] private bool allowRotation = true;

    [Tooltip("Mouse rotation sensitivity")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(3f, 2f);

    [Tooltip("Smooth rotation damping")]
    [Range(0f, 0.3f)]
    [SerializeField] private float rotationSmoothing = 0.1f;

    [Tooltip("Invert vertical mouse axis")]
    [SerializeField] private bool invertVerticalAxis = false;

    [Header("Debug")]
    [Tooltip("Show debug information in console")]
    [SerializeField] private bool debugMode = false;

    [Tooltip("Show gizmos in scene view")]
    [SerializeField] private bool showGizmos = true;

    // Input Actions
    private InputAction nextPositionAction;
    private InputAction previousPositionAction;
    private InputAction lookAction;

    // Internal state
    private int currentPositionIndex = 0;
    private bool isTransitioning = false;
    private float currentHorizontalRotation = 0f;
    private float currentVerticalRotation = 0f;
    private float targetHorizontalRotation = 0f;
    private float targetVerticalRotation = 0f;
    private Vector2 rotationVelocity = Vector2.zero;

    // Events
    public delegate void CameraPositionChanged(CameraPosition newPosition, CameraPosition oldPosition);
    public event CameraPositionChanged OnCameraPositionChanged;

    #region Properties
    public CameraPosition CurrentPosition => GetCurrentPosition();
    public int CurrentPositionIndex => currentPositionIndex;
    public bool IsTransitioning => isTransitioning;
    public bool RotationEnabled => allowRotation;
    public int TotalPositions => cameraPositions.Count;
    #endregion

    #region Initialization
    private void Awake()
    {
      // Auto-assign camera if not set
      if (mainCamera == null)
      {
        mainCamera = GetComponent<UnityEngine.Camera>();
      }

      // Auto-find InvestigationZoom if not assigned
      if (investigationZoom == null)
      {
        investigationZoom = FindFirstObjectByType<InvestigationZoom>();
      }

      // Set up Input Actions
      SetupInputActions();
    }

    private void OnEnable()
    {
      // Enable input actions
      if (nextPositionAction != null) nextPositionAction.Enable();
      if (previousPositionAction != null) previousPositionAction.Enable();
      if (lookAction != null) lookAction.Enable();
    }

    private void OnDisable()
    {
      // Disable input actions
      if (nextPositionAction != null) nextPositionAction.Disable();
      if (previousPositionAction != null) previousPositionAction.Disable();
      if (lookAction != null) lookAction.Disable();
    }

    private void SetupInputActions()
    {
      if (inputActions == null)
      {
        Debug.LogError("[CameraManager] Input Actions asset not assigned! Please assign it in the Inspector.");
        return;
      }

      // Get the Camera action map
      var cameraMap = inputActions.FindActionMap("Camera");
      if (cameraMap == null)
      {
        Debug.LogError("[CameraManager] 'Camera' action map not found in Input Actions!");
        return;
      }

      // Get individual actions
      nextPositionAction = cameraMap.FindAction("NextPosition");
      previousPositionAction = cameraMap.FindAction("PreviousPosition");
      lookAction = cameraMap.FindAction("Look");

      // Subscribe to button press events
      if (nextPositionAction != null)
      {
        nextPositionAction.performed += ctx => SwitchToNextPosition();
      }

      if (previousPositionAction != null)
      {
        previousPositionAction.performed += ctx => SwitchToPreviousPosition();
      }

      // Add number key bindings for direct position access
      for (int i = 1; i <= 9; i++)
      {
        int positionIndex = i - 1; // Capture for closure
        var action = cameraMap.FindAction($"Position{i}");
        if (action != null)
        {
          action.performed += ctx => SwitchToPosition(positionIndex);
        }
      }

      if (debugMode)
      {
        Debug.Log("[CameraManager] Input Actions set up successfully");
      }
    }

    private void Start()
    {
      ValidateCameraPositions();
      InitializeCamera();
    }

    private void InitializeCamera()
    {
      if (cameraPositions.Count == 0)
      {
        Debug.LogError("[CameraManager] No camera positions assigned!");
        return;
      }

      // Clamp starting index
      startingPositionIndex = Mathf.Clamp(startingPositionIndex, 0, cameraPositions.Count - 1);
      currentPositionIndex = startingPositionIndex;

      // Move to starting position immediately
      CameraPosition startPos = cameraPositions[currentPositionIndex];
      transform.position = startPos.Position;
      transform.rotation = startPos.DefaultRotation;
      mainCamera.fieldOfView = startPos.FieldOfView;

      // Initialize rotation state
      Vector3 euler = startPos.DefaultRotation.eulerAngles;
      currentHorizontalRotation = euler.y;
      currentVerticalRotation = NormalizeAngle(euler.x);
      targetHorizontalRotation = currentHorizontalRotation;
      targetVerticalRotation = currentVerticalRotation;

      if (debugMode)
      {
        Debug.Log($"[CameraManager] Initialized at position: {startPos.PositionName}");
      }
    }

    private void ValidateCameraPositions()
    {
      if (cameraPositions == null || cameraPositions.Count == 0)
      {
        Debug.LogWarning("[CameraManager] Attempting to auto-find camera positions...");
        CameraPosition[] foundPositions = FindObjectsByType<CameraPosition>(FindObjectsSortMode.None);
        if (foundPositions.Length > 0)
        {
          cameraPositions = foundPositions.OrderBy(p => p.PositionIndex).ToList();
          Debug.Log($"[CameraManager] Found {cameraPositions.Count} camera positions");
        }
        else
        {
          Debug.LogError("[CameraManager] No camera positions found in scene!");
        }
      }

      // Remove null references
      cameraPositions.RemoveAll(pos => pos == null);

      // Sort by index
      cameraPositions = cameraPositions.OrderBy(p => p.PositionIndex).ToList();

      // Validate indices
      for (int i = 0; i < cameraPositions.Count; i++)
      {
        if (cameraPositions[i].PositionIndex != i)
        {
          Debug.LogWarning($"[CameraManager] Camera position index mismatch: {cameraPositions[i].PositionName} (expected {i}, got {cameraPositions[i].PositionIndex})");
        }
      }
    }
    #endregion

    #region Update Loop
    private void Update()
    {
      if (isTransitioning) return;

      HandleRotation();
    }

    private void HandleRotation()
    {
      if (!allowRotation || lookAction == null) return;

      // Get mouse input from Input System
      Vector2 lookDelta = lookAction.ReadValue<Vector2>();

      // Apply sensitivity
      float mouseX = lookDelta.x * mouseSensitivity.x * Time.deltaTime;
      float mouseY = lookDelta.y * mouseSensitivity.y * Time.deltaTime;

      if (invertVerticalAxis)
      {
        mouseY = -mouseY;
      }

      // Update target rotations
      targetHorizontalRotation += mouseX;
      targetVerticalRotation -= mouseY;

      // Clamp vertical rotation to current position limits
      CameraPosition currentPos = GetCurrentPosition();
      if (currentPos != null)
      {
        targetVerticalRotation = Mathf.Clamp(
            targetVerticalRotation,
            currentPos.MinVerticalAngle,
            currentPos.MaxVerticalAngle
        );
      }

      // Smooth rotation using SmoothDamp
      currentHorizontalRotation = Mathf.SmoothDampAngle(
          currentHorizontalRotation,
          targetHorizontalRotation,
          ref rotationVelocity.x,
          rotationSmoothing
      );

      currentVerticalRotation = Mathf.SmoothDampAngle(
          currentVerticalRotation,
          targetVerticalRotation,
          ref rotationVelocity.y,
          rotationSmoothing
      );

      // Apply rotation
      transform.rotation = Quaternion.Euler(currentVerticalRotation, currentHorizontalRotation, 0f);
    }
    #endregion

    #region Position Switching
    /// <summary>
    /// Switch to the next camera position in sequence
    /// </summary>
    public void SwitchToNextPosition()
    {
      if (isTransitioning || cameraPositions.Count == 0) return;

      // Prevent switching while zoomed in during investigation
      if (investigationZoom != null && investigationZoom.IsZoomed())
      {
        if (debugMode)
        {
          Debug.Log("[CameraManager] Cannot switch positions while zoomed in");
        }
        return;
      }

      int nextIndex = (currentPositionIndex + 1) % cameraPositions.Count;
      SwitchToPosition(nextIndex);
    }

    /// <summary>
    /// Switch to the previous camera position in sequence
    /// </summary>
    public void SwitchToPreviousPosition()
    {
      if (isTransitioning || cameraPositions.Count == 0) return;

      // Prevent switching while zoomed in during investigation

      if (investigationZoom != null && investigationZoom.IsZoomed())
      {
        if (debugMode)
        {
          Debug.Log("[CameraManager] Cannot switch positions while zoomed in");
        }
        return;
      }

      int prevIndex = currentPositionIndex - 1;
      if (prevIndex < 0) prevIndex = cameraPositions.Count - 1;
      SwitchToPosition(prevIndex);
    }

    /// <summary>
    /// Switch to a specific camera position by index
    /// </summary>
    public void SwitchToPosition(int index)
    {
      if (isTransitioning) return;

      // Prevent switching while zoomed in during investigation
      if (investigationZoom != null && investigationZoom.IsZoomed())
      {
        if (debugMode)
        {
          Debug.Log("[CameraManager] Cannot switch positions while zoomed in");
        }
        return;
      }

      if (index < 0 || index >= cameraPositions.Count)
      {
        Debug.LogWarning($"[CameraManager] Invalid position index: {index}");
        return;
      }
      if (index == currentPositionIndex) return;

      CameraPosition oldPosition = GetCurrentPosition();
      CameraPosition newPosition = cameraPositions[index];

      if (debugMode)
      {
        Debug.Log($"[CameraManager] Switching from '{oldPosition?.PositionName}' to '{newPosition.PositionName}'");
      }

      StartCoroutine(TransitionToPosition(newPosition, oldPosition, index));
    }

    /// <summary>
    /// Smooth transition between camera positions
    /// </summary>
    private IEnumerator TransitionToPosition(CameraPosition target, CameraPosition source, int targetIndex)
    {
      isTransitioning = true;

      Vector3 startPosition = transform.position;
      Quaternion startRotation = transform.rotation;
      float startFOV = mainCamera.fieldOfView;

      Vector3 targetPosition = target.Position;
      Quaternion targetRotation = target.DefaultRotation;
      float targetFOV = target.FieldOfView;

      float elapsed = 0f;

      while (elapsed < transitionSpeed)
      {
        elapsed += Time.deltaTime;
        float t = transitionCurve.Evaluate(elapsed / transitionSpeed);

        // Interpolate position and rotation
        transform.position = Vector3.Lerp(startPosition, targetPosition, t);
        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
        mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);

        yield return null;
      }

      // Ensure final values are exact
      transform.position = targetPosition;
      transform.rotation = targetRotation;
      mainCamera.fieldOfView = targetFOV;

      // Update rotation state
      Vector3 euler = targetRotation.eulerAngles;
      currentHorizontalRotation = euler.y;
      currentVerticalRotation = NormalizeAngle(euler.x);
      targetHorizontalRotation = currentHorizontalRotation;
      targetVerticalRotation = currentVerticalRotation;
      rotationVelocity = Vector2.zero;

      // Update current position
      currentPositionIndex = targetIndex;
      isTransitioning = false;

      // Trigger event
      OnCameraPositionChanged?.Invoke(target, source);

      if (debugMode)
      {
        Debug.Log($"[CameraManager] Transition complete. Now at: {target.PositionName}");
      }
    }
    #endregion

    #region Rotation Control
    /// <summary>
    /// Enable or disable camera rotation
    /// </summary>
    public void SetRotationEnabled(bool enabled)
    {
      allowRotation = enabled;
      if (debugMode)
      {
        Debug.Log($"[CameraManager] Rotation {(enabled ? "enabled" : "disabled")}");
      }
    }

    /// <summary>
    /// Reset camera to default rotation of current position
    /// </summary>
    public void ResetToDefaultRotation()
    {
      CameraPosition currentPos = GetCurrentPosition();
      if (currentPos == null) return;

      Vector3 euler = currentPos.DefaultRotation.eulerAngles;
      targetHorizontalRotation = euler.y;
      targetVerticalRotation = NormalizeAngle(euler.x);
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Get the current camera position data
    /// </summary>
    public CameraPosition GetCurrentPosition()
    {
      if (currentPositionIndex >= 0 && currentPositionIndex < cameraPositions.Count)
      {
        return cameraPositions[currentPositionIndex];
      }
      return null;
    }

    /// <summary>
    /// Normalize angle to -180 to 180 range
    /// </summary>
    private float NormalizeAngle(float angle)
    {
      while (angle > 180f) angle -= 360f;
      while (angle < -180f) angle += 360f;
      return angle;
    }

    /// <summary>
    /// Get position by index
    /// </summary>
    public CameraPosition GetPositionByIndex(int index)
    {
      if (index >= 0 && index < cameraPositions.Count)
      {
        return cameraPositions[index];
      }
      return null;
    }

    /// <summary>
    /// Find position by name
    /// </summary>
    public CameraPosition FindPositionByName(string name)
    {
      return cameraPositions.FirstOrDefault(p => p.PositionName.Equals(name, System.StringComparison.OrdinalIgnoreCase));
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
      if (!showGizmos || cameraPositions == null || cameraPositions.Count == 0) return;

      // Draw lines connecting camera positions
      Gizmos.color = Color.cyan;
      for (int i = 0; i < cameraPositions.Count - 1; i++)
      {
        if (cameraPositions[i] != null && cameraPositions[i + 1] != null)
        {
          Gizmos.DrawLine(cameraPositions[i].Position, cameraPositions[i + 1].Position);
        }
      }

      // Draw current position in different color
      if (Application.isPlaying && GetCurrentPosition() != null)
      {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GetCurrentPosition().Position, 0.5f);
      }
    }
    #endregion
  }
}