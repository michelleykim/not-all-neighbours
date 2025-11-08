using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace NotAllNeighbours.Dialogue
{
    /// <summary>
    /// Dialogue option with Sanity/Clarity requirements and effects
    /// </summary>
    [Serializable]
    public class DialogueOption
    {
        public string optionText;
        public string responseText;

        [Header("Requirements")]
        public float minSanityRequired = 0f;
        public float minClarityRequired = 0f;

        [Header("Effects")]
        public float sanityChange = 0f;
        public float clarityChange = 0f;

        [Header("Flags")]
        public bool isGaslighting = false;
        public bool revealsClue = false;
        public string nextDialogueID;

        public bool MeetsRequirements(float currentSanity, float currentClarity)
        {
            return currentSanity >= minSanityRequired && currentClarity >= minClarityRequired;
        }
    }

    /// <summary>
    /// Dialogue node containing conversation data
    /// </summary>
    [Serializable]
    public class DialogueNode
    {
        public string dialogueID;
        public string speakerName;
        public string dialogueText;
        public List<DialogueOption> options = new List<DialogueOption>();
        public bool endsConversation = false;
    }

    /// <summary>
    /// Manages NPC dialogue interactions with Sanity/Clarity mechanics
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("Stats")]
        [SerializeField] private float currentSanity = 100f;
        [SerializeField] private float currentClarity = 100f;

        [Header("UI Feedback")]
        [SerializeField] private TextMeshProUGUI sanityText;
        [SerializeField] private TextMeshProUGUI clarityText;

        // Events
        public event Action<float> OnSanityChanged;
        public event Action<float> OnClarityChanged;
        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;

        private DialogueNode currentDialogue;
        private bool isDialogueActive = false;
        private List<GameObject> currentOptionButtons = new List<GameObject>();

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

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

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

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            DisplayDialogue(dialogueNode);
            OnDialogueStarted?.Invoke();
        }

        /// <summary>
        /// Display a dialogue node
        /// </summary>
        private void DisplayDialogue(DialogueNode node)
        {
            // Set speaker name
            if (speakerNameText != null)
            {
                speakerNameText.text = node.speakerName;
            }

            // Set dialogue text
            if (dialogueText != null)
            {
                dialogueText.text = node.dialogueText;
            }

            // Clear previous options
            ClearOptions();

            // Create option buttons
            foreach (var option in node.options)
            {
                // Check if option is available based on Sanity/Clarity
                bool isAvailable = option.MeetsRequirements(currentSanity, currentClarity);
                CreateOptionButton(option, isAvailable);
            }
        }

        /// <summary>
        /// Create a dialogue option button
        /// </summary>
        private void CreateOptionButton(DialogueOption option, bool isAvailable)
        {
            if (optionButtonPrefab == null || optionsContainer == null)
            {
                Debug.LogWarning("DialogueManager: Option button prefab or container not assigned");
                return;
            }

            GameObject buttonObj = Instantiate(optionButtonPrefab, optionsContainer);
            currentOptionButtons.Add(buttonObj);

            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = option.optionText;

                // Visual feedback for unavailable options
                if (!isAvailable)
                {
                    buttonText.text += " [Locked]";
                    buttonText.color = Color.gray;
                }
            }

            if (button != null)
            {
                button.interactable = isAvailable;
                button.onClick.AddListener(() => OnOptionSelected(option));
            }
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
            if (!string.IsNullOrEmpty(option.responseText))
            {
                if (dialogueText != null)
                {
                    dialogueText.text = option.responseText;
                }
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
        /// Clear all dialogue options
        /// </summary>
        private void ClearOptions()
        {
            foreach (var button in currentOptionButtons)
            {
                Destroy(button);
            }
            currentOptionButtons.Clear();
        }

        /// <summary>
        /// End the current dialogue
        /// </summary>
        public void EndDialogue()
        {
            isDialogueActive = false;
            currentDialogue = null;
            ClearOptions();

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
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
        /// Update stats UI display
        /// </summary>
        private void UpdateStatsUI()
        {
            if (sanityText != null)
            {
                sanityText.text = $"Sanity: {currentSanity:F0}";
            }

            if (clarityText != null)
            {
                clarityText.text = $"Clarity: {currentClarity:F0}";
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
