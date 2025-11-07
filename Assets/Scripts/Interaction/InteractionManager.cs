using UnityEngine;
using UnityEngine.InputSystem;

namespace NotAllNeighbours.Interaction
{
    public class InteractionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RaycastDetector raycastDetector;
        [SerializeField] private InvestigationZoom investigationZoom;
        
        [Header("Input")]
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private InputActionReference alternateInteractAction;
        
        [Header("Settings")]
        [SerializeField] private bool enableInteraction = true;
        
        private void OnEnable()
        {
            if (interactAction != null)
            {
                interactAction.action.Enable();
                interactAction.action.performed += OnInteractPerformed;
            }
            
            if (alternateInteractAction != null)
            {
                alternateInteractAction.action.Enable();
                alternateInteractAction.action.performed += OnAlternateInteractPerformed;
            }
        }
        
        private void OnDisable()
        {
            if (interactAction != null)
            {
                interactAction.action.performed -= OnInteractPerformed;
                interactAction.action.Disable();
            }
            
            if (alternateInteractAction != null)
            {
                alternateInteractAction.action.performed -= OnAlternateInteractPerformed;
                alternateInteractAction.action.Disable();
            }
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!enableInteraction) return;
            
            IInteractable hoveredObject = raycastDetector.GetCurrentHoveredObject();
            
            if (hoveredObject != null && hoveredObject.CanInteract())
            {
                ProcessInteraction(hoveredObject);
            }
        }
        
        private void OnAlternateInteractPerformed(InputAction.CallbackContext context)
        {
            if (!enableInteraction) return;
            
            IInteractable hoveredObject = raycastDetector.GetCurrentHoveredObject();
            
            if (hoveredObject != null && hoveredObject.CanInteract())
            {
                ProcessAlternateInteraction(hoveredObject);
            }
        }
        
        private void ProcessInteraction(IInteractable interactable)
        {
            InteractionType type = interactable.GetInteractionType();
            
            switch (type)
            {
                case InteractionType.Examine:
                    HandleExamine(interactable);
                    break;
                    
                case InteractionType.Collect:
                    HandleCollect(interactable);
                    break;
                    
                case InteractionType.Investigate:
                    HandleInvestigate(interactable);
                    break;
                    
                case InteractionType.Talk:
                    HandleTalk(interactable);
                    break;
                    
                case InteractionType.Use:
                    HandleUse(interactable);
                    break;
                    
                case InteractionType.Document:
                    HandleDocument(interactable);
                    break;
                    
                case InteractionType.Door:
                    HandleDoor(interactable);
                    break;
                    
                default:
                    // Default interaction
                    interactable.OnInteract();
                    break;
            }
        }
        
        private void ProcessAlternateInteraction(IInteractable interactable)
        {
            // Right-click typically triggers detailed inspection
            if (investigationZoom != null)
            {
                investigationZoom.ZoomToObject(interactable);
            }
        }
        
        private void HandleExamine(IInteractable interactable)
        {
            // Show examination UI/text
            Debug.Log($"Examining: {interactable.InteractionPrompt}");
            interactable.OnInteract();
        }
        
        private void HandleCollect(IInteractable interactable)
        {
            // Add to inventory
            Debug.Log($"Collecting: {interactable.InteractionPrompt}");
            interactable.OnInteract();
            
            // TODO: Add to inventory system (Phase 1 Week 3-4)
        }
        
        private void HandleInvestigate(IInteractable interactable)
        {
            // Trigger investigation zoom
            if (investigationZoom != null)
            {
                investigationZoom.ZoomToObject(interactable);
            }
            
            interactable.OnInteract();
        }
        
        private void HandleTalk(IInteractable interactable)
        {
            // Trigger dialogue system
            Debug.Log($"Talking to: {interactable.InteractionPrompt}");
            interactable.OnInteract();
            
            // TODO: Integrate with dialogue system (Phase 1 Week 5-6)
        }
        
        private void HandleUse(IInteractable interactable)
        {
            // Use object (door, switch, etc.)
            Debug.Log($"Using: {interactable.InteractionPrompt}");
            interactable.OnInteract();
        }
        
        private void HandleDocument(IInteractable interactable)
        {
            // Photography/recording
            Debug.Log($"Documenting: {interactable.InteractionPrompt}");
            interactable.OnInteract();
            
            // TODO: Integrate with camera/recording system
        }
        
        private void HandleDoor(IInteractable interactable)
        {
            // Room transition
            Debug.Log($"Opening door: {interactable.InteractionPrompt}");
            interactable.OnInteract();
            
            // TODO: Integrate with scene transition system (Phase 1 Week 9-10)
        }
        
        public void SetInteractionEnabled(bool enabled)
        {
            enableInteraction = enabled;
        }
    }
}