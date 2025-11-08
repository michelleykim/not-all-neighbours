using UnityEngine;
using NotAllNeighbours.Data.Interaction;
using NotAllNeighbours.Interaction;

namespace NotAllNeighbours.Interaction
{
  public class EvidenceObject : InteractableObject
  {
    [Header("Evidence Data")]
    [SerializeField] private InventoryItem evidenceItem;
    [SerializeField] private bool autoDestroy = true;

    private InventorySystem inventorySystem;

    protected override void Awake()
    {
      base.Awake();
      inventorySystem = FindObjectOfType<InventorySystem>();
    }

    public override void OnInteract()
    {
      if (!CanInteract()) return;

      if (inventorySystem != null && evidenceItem != null)
      {
        if (inventorySystem.AddItem(evidenceItem))
        {
          Debug.Log($"Collected evidence: {evidenceItem.itemName}");

          if (autoDestroy)
          {
            Destroy(gameObject);
          }
          else
          {
            SetInteractable(false);
          }
        }
      }

      base.OnInteract();
    }
  }
}