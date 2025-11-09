using UnityEngine;
using UnityEngine.InputSystem;
using NotAllNeighbours.UI;
using NotAllNeighbours.Managers;
using NotAllNeighbours.Evidence;
using NotAllNeighbours.Dialogue;
using NotAllNeighbours.NPC;
using NotAllNeighbours.Core.Enums;
using NotAllNeighbours.Systems.InteractionSystem.Interactables;

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
        raycastDetector = FindFirstObjectByType<RaycastDetector>();
      }

      if (investigationZoom == null)
      {
        investigationZoom = FindFirstObjectByType<InvestigationZoom>();
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
      // Right-click is now for photography on ALL interactable objects
      HandleCollect(interactable);
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
          // Get evidence data from the interactable object (now all InteractableObjects have photography support)
          string objectName = interactableMono.gameObject.name;
          string description = interactable.InteractionPrompt;
          bool isValidEvidence = false;

          // Get InteractableObject component (base class now has photography support)
          var interactableObj = interactableMono.GetComponent<InteractableObject>();
          if (interactableObj != null)
          {
            isValidEvidence = interactableObj.IsValidEvidence;
            description = interactableObj.GetEvidenceDescription();

            // Check if can be photographed when zoomed
            bool isZoomedIn = investigationZoom != null && investigationZoom.IsZoomed();
            if (!interactableObj.CanPhotograph(isZoomedIn))
            {
              Debug.Log("This evidence can only be photographed when zoomed in");
              return;
            }

            // Take the photo
            bool photoTaken = photographySystem.TakePhoto(objectName, description, isValidEvidence);

            // Mark as photographed if successful
            if (photoTaken)
            {
              interactableObj.MarkAsPhotographed();
            }
          }
          else
          {
            // Fallback for non-InteractableObject IInteractables
            photographySystem.TakePhoto(objectName, description, isValidEvidence);
          }
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