using UnityEngine;

namespace NotAllNeighbours.Interaction
{
    public class InteractionTestSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetup = true;
        
        private void Start()
        {
            if (autoSetup)
            {
                SetupInteractionSystem();
            }
        }
        
        private void SetupInteractionSystem()
        {
            // Find or create main camera
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCamera = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            
            // Add RaycastDetector
            RaycastDetector raycastDetector = FindObjectOfType<RaycastDetector>();
            if (raycastDetector == null)
            {
                raycastDetector = mainCamera.gameObject.AddComponent<RaycastDetector>();
            }
            
            // Add InteractionManager
            InteractionManager interactionManager = FindObjectOfType<InteractionManager>();
            if (interactionManager == null)
            {
                GameObject managerObj = new GameObject("InteractionManager");
                interactionManager = managerObj.AddComponent<InteractionManager>();
            }
            
            // Add InvestigationZoom
            InvestigationZoom investigationZoom = FindObjectOfType<InvestigationZoom>();
            if (investigationZoom == null)
            {
                investigationZoom = mainCamera.gameObject.AddComponent<InvestigationZoom>();
            }
            
            // Add InventorySystem
            InventorySystem inventorySystem = FindObjectOfType<InventorySystem>();
            if (inventorySystem == null)
            {
                GameObject invObj = new GameObject("InventorySystem");
                inventorySystem = invObj.AddComponent<InventorySystem>();
            }
            
            // Add CursorManager
            CursorManager cursorManager = FindObjectOfType<CursorManager>();
            if (cursorManager == null)
            {
                GameObject cursorObj = new GameObject("CursorManager");
                cursorManager = cursorObj.AddComponent<CursorManager>();
            }
            
            Debug.Log("Interaction system setup complete!");
        }
    }
}