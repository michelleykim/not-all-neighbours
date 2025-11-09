using UnityEngine;
using NotAllNeighbours.Core.Enums;
using NotAllNeighbours.Interaction;

namespace NotAllNeighbours.Systems.InteractionSystem.Interactables
{
  /// <summary>
  /// Door interaction for scene transitions
  /// No inventory key system - doors are either open or locked by story progression
  /// </summary>
  public class DoorInteraction : InteractableObject
  {
    [Header("Door Settings")]
    [SerializeField] private string _targetSceneName;
    [SerializeField] private Transform targetSpawnPoint;

    [Header("Lock State")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string lockReason = "This door is locked.";

    // Public property to access target scene name
    public string targetSceneName => _targetSceneName;

    [Header("Animation")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTrigger = "Open";

    public override bool CanInteract()
    {
      if (!base.CanInteract()) return false;

      // Simply check if locked
      // No inventory key checks
      return !isLocked;
    }

    public override void OnInteract()
    {
      if (!CanInteract())
      {
        Debug.Log($"Door is locked: {lockReason}");
        // TODO: Show UI message about why door is locked
        return;
      }

      // Play door animation
      if (doorAnimator != null)
      {
        doorAnimator.SetTrigger(openTrigger);
      }

      // Scene transition handled by InteractionManager
      Debug.Log($"Opening door to: {_targetSceneName}");

      base.OnInteract();
    }

    public void Unlock()
    {
      isLocked = false;
    }

    public void Lock()
    {
      isLocked = true;
    }
  }
}