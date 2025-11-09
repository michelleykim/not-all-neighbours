using UnityEngine;

namespace NotAllNeighbours.Systems.InteractionSystem.Interactables
{
  /// <summary>
  /// Interactable object that can be photographed for evidence
  /// Right-click to photograph and add to journal
  /// GDD: No physical inventory - all evidence collected via photography
  /// </summary>
  public class CollectableObject : InteractableObject
  {
    [Header("Evidence Settings")]
    [Tooltip("Is this actual evidence (true) or a red herring (false)?")]
    [SerializeField] private bool isValidEvidence = false;

    [Tooltip("Description shown in journal when photographed")]
    [TextArea(2, 4)]
    [SerializeField] private string evidenceDescription = "Potential evidence";

    [Tooltip("Can only be photographed when using Investigate zoom")]
    [SerializeField] private bool canOnlyBePhotographedWhenZoomed = false;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject photographedIndicator;
    [SerializeField] private bool disableAfterPhotograph = false;

    private bool hasBeenPhotographed = false;

    public bool IsValidEvidence => isValidEvidence;
    public bool HasBeenPhotographed => hasBeenPhotographed;
    public bool CanOnlyBePhotographedWhenZoomed => canOnlyBePhotographedWhenZoomed;

    protected override void Awake()
    {
      base.Awake();

      if (photographedIndicator != null)
      {
        photographedIndicator.SetActive(false);
      }
    }

    public override void OnInteract()
    {
      // This is called when photographed (right-click handled by InteractionManager)
      MarkAsPhotographed();
      base.OnInteract();
    }

    /// <summary>
    /// Mark this object as photographed
    /// </summary>
    public void MarkAsPhotographed()
    {
      hasBeenPhotographed = true;

      // Show indicator
      if (photographedIndicator != null)
      {
        photographedIndicator.SetActive(true);
      }

      // Disable interaction if configured
      if (disableAfterPhotograph)
      {
        SetInteractable(false);
      }

      Debug.Log($"CollectableObject: {gameObject.name} has been photographed");
    }

    /// <summary>
    /// Get the evidence description for the journal
    /// </summary>
    public string GetEvidenceDescription()
    {
      return evidenceDescription;
    }

    /// <summary>
    /// Check if this object can be photographed right now
    /// </summary>
    /// <param name="isZoomedIn">Is player currently using Investigate zoom?</param>
    public bool CanPhotograph(bool isZoomedIn)
    {
      // If requires zoom and not zoomed, can't photograph
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
    public void SetValidEvidence(bool valid)
    {
      isValidEvidence = valid;
    }
  }
}