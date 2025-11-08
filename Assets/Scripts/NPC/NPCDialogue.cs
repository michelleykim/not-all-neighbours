using UnityEngine;
using NotAllNeighbours.Dialogue;

namespace NotAllNeighbours.NPC
{
    /// <summary>
    /// Component for NPCs that provides dialogue data
    /// Attach to InteractableObject with Talk interaction type
    /// </summary>
    public class NPCDialogue : MonoBehaviour
    {
        [Header("NPC Info")]
        [SerializeField] private string npcName = "Neighbor";

        [Header("Dialogue")]
        [SerializeField] private DialogueNode initialDialogue;
        [SerializeField] private bool useScriptableObject = false;
        [SerializeField] private DialogueDataSO dialogueDataSO;

        [Header("Runtime")]
        private DialogueNode currentDialogueNode;

        private void Awake()
        {
            if (useScriptableObject && dialogueDataSO != null)
            {
                currentDialogueNode = dialogueDataSO.GetInitialDialogue();
            }
            else
            {
                currentDialogueNode = initialDialogue;
            }
        }

        /// <summary>
        /// Get the current dialogue node for this NPC
        /// </summary>
        public DialogueNode GetDialogueNode()
        {
            return currentDialogueNode;
        }

        /// <summary>
        /// Set a new dialogue node (for branching conversations)
        /// </summary>
        public void SetDialogueNode(DialogueNode newNode)
        {
            currentDialogueNode = newNode;
        }

        /// <summary>
        /// Get the NPC's name
        /// </summary>
        public string GetNPCName()
        {
            return npcName;
        }
    }

    /// <summary>
    /// ScriptableObject for storing dialogue data (optional)
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueData", menuName = "NotAllNeighbours/Dialogue Data")]
    public class DialogueDataSO : ScriptableObject
    {
        [SerializeField] private string npcName;
        [SerializeField] private DialogueNode initialDialogue;
        [SerializeField] private DialogueNode[] allDialogues;

        public DialogueNode GetInitialDialogue()
        {
            return initialDialogue;
        }

        public DialogueNode GetDialogueByID(string dialogueID)
        {
            foreach (var dialogue in allDialogues)
            {
                if (dialogue.dialogueID == dialogueID)
                {
                    return dialogue;
                }
            }

            Debug.LogWarning($"DialogueDataSO: Could not find dialogue with ID {dialogueID}");
            return null;
        }
    }
}
