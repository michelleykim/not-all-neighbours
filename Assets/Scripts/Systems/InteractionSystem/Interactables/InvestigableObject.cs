using UnityEngine;

namespace NotAllNeighbours.System.InteractionSystem.Interactables
{
  /// <summary>
  /// Interactable object that can be investigated with camera zoom
  /// Left-click to zoom in for detailed inspection
  /// Some items can only be photographed when zoomed in
  /// </summary>
  public class InvestigableObject : InteractableObject
  {
    [Header("Investigation Settings")]
    [SerializeField] private bool requiresZoomForPhotography = false;
    [SerializeField] private string investigationDescription = "Investigate this closely...";
    [SerializeField] private bool revealsHiddenItem = false;
    [SerializeField] private GameObject hiddenItemPrefab;
    [SerializeField] private Transform hiddenItemSpawnPoint;

    [Header("Zoom Settings")]
    [SerializeField] private float customZoomDistance = 1.5f;

    private bool hasBeenInvestigated = false;
    private GameObject revealedItem;

    public bool RequiresZoomForPhotography => requiresZoomForPhotography;
    public bool HasBeenInvestigated => hasBeenInvestigated;
    public float ZoomDistance => customZoomDistance;

    public override void OnInteract()
    {
      if (!hasBeenInvestigated)
      {
        hasBeenInvestigated = true;

        // Reveal hidden item if configured
        if (revealsHiddenItem && hiddenItemPrefab != null)
        {
          RevealHiddenItem();
        }
      }

      base.OnInteract();
    }

    /// <summary>
    /// Reveal a hidden item when investigated
    /// </summary>
    private void RevealHiddenItem()
    {
      if (revealedItem != null) return;

      Vector3 spawnPosition = hiddenItemSpawnPoint != null ?
          hiddenItemSpawnPoint.position :
          transform.position;

      revealedItem = Instantiate(hiddenItemPrefab, spawnPosition, Quaternion.identity);
      Debug.Log($"InvestigableObject: Revealed hidden item at {gameObject.name}");
    }

    /// <summary>
    /// Check if this object can be photographed in current state
    /// </summary>
    public bool CanBePhotographed(bool isZoomedIn)
    {
      if (requiresZoomForPhotography)
      {
        return isZoomedIn;
      }
      return true;
    }

    /// <summary>
    /// Get the investigation description
    /// </summary>
    public string GetInvestigationDescription()
    {
      return investigationDescription;
    }
  }
}
