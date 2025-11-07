using UnityEngine;

namespace NotAllNeighbours.Interaction
{
    public class RaycastDetector : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float raycastDistance = 100f;
        [SerializeField] private bool showDebugRay = true;
        
        [Header("Camera Reference")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        
        private IInteractable currentHoveredObject;
        private RaycastHit lastHit;
        
        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
            }
        }
        
        private void Update()
        {
            PerformRaycast();
        }
        
        private void PerformRaycast()
        {
            // Create ray from mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (showDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.yellow);
            }
            
            // Perform raycast
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayer))
            {
                lastHit = hit;
                
                // Try to get IInteractable component
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                
                if (interactable != null)
                {
                    // New object hovered
                    if (currentHoveredObject != interactable)
                    {
                        // Exit previous object
                        currentHoveredObject?.OnHoverExit();
                        
                        // Enter new object
                        currentHoveredObject = interactable;
                        currentHoveredObject.OnHoverEnter();
                    }
                }
                else
                {
                    // Hit something but not interactable
                    ClearCurrentHover();
                }
            }
            else
            {
                // No hit
                ClearCurrentHover();
            }
        }
        
        private void ClearCurrentHover()
        {
            if (currentHoveredObject != null)
            {
                currentHoveredObject.OnHoverExit();
                currentHoveredObject = null;
            }
        }
        
        public IInteractable GetCurrentHoveredObject()
        {
            return currentHoveredObject;
        }
        
        public RaycastHit GetLastHit()
        {
            return lastHit;
        }
        
        public bool IsHoveringInteractable()
        {
            return currentHoveredObject != null;
        }
    }
}