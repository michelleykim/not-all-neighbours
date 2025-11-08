using UnityEngine;
using UnityEngine.Events;

namespace NotAllNeighbours.Interaction
{
    [RequireComponent(typeof(Collider))]
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [Tooltip("Primary interaction (left-click)")]
        [SerializeField] private InteractionType interactionType;
        [Tooltip("Secondary interaction (right-click) - set to None if not used")]
        [SerializeField] private InteractionType secondaryInteractionType = InteractionType.None;
        [SerializeField] private string interactionPrompt = "Examine";
        [SerializeField] private bool canInteract = true;
        [SerializeField] private bool requiresProximity = false;
        [SerializeField] private float interactionRange = 2f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private GameObject interactionIndicator;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private UnityEvent onHoverStart;
        [SerializeField] private UnityEvent onHoverEnd;
        
        private Material originalMaterial;
        private Renderer objectRenderer;
        private bool isHighlighted = false;
        
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
        }
        
        public InteractionType GetInteractionType()
        {
            return interactionType;
        }

        public InteractionType GetSecondaryInteractionType()
        {
            return secondaryInteractionType;
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
    }
}