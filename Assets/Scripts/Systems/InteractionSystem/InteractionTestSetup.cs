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
                mainCamera = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
            }

            // Add RaycastDetector
            RaycastDetector raycastDetector = FindFirstObjectByType<RaycastDetector>();
            if (raycastDetector == null)
            {
                raycastDetector = mainCamera.gameObject.AddComponent<RaycastDetector>();
                Debug.Log("Created RaycastDetector on camera");
            }

            // Add InvestigationZoom
            InvestigationZoom investigationZoom = FindFirstObjectByType<InvestigationZoom>();
            if (investigationZoom == null)
            {
                investigationZoom = mainCamera.gameObject.AddComponent<InvestigationZoom>();
                Debug.Log("Created InvestigationZoom on camera");
            }

            // Add InteractionManager and wire up references
            InteractionManager interactionManager = FindFirstObjectByType<InteractionManager>();
            if (interactionManager == null)
            {
                GameObject managerObj = new GameObject("InteractionManager");
                interactionManager = managerObj.AddComponent<InteractionManager>();
                Debug.Log("Created InteractionManager");
            }

            // Wire up InteractionManager references using reflection
            var imType = typeof(InteractionManager);
            var raycastField = imType.GetField("raycastDetector", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var zoomField = imType.GetField("investigationZoom", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (raycastField != null)
            {
                raycastField.SetValue(interactionManager, raycastDetector);
                Debug.Log("Wired RaycastDetector to InteractionManager");
            }

            if (zoomField != null)
            {
                zoomField.SetValue(interactionManager, investigationZoom);
                Debug.Log("Wired InvestigationZoom to InteractionManager");
            }

            // Add CursorManager and wire up references
            CursorManager cursorManager = FindFirstObjectByType<CursorManager>();
            if (cursorManager == null)
            {
                GameObject cursorObj = new GameObject("CursorManager");
                cursorManager = cursorObj.AddComponent<CursorManager>();
                Debug.Log("Created CursorManager");
            }

            // Wire up CursorManager reference using reflection
            var cmType = typeof(CursorManager);
            var cmRaycastField = cmType.GetField("raycastDetector", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (cmRaycastField != null)
            {
                cmRaycastField.SetValue(cursorManager, raycastDetector);
                Debug.Log("Wired RaycastDetector to CursorManager");
            }

            Debug.Log("Interaction system setup complete!");
        }
    }
}