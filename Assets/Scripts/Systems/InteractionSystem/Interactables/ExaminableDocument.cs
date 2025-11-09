using UnityEngine;

namespace NotAllNeighbours.System.InteractionSystem.Interactables
{
  public class ExaminableDocument : InteractableObject
  {
    [Header("Document Data")]
    [SerializeField] private string documentTitle;
    [SerializeField][TextArea(5, 15)] private string documentText;
    [SerializeField] private Sprite documentImage;
    [SerializeField] private bool addToJournal = true;

    public override void OnInteract()
    {
      if (!CanInteract()) return;

      Debug.Log($"Reading document: {documentTitle}");

      // TODO: Open document reading UI
      // TODO: Add to journal if enabled

      base.OnInteract();
    }

    public string GetDocumentTitle()
    {
      return documentTitle;
    }

    public string GetDocumentText()
    {
      return documentText;
    }

    public Sprite GetDocumentImage()
    {
      return documentImage;
    }
  }
}