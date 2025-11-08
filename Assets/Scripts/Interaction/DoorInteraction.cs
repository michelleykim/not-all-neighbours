using UnityEngine;

namespace NotAllNeighbours.Interaction
{
    public class DoorInteraction : InteractableObject
    {
        [Header("Door Settings")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private Transform targetSpawnPoint;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string requiredKeyID;
        
        [Header("Animation")]
        [SerializeField] private Animator doorAnimator;
        [SerializeField] private string openTrigger = "Open";
        
        private InventorySystem inventorySystem;
        
        protected override void Awake()
        {
            base.Awake();
            inventorySystem = FindObjectOfType<InventorySystem>();
        }
        
        public override bool CanInteract()
        {
            if (!base.CanInteract()) return false;
            
            if (isLocked)
            {
                // Check if player has required key
                if (!string.IsNullOrEmpty(requiredKeyID) && inventorySystem != null)
                {
                    return inventorySystem.HasItem(requiredKeyID);
                }
                return false;
            }
            
            return true;
        }
        
        public override void OnInteract()
        {
            if (!CanInteract())
            {
                Debug.Log("Door is locked!");
                return;
            }
            
            // Play door animation
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger(openTrigger);
            }
            
            // TODO: Integrate with scene transition system (Phase 1 Week 9-10)
            Debug.Log($"Opening door to: {targetSceneName}");
            
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