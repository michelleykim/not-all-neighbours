using System;
using System.Collections.Generic;

namespace NotAllNeighbours.Data.Dialogue
{
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
}