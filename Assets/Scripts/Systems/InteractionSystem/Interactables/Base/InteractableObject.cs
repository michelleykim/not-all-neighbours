using UnityEngine;
using UnityEngine.Events;
using NotAllNeighbours.Core.Enums;

namespace NotAllNeighbours.Interaction
{
  [RequireComponent(typeof(Collider))]
  public class InteractableObject : MonoBehaviour, IInteractable
  {
    [Header("Interaction Settings")]
    [SerializeField] private InteractionType interactionType;
    [SerializeField] private string interactionPrompt = "Examine";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool requiresProximity = false;
    [SerializeField] private float interactionRange = 2f;

    [Header("Visual Feedback")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private GameObject interactionIndicator;

    [Header("Photography Settings")]
    [Tooltip("Is this actual evidence (true) or a red herring (false)?")]
    [SerializeField] private bool isValidEvidence = false;

    [Tooltip("Description shown in journal when photographed.")]
    [SerializeField] private string evidenceDescription = "A potential evidence.";

    [Tooltip("Can only be photographed when using Investigate zoom.")]
    [SerializeField] private bool canOnlyBePhotographedWhenZoomed = false;

    [Tooltip("Visual indicator shownn after object has been photographed.")]
    [SerializeField] private GameObject photographedIndicator;

    [Tooltip("Disable interaction after photographing.")]
    [SerializeField] private bool disableAfterPhotograph = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onInteract;
    [SerializeField] private UnityEvent onHoverStart;
    [SerializeField] private UnityEvent onHoverEnd;

    private Material originalMaterial;
    private Renderer objectRenderer;
    private bool isHighlighted = false;
    private bool hasBeenPhotographed = false;

    // Public properties for photography system
    public bool IsValidEvidence => isValidEvidence;
    public bool HasBeenPhotographed => hasBeenPhotographed;
    public bool CanOnlyBePhotographedWhenZoomed => canOnlyBePhotographedWhenZoomed;
    public string InteractionPrompt => interactionPrompt;

    protected virtual void Awake()
    {
      objectRenderer = GetComponent<Renderer>();
      if (objectRenderer != null)
      {
        originalMaterial = objectRenderer.material;
      }

      if (interactionIndicator != null)
      {
        interactionIndicator.SetActive(false);
      }

      if (photographedIndicator != null)
      {
        photographedIndicator.SetActive(false);
      }
    }

    public InteractionType GetInteractionType()
    {
      return interactionType;
    }

    public virtual bool CanInteract()
    {
      if (!canInteract) return false;

      if (requiresProximity)
      {
        // Check distance from current camera position
        UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
        if (mainCamera != null)
        {
          float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
          return distance <= interactionRange;
        }
      }

      return true;
    }

    public virtual void OnHoverEnter()
    {
      if (!CanInteract()) return;

      isHighlighted = true;

      // Apply highlight effect
      if (objectRenderer != null && highlightMaterial != null)
      {
        objectRenderer.material = highlightMaterial;
      }

      // Show interaction indicator
      if (interactionIndicator != null)
      {
        interactionIndicator.SetActive(true);
      }

      onHoverStart?.Invoke();
    }

    public virtual void OnHoverExit()
    {
      isHighlighted = false;

      // Restore original material
      if (objectRenderer != null && originalMaterial != null)
      {
        objectRenderer.material = originalMaterial;
      }

      // Hide interaction indicator
      if (interactionIndicator != null)
      {
        interactionIndicator.SetActive(false);
      }

      onHoverEnd?.Invoke();
    }

    public virtual void OnInteract()
    {
      if (!CanInteract()) return;

      Debug.Log($"Interacting with {gameObject.name} - Type: {interactionType}");
      onInteract?.Invoke();
    }

    // Utility method for setting interaction state
    public void SetInteractable(bool state)
    {
      canInteract = state;
      if (!state && isHighlighted)
      {
        OnHoverExit();
      }
    }

    // Photography methods
    /// <summary>
    /// Mark this object as photographed.
    /// </summary>
    public void MarkAsPhotographed()
    {
      hasBeenPhotographed = true;

      // Show photographed indicator
      if (photographedIndicator != null)
      {
        photographedIndicator.SetActive(true);
      }

      // Disable further interaction if specified
      if (disableAfterPhotograph)
      {
        SetInteractable(false);
      }

      Debug.Log($"{gameObject.name} has been photographed");
    }

    /// <summary>
    /// Get the evidence description for this object.
    /// </summary>
    public string GetEvidenceDescription()
    {
      return evidenceDescription;
    }

    /// <summary>
    /// Check if this object can be photographed right now
    /// </summary>
    /// <param name="isZoomedIn">Is the player currently using Investigate zoom?</param>
    public bool CanPhotograph(bool isZoomedIn)
    {
      if (!CanInteract()) return false;

      // If requires zoom and not zoomed in, cannot photograph
      if (canOnlyBePhotographedWhenZoomed && !isZoomedIn)
      {
        return false;
      }

      // Can photograph
      return true;
    }

    /// <summary>
    /// Set whether this is valid evidence (for dynamic validation)
    /// </summary>
    public void SetValidEvidence(bool isValid)
    {
      isValidEvidence = isValid;
    }
  }
}