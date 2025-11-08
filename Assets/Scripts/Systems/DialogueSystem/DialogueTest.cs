using UnityEngine;
using NotAllNeighbours.Dialogue;
using NotAllNeighbours.Data.Dialogue;

public class DialogueTest : MonoBehaviour
{
  public void TestDialogue()
  {
    DialogueNode testNode = new DialogueNode();
    testNode.dialogueID = "test_01";
    testNode.speakerName = "Test NPC";
    testNode.dialogueText = "This is a test dialogue. How do you respond?";

    DialogueOption option1 = new DialogueOption();
    option1.optionText = "I respond positively";
    option1.responseText = "That's great to hear!";
    option1.sanityChange = 5f;
    option1.clarityChange = 0f;

    DialogueOption option2 = new DialogueOption();
    option2.optionText = "I respond negatively";
    option2.responseText = "Oh... I see.";
    option2.sanityChange = -5f;
    option2.clarityChange = 5f;

    testNode.options.Add(option1);
    testNode.options.Add(option2);

    DialogueManager.Instance.StartDialogue(testNode);
  }
}