using UnityEngine;
using UnityEngine.InputSystem;
using NotAllNeighbours.UI;
using NotAllNeighbours.Managers;
using NotAllNeighbours.Evidence;
using NotAllNeighbours.Dialogue;
using NotAllNeighbours.NPC;

namespace NotAllNeighbours.Interaction
{
    public class InteractionManager : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private RaycastDetector raycastDetector;
        [SerializeField] private InvestigationZoom investigationZoom;
        [SerializeField] private ExamineUI examineUI;
        [SerializeField] private SceneTransitionManager sceneTransitionManager;
        [SerializeField] private PhotographySystem photographySystem;
        [SerializeField] private DialogueManager dialogueManager;

        [Header("Input")]
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private InputActionReference alternateInteractAction;
        [SerializeField] private bool useFallbackInput = true;

        [Header("Settings")]
        [SerializeField] private bool enableInteraction = true;
        
        private void Awake()
        {
            if (raycastDetector == null)
            {
                raycastDetector = FindObjectOfType<RaycastDetector>();
            }

            if (investigationZoom == null)
            {
                investigationZoom = FindObjectOfType<InvestigationZoom>();
            }

            if (examineUI == null)
            {
                examineUI = ExamineUI.Instance;
            }

            if (sceneTransitionManager == null)
            {
                sceneTransitionManager = SceneTransitionManager.Instance;
            }

            if (photographySystem == null)
            {
                photographySystem = PhotographySystem.Instance;
            }

            if (dialogueManager == null)
            {
                dialogueManager = DialogueManager.Instance;
            }
        }

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

        private void Update()
        {
            // Fallback input when InputActionReferences are not set up
            if (useFallbackInput && (interactAction == null || alternateInteractAction == null))
            {
                // Use new Input System
                var mouse = Mouse.current;
                var keyboard = Keyboard.current;

              if (mouse != null && keyboard != null)
              {
                if (mouse.leftButton.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame)
                {
                  HandleInteractInput();
                }

                if (mouse.rightButton.wasPressedThisFrame)
                {
                  HandleAlternateInteractInput();
                }
              }
            }
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            HandleInteractInput();
        }

        private void OnAlternateInteractPerformed(InputAction.CallbackContext context)
        {
            HandleAlternateInteractInput();
        }

        private void HandleInteractInput()
        {
            if (!enableInteraction) return;

            if (raycastDetector == null)
            {
                Debug.LogWarning("InteractionManager: RaycastDetector is not assigned!");
                return;
            }

            IInteractable hoveredObject = raycastDetector.GetCurrentHoveredObject();

            if (hoveredObject != null && hoveredObject.CanInteract())
            {
                ProcessInteraction(hoveredObject);
            }
        }

        private void HandleAlternateInteractInput()
        {
            if (!enableInteraction) return;

            if (raycastDetector == null)
            {
                Debug.LogWarning("InteractionManager: RaycastDetector is not assigned!");
                return;
            }

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
                    // COLLECT is handled by right-click only (ProcessAlternateInteraction)
                    Debug.Log("Use right-click to photograph evidence");
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
            // Get secondary interaction type (right-click)
            InteractionType secondaryType = interactable.GetSecondaryInteractionType();

            // If no secondary interaction is set, fall back to investigation zoom
            if (secondaryType == InteractionType.None)
            {
                // For objects without secondary interaction, right-click can trigger investigation zoom
                if (investigationZoom != null)
                {
                    investigationZoom.ZoomToObject(interactable);
                }
                return;
            }

            // Handle the secondary interaction type
            switch (secondaryType)
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
                    // Default to investigation zoom
                    if (investigationZoom != null)
                    {
                        investigationZoom.ZoomToObject(interactable);
                    }
                    break;
            }
        }
        
        private void HandleExamine(IInteractable interactable)
        {
            // Show examination UI/text
            if (examineUI != null)
            {
                examineUI.ShowExamineText(interactable.InteractionPrompt);
            }
            else
            {
                Debug.Log($"Examining: {interactable.InteractionPrompt}");
            }

            interactable.OnInteract();
        }
        
        private void HandleCollect(IInteractable interactable)
        {
            // Take photograph for evidence collection
            if (photographySystem != null)
            {
                MonoBehaviour interactableMono = interactable as MonoBehaviour;
                if (interactableMono != null)
                {
                    // Get evidence data from the interactable object
                    string objectName = interactableMono.gameObject.name;
                    string description = interactable.InteractionPrompt;
                    bool isValidEvidence = false;
                    bool isZoomedIn = investigationZoom != null && investigationZoom.IsZoomed();

                    // Check for CollectableObject component first
                    var collectableObj = interactableMono.GetComponent<CollectableObject>();
                    if (collectableObj != null)
                    {
                        isValidEvidence = collectableObj.IsValidEvidence;
                        description = collectableObj.GetEvidenceDescription();

                        // Check if can be photographed when zoomed
                        if (!collectableObj.CanPhotograph(isZoomedIn))
                        {
                            Debug.Log("This evidence can only be photographed when zoomed in");
                            return;
                        }
                    }
                    // Check for DoorInteraction (door that can be photographed)
                    else if (interactableMono.TryGetComponent<DoorInteraction>(out var doorInteraction))
                    {
                        isValidEvidence = doorInteraction.IsValidEvidence;
                        description = doorInteraction.EvidenceDescription;
                    }
                    // Check for ExaminableObject
                    else if (interactableMono.TryGetComponent<ExaminableObject>(out var examinableObj))
                    {
                        // Examinable objects can be photographed with their examination text as description
                        description = examinableObj.InteractionPrompt;
                        isValidEvidence = examinableObj.IsValidEvidence;
                    }
                    // Check for InvestigableObject
                    else if (interactableMono.TryGetComponent<InvestigableObject>(out var investigableObj))
                    {
                        description = investigableObj.GetInvestigationDescription();
                        isValidEvidence = investigableObj.IsValidEvidence;

                        // Check if requires zoom for photography
                        if (investigableObj.RequiresZoomForPhotography && !isZoomedIn)
                        {
                            Debug.Log("This evidence can only be photographed when zoomed in");
                            return;
                        }
                    }
                    // Fallback to EvidenceObject for backwards compatibility
                    else if (interactableMono.TryGetComponent<EvidenceObject>(out var evidenceObj))
                    {
                        isValidEvidence = true;
                    }

                    photographySystem.TakePhoto(objectName, description, isValidEvidence);
                }
            }
            else
            {
                Debug.Log($"Collecting: {interactable.InteractionPrompt}");
            }

            interactable.OnInteract();
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
            if (dialogueManager != null)
            {
                MonoBehaviour interactableMono = interactable as MonoBehaviour;
                if (interactableMono != null)
                {
                    // Get dialogue node from the NPC
                    var npc = interactableMono.GetComponent<NPCDialogue>();
                    if (npc != null && npc.GetDialogueNode() != null)
                    {
                        dialogueManager.StartDialogue(npc.GetDialogueNode());
                    }
                    else
                    {
                        Debug.LogWarning($"No dialogue data found for {interactableMono.gameObject.name}");
                    }
                }
            }
            else
            {
                Debug.Log($"Talking to: {interactable.InteractionPrompt}");
            }

            interactable.OnInteract();
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
            // Room transition with fade effect
            if (sceneTransitionManager != null)
            {
                MonoBehaviour interactableMono = interactable as MonoBehaviour;
                if (interactableMono != null)
                {
                    var doorInteraction = interactableMono.GetComponent<DoorInteraction>();
                    if (doorInteraction != null && !string.IsNullOrEmpty(doorInteraction.targetSceneName))
                    {
                        sceneTransitionManager.TransitionToScene(doorInteraction.targetSceneName);
                    }
                    else
                    {
                        Debug.LogWarning($"Door {interactableMono.gameObject.name} has no target scene set");
                    }
                }
            }
            else
            {
                Debug.Log($"Opening door: {interactable.InteractionPrompt}");
            }

            interactable.OnInteract();
        }
        
        public void SetInteractionEnabled(bool enabled)
        {
            enableInteraction = enabled;
        }
    }
}