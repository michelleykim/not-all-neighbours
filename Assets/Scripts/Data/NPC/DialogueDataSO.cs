using UnityEngine;
using NotAllNeighbours.Data.Dialogue;

namespace NotAllNeighbours.Data.NPC
{
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