using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NotAllNeighbours.Interaction;
using NotAllNeighbours.Core.Enums;

namespace NotAllNeighbours.UI.Investigation
{
  /// <summary>
  /// Displays interaction prompts when hovering over interactable objects
  /// Shows interaction type icon and prompt text
  /// According to GDD: 5 interaction types (Examine, Investigate, Door, Collect, Talk)
  /// </summary>
  public class InteractionPromptUI : MonoBehaviour
  {
    [Header("Display Elements")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image interactionIcon;

    [Header("Interaction Type Icons")]
    [SerializeField] private Sprite examineIcon;      // Examine (magnifying glass)
    [SerializeField] private Sprite investigateIcon;  // Investigate (detailed magnifier)
    [SerializeField] private Sprite doorIcon;         // Door (arrow/door)
    [SerializeField] private Sprite collectIcon;      // Collect (camera)
    [SerializeField] private Sprite talkIcon;         // Talk (speech bubble)

    [Header("Input Hints")]
    [SerializeField] private TextMeshProUGUI inputHintText;
    [SerializeField] private bool showInputHints = true;

    [Header("Positioning")]
    [SerializeField] private bool followMouse = true;
    [SerializeField] private Vector2 offset = new Vector2(15f, 15f);

    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
      rectTransform = GetComponent<RectTransform>();
      canvas = GetComponentInParent<Canvas>();

      if (promptPanel != null)
      {
        promptPanel.SetActive(false);
      }
    }

    private void Update()
    {
      if (followMouse && promptPanel != null && promptPanel.activeSelf)
      {
        UpdatePosition();
      }
    }

    /// <summary>
    /// Show interaction prompt for an interactable object
    /// </summary>
    public void ShowPrompt(IInteractable interactable)
    {
      if (interactable == null || !interactable.CanInteract())
      {
        HidePrompt();
        return;
      }

      if (promptPanel != null)
      {
        promptPanel.SetActive(true);
      }

      // Set prompt text
      if (promptText != null)
      {
        promptText.text = interactable.InteractionPrompt;
      }

      // Set interaction icon
      InteractionType type = interactable.GetInteractionType();
      SetInteractionIcon(type);

      // Set input hint
      if (inputHintText != null && showInputHints)
      {
        inputHintText.text = GetInputHint(type);
      }

      UpdatePosition();
    }

    /// <summary>
    /// Hide interaction prompt
    /// </summary>
    public void HidePrompt()
    {
      if (promptPanel != null)
      {
        promptPanel.SetActive(false);
      }
    }

    /// <summary>
    /// Set interaction icon based on type
    /// </summary>
    private void SetInteractionIcon(InteractionType type)
    {
      if (interactionIcon == null) return;

      Sprite icon = type switch
      {
        InteractionType.Examine => examineIcon,
        InteractionType.Investigate => investigateIcon,
        InteractionType.Door => doorIcon,
        InteractionType.Collect => collectIcon,
        InteractionType.Talk => talkIcon,
        _ => examineIcon
      };

      interactionIcon.sprite = icon;
      interactionIcon.enabled = icon != null;
    }

    /// <summary>
    /// Get input hint text based on interaction type
    /// According to GDD: Left-click for most, Right-click for Collect
    /// </summary>
    private string GetInputHint(InteractionType type)
    {
      return type switch
      {
        InteractionType.Examine => "[Left Click] Examine",
        InteractionType.Investigate => "[Left Click] Investigate",
        InteractionType.Door => "[Left Click] Open",
        InteractionType.Collect => "[Right Click] Photograph",
        InteractionType.Talk => "[Left Click] Talk",
        _ => "[Left Click] Interact"
      };
    }

    /// <summary>
    /// Update prompt position to follow mouse
    /// </summary>
    private void UpdatePosition()
    {
      if (canvas == null || rectTransform == null) return;

      Vector2 mousePosition = Input.mousePosition;
      Vector2 anchoredPosition = mousePosition / canvas.scaleFactor + offset;

      rectTransform.anchoredPosition = anchoredPosition;
    }

    /// <summary>
    /// Check if prompt is currently visible
    /// </summary>
    public bool IsVisible()
    {
      return promptPanel != null && promptPanel.activeSelf;
    }
  }
}