using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using NotAllNeighbours.Data.Dialogue;
using NotAllNeighbours.UI.Dialogue;
using NotAllNeighbours.UI.Stats;

namespace NotAllNeighbours.Dialogue
{
  /// <summary>
  /// Manages NPC dialogue interactions with Sanity/Clarity mechanics
  /// </summary>
  public class DialogueManager : MonoBehaviour
  {
    [Header("UI Components")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private StatsDisplayUI statsDisplayUI;

    [Header("Stats")]
    [SerializeField] private float currentSanity = 100f;
    [SerializeField] private float currentClarity = 100f;

    // Events
    public event Action<float> OnSanityChanged;
    public event Action<float> OnClarityChanged;
    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;

    private DialogueNode currentDialogue;
    private bool isDialogueActive = false;

    private static DialogueManager instance;

    public static DialogueManager Instance
    {
      get
      {
        if (instance == null)
        {
          instance = FindObjectOfType<DialogueManager>();
        }
        return instance;
      }
    }

    public bool IsDialogueActive => isDialogueActive;
    public float Sanity => currentSanity;
    public float Clarity => currentClarity;

    private void Awake()
    {
      if (instance != null && instance != this)
      {
        Destroy(gameObject);
        return;
      }

      instance = this;

      // Auto-find UI components if not assigned
      if (dialogueUI == null)
      {
        dialogueUI = FindObjectOfType<DialogueUI>();
        if (dialogueUI == null)
        {
          Debug.LogWarning("DialogueManager: DialogueUI not found! Please assign it in the inspector.");
        }
      }

      if (statsDisplayUI == null)
      {
        statsDisplayUI = FindObjectOfType<StatsDisplayUI>();
        if (statsDisplayUI == null)
        {
          Debug.LogWarning("DialogueManager: StatsDisplayUI not found! Please assign it in the inspector.");
        }
      }

      // Initialize stats display
      UpdateStatsUI();
    }

    /// <summary>
    /// Start a dialogue with an NPC
    /// </summary>
    public void StartDialogue(DialogueNode dialogueNode)
    {
      if (dialogueNode == null)
      {
        Debug.LogError("DialogueManager: Attempted to start dialogue with null node");
        return;
      }

      currentDialogue = dialogueNode;
      isDialogueActive = true;

      if (dialogueUI != null)
      {
        dialogueUI.ShowDialoguePanel();
      }

      DisplayDialogue(dialogueNode);
      OnDialogueStarted?.Invoke();
    }

    /// <summary>
    /// Display a dialogue node
    /// </summary>
    private void DisplayDialogue(DialogueNode node)
    {
      if (dialogueUI == null)
      {
        Debug.LogError("DialogueManager: Cannot display dialogue - DialogueUI is null");
        return;
      }

      // Set speaker name and dialogue text
      dialogueUI.SetSpeakerName(node.speakerName);
      dialogueUI.SetDialogueText(node.dialogueText);

      // Display options with current stats for requirement checking
      dialogueUI.DisplayOptions(
          node.options,
          OnOptionSelected,
          currentSanity,
          currentClarity
      );
    }

    /// <summary>
    /// Handle option selection
    /// </summary>
    private void OnOptionSelected(DialogueOption option)
    {
      // Apply stat changes
      ModifySanity(option.sanityChange);
      ModifyClarity(option.clarityChange);

      // Show NPC response
      if (!string.IsNullOrEmpty(option.responseText) && dialogueUI != null)
      {
        dialogueUI.SetDialogueText(option.responseText);
      }

      // Handle gaslighting effects
      if (option.isGaslighting)
      {
        HandleGaslighting();
      }

      // Continue to next dialogue or end
      if (!string.IsNullOrEmpty(option.nextDialogueID))
      {
        // TODO: Load next dialogue node from database
        Debug.Log($"DialogueManager: Should load dialogue {option.nextDialogueID}");
      }
      else
      {
        EndDialogue();
      }
    }

    /// <summary>
    /// Handle gaslighting mechanics
    /// </summary>
    private void HandleGaslighting()
    {
      // Apply additional sanity/clarity penalties for gaslighting
      ModifySanity(-5f);
      Debug.Log("DialogueManager: Gaslighting detected - sanity reduced");
    }

    /// <summary>
    /// End the current dialogue
    /// </summary>
    public void EndDialogue()
    {
      isDialogueActive = false;
      currentDialogue = null;

      if (dialogueUI != null)
      {
        dialogueUI.ClearOptions();
        dialogueUI.HideDialoguePanel();
      }

      OnDialogueEnded?.Invoke();
    }

    /// <summary>
    /// Modify player's sanity
    /// </summary>
    public void ModifySanity(float amount)
    {
      currentSanity = Mathf.Clamp(currentSanity + amount, 0f, 100f);
      OnSanityChanged?.Invoke(currentSanity);
      UpdateStatsUI();
      Debug.Log($"DialogueManager: Sanity changed by {amount:F1} (now {currentSanity:F1})");
    }

    /// <summary>
    /// Modify player's clarity
    /// </summary>
    public void ModifyClarity(float amount)
    {
      currentClarity = Mathf.Clamp(currentClarity + amount, 0f, 100f);
      OnClarityChanged?.Invoke(currentClarity);
      UpdateStatsUI();
      Debug.Log($"DialogueManager: Clarity changed by {amount:F1} (now {currentClarity:F1})");
    }

    /// <summary>
    /// Update stats UI display (delegated to StatsDisplayUI)
    /// </summary>
    private void UpdateStatsUI()
    {
      if (statsDisplayUI != null)
      {
        statsDisplayUI.UpdateSanity(currentSanity);
        statsDisplayUI.UpdateClarity(currentClarity);
      }
    }

    /// <summary>
    /// Set sanity value directly
    /// </summary>
    public void SetSanity(float value)
    {
      currentSanity = Mathf.Clamp(value, 0f, 100f);
      OnSanityChanged?.Invoke(currentSanity);
      UpdateStatsUI();
    }

    /// <summary>
    /// Set clarity value directly
    /// </summary>
    public void SetClarity(float value)
    {
      currentClarity = Mathf.Clamp(value, 0f, 100f);
      OnClarityChanged?.Invoke(currentClarity);
      UpdateStatsUI();
    }
  }
}
