using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System;
using NotAllNeighbours.Data.Dialogue;

namespace NotAllNeighbours.UI.Dialogue
{
  /// <summary>
  /// Handles all dialogue UI presentation and visual feedback
  /// Separated from DialogueManager business logic
  /// </summary>
  public class DialogueUI : MonoBehaviour
  {
    [Header("UI Panel References")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Options Container")]
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private List<GameObject> currentOptionButtons = new List<GameObject>();

    private void Awake()
    {
      // Get or add CanvasGroup for fade animations
      if (dialoguePanel != null)
      {
        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
          canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
        }
      }
    }

    /// <summary>
    /// Show the dialogue panel with fade-in animation
    /// </summary>
    public void ShowDialoguePanel()
    {
      if (dialoguePanel != null)
      {
        dialoguePanel.SetActive(true);

        if (canvasGroup != null)
        {
          StopAllCoroutines();
          StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInDuration));
        }
      }
    }

    /// <summary>
    /// Hide the dialogue panel with fade-out animation
    /// </summary>
    public void HideDialoguePanel()
    {
      if (canvasGroup != null)
      {
        StopAllCoroutines();
        StartCoroutine(FadeAndHidePanel());
      }
      else if (dialoguePanel != null)
      {
        dialoguePanel.SetActive(false);
      }
    }

    /// <summary>
    /// Update speaker name display
    /// </summary>
    public void SetSpeakerName(string speakerName)
    {
      if (speakerNameText != null)
      {
        speakerNameText.text = speakerName;
      }
    }

    /// <summary>
    /// Update dialogue text display
    /// </summary>
    public void SetDialogueText(string text)
    {
      if (dialogueText != null)
      {
        dialogueText.text = text;
      }
    }

    /// <summary>
    /// Create dialogue option buttons
    /// </summary>
    /// <param name="options">List of dialogue options to display</param>
    /// <param name="onOptionSelected">Callback when option is clicked</param>
    /// <param name="currentSanity">Current sanity value for requirement checking</param>
    /// <param name="currentClarity">Current clarity value for requirement checking</param>
    public void DisplayOptions(
        List<DialogueOption> options,
        System.Action<DialogueOption> onOptionSelected,
        float currentSanity,
        float currentClarity)
    {
      ClearOptions();

      if (optionButtonPrefab == null || optionsContainer == null)
      {
        Debug.LogWarning("DialogueUI: Option button prefab or container not assigned");
        return;
      }

      foreach (var option in options)
      {
        bool isAvailable = option.MeetsRequirements(currentSanity, currentClarity);
        CreateOptionButton(option, isAvailable, onOptionSelected, currentSanity, currentClarity);
      }
    }

    /// <summary>
    /// Create individual option button with appropriate styling
    /// </summary>
    private void CreateOptionButton(
        DialogueOption option,
        bool isAvailable,
        System.Action<DialogueOption> onOptionSelected,
        float currentSanity,
        float currentClarity)
    {
      GameObject buttonObj = Instantiate(optionButtonPrefab, optionsContainer);
      currentOptionButtons.Add(buttonObj);

      Button button = buttonObj.GetComponent<Button>();
      TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

      if (buttonText != null)
      {
        buttonText.text = option.optionText;

        // Visual feedback for locked options
        if (!isAvailable)
        {
          // Show requirement tooltip
          string lockReason = GetLockReason(option, currentSanity, currentClarity);
          buttonText.text += $" <color=#888888>[{lockReason}]</color>";
          buttonText.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }
        else
        {
          buttonText.color = Color.white;
        }
      }

      if (button != null)
      {
        button.interactable = isAvailable;

        if (isAvailable)
        {
          button.onClick.AddListener(() => onOptionSelected?.Invoke(option));
        }
      }

      // Add hover tooltip component for locked options
      if (!isAvailable)
      {
        AddTooltip(buttonObj, option, currentSanity, currentClarity);
      }
    }

    /// <summary>
    /// Determine why an option is locked
    /// </summary>
    private string GetLockReason(DialogueOption option, float currentSanity, float currentClarity)
    {
      if (currentSanity < option.minSanityRequired)
      {
        return $"Requires Sanity: {option.minSanityRequired:F0}+";
      }

      if (currentClarity < option.minClarityRequired)
      {
        return $"Requires Clarity: {option.minClarityRequired:F0}+";
      }

      return "Locked";
    }

    /// <summary>
    /// Add tooltip to locked option button
    /// </summary>
    private void AddTooltip(GameObject buttonObj, DialogueOption option, float currentSanity, float currentClarity)
    {
      var tooltip = buttonObj.AddComponent<DialogueOptionTooltip>();
      tooltip.Initialize(option, currentSanity, currentClarity);
    }

    /// <summary>
    /// Clear all dialogue option buttons
    /// </summary>
    public void ClearOptions()
    {
      foreach (var button in currentOptionButtons)
      {
        if (button != null)
        {
          Destroy(button);
        }
      }
      currentOptionButtons.Clear();
    }

    /// <summary>
    /// Check if dialogue panel is currently visible
    /// </summary>
    public bool IsVisible()
    {
      return dialoguePanel != null && dialoguePanel.activeSelf;
    }

    /// <summary>
    /// Fade canvas group over time
    /// </summary>
    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
      float elapsedTime = 0f;
      cg.alpha = startAlpha;

      while (elapsedTime < duration)
      {
        elapsedTime += Time.deltaTime;
        cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
        yield return null;
      }

      cg.alpha = endAlpha;
    }

    /// <summary>
    /// Fade out and then hide panel
    /// </summary>
    private System.Collections.IEnumerator FadeAndHidePanel()
    {
      yield return FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration);

      if (dialoguePanel != null)
      {
        dialoguePanel.SetActive(false);
      }
    }
  }

  /// <summary>
  /// Tooltip component for locked dialogue options
  /// Shows detailed requirement information on hover
  /// </summary>
  public class DialogueOptionTooltip : MonoBehaviour
  {
    private DialogueOption option;
    private float currentSanity;
    private float currentClarity;
    private GameObject tooltipPanel;

    public void Initialize(DialogueOption opt, float sanity, float clarity)
    {
      option = opt;
      currentSanity = sanity;
      currentClarity = clarity;
    }

    // Implement tooltip display logic here
    // This would show a detailed panel on hover explaining requirements
  }
}