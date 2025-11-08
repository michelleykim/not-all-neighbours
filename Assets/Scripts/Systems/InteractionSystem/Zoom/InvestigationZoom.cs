using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace NotAllNeighbours.Interaction
{
    public class InvestigationZoom : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private float zoomDistance = 1.5f;
        [SerializeField] private float zoomSpeed = 2f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private bool allowRotation = true;
        
        [Header("UI")]
        [SerializeField] private GameObject zoomUI;
        [SerializeField] private GameObject exitPrompt;
        
        private bool isZoomed = false;
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;
        private GameObject currentInspectedObject;
        private IInteractable currentInteractable;
        
        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
            }
            
            if (zoomUI != null)
            {
                zoomUI.SetActive(false);
            }
            
            if (exitPrompt != null)
            {
                exitPrompt.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (isZoomed)
            {
                HandleZoomInput();
            }
        }
        
        public void ZoomToObject(IInteractable interactable)
        {
            if (isZoomed) return;
            
            MonoBehaviour interactableMono = interactable as MonoBehaviour;
            if (interactableMono == null) return;
            
            currentInteractable = interactable;
            currentInspectedObject = interactableMono.gameObject;
            
            // Store original camera state
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
            
            // Calculate zoom position
            Vector3 targetPosition = currentInspectedObject.transform.position;
            Vector3 directionToObject = (targetPosition - mainCamera.transform.position).normalized;
            Vector3 zoomPosition = targetPosition - directionToObject * zoomDistance;
            
            StartCoroutine(ZoomCameraCoroutine(zoomPosition, Quaternion.LookRotation(directionToObject)));
        }
        
        private IEnumerator ZoomCameraCoroutine(Vector3 targetPosition, Quaternion targetRotation)
        {
            isZoomed = true;
            
            // Show zoom UI
            if (zoomUI != null)
            {
                zoomUI.SetActive(true);
            }
            
            if (exitPrompt != null)
            {
                exitPrompt.SetActive(true);
            }
            
            float elapsedTime = 0f;
            Vector3 startPosition = mainCamera.transform.position;
            Quaternion startRotation = mainCamera.transform.rotation;
            
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * zoomSpeed;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime);
                
                mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                
                yield return null;
            }
        }
        
        private void HandleZoomInput()
        {
            var keyboard = Keyboard.current;

            var mouse = Mouse.current;

 

            if (keyboard == null || mouse == null) return;

 

            // Exit zoom with ESC or right-click

            if (keyboard.escapeKey.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)

            {

                ExitZoom();

                return;

            }

 

            // Rotate object with mouse drag

            if (allowRotation && mouse.leftButton.isPressed)

            {

                Vector2 mouseDelta = mouse.delta.ReadValue();

                float rotationX = mouseDelta.x * rotationSpeed * Time.deltaTime;

                float rotationY = mouseDelta.y * rotationSpeed * Time.deltaTime;
                
                if (currentInspectedObject != null)
                {
                    currentInspectedObject.transform.Rotate(Vector3.up, -rotationX, Space.World);
                    currentInspectedObject.transform.Rotate(Vector3.right, rotationY, Space.World);
                }
            }
        }
        
        public void ExitZoom()
        {
            if (!isZoomed) return;
            
            StartCoroutine(ExitZoomCoroutine());
        }
        
        private IEnumerator ExitZoomCoroutine()
        {
            float elapsedTime = 0f;
            Vector3 startPosition = mainCamera.transform.position;
            Quaternion startRotation = mainCamera.transform.rotation;
            
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * zoomSpeed;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime);
                
                mainCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, t);
                mainCamera.transform.rotation = Quaternion.Slerp(startRotation, originalCameraRotation, t);
                
                yield return null;
            }
            
            // Clean up
            isZoomed = false;
            currentInspectedObject = null;
            currentInteractable = null;
            
            if (zoomUI != null)
            {
                zoomUI.SetActive(false);
            }
            
            if (exitPrompt != null)
            {
                exitPrompt.SetActive(false);
            }
        }
        
        public bool IsZoomed()
        {
            return isZoomed;
        }
    }
}