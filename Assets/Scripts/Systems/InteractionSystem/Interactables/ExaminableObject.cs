using UnityEngine;
using NotAllNeighbours.Interaction;

namespace NotAllNeighbours.System.InteractionSystem.Interactables
{
  /// <summary>
  /// Interactable object that provides examination text when clicked
  /// Left-click to examine and see description
  /// Can also be used for documents with title, text, and image
  /// </summary>
  public class ExaminableObject : InteractableObject
  {
    [Header("Examination Settings")]
    [SerializeField] private string examinationText = "An interesting object.";
    [SerializeField] private bool changeTextAfterFirstExamine = false;
    [SerializeField] private string subsequentExaminationText = "";

    [Header("Document Data (Optional)")]
    [SerializeField] private string documentTitle;
    [SerializeField][TextArea(5, 15)] private string documentText;
    [SerializeField] private Sprite documentImage;
    [SerializeField] private bool addToJournal = false;

    private bool hasBeenExamined = false;

    /// <summary>
    /// Override InteractionPrompt to return examination text
    /// </summary>
    public new string InteractionPrompt
    {
      get
      {
        if (hasBeenExamined && changeTextAfterFirstExamine && !string.IsNullOrEmpty(subsequentExaminationText))
        {
          return subsequentExaminationText;
        }
        return examinationText;
      }
    }

    public override void OnInteract()
    {
      if (!CanInteract()) return;

      hasBeenExamined = true;

      // Handle document-specific behaviour
      if (IsDocument())
      {
        Debug.Log($"Reading document: {documentTitle}");
        // TODO: Open document reading UI
        // TODO: Add to journal if enabled
      }

      base.OnInteract();
    }

    /// <summary>
    /// Set the examination text at runtime
    /// </summary>
    public void SetExaminationText(string text)
    {
      examinationText = text;
    }

    /// <summary>
    /// Check if this object has been examined
    /// </summary>
    public bool HasBeenExamined()
    {
      return hasBeenExamined;
    }

    /// <summary>
    /// Get document title (if this is a document)
    /// </summary>
    public string GetDocumentTitle()
    {
      return documentTitle;
    }

    /// <summary>
    /// Get document text (if this is a document)
    /// </summary>
    public string GetDocumentText()
    {
      return documentText;
    }

    /// <summary>
    /// Get document image (if this is a document)
    /// </summary>
    public Sprite GetDocumentImage()
    {
      return documentImage;
    }

    /// <summary>
    /// Check if this document should be added to the journal
    /// </summary>
    public bool ShouldAddToJournal()
    {
      return addToJournal;
    }

    /// <summary>
    /// Check if this examinable object is a document (has document data)
    /// </summary>
    public bool IsDocument()
    {
      return !string.IsNullOrEmpty(documentTitle) || !string.IsNullOrEmpty(documentText);
    }
  }
}
