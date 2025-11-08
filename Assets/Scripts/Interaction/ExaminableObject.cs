using UnityEngine;

namespace NotAllNeighbours.Interaction
{
    /// <summary>
    /// Interactable object that provides examination text when clicked
    /// Left-click to examine and see description
    /// </summary>
    public class ExaminableObject : InteractableObject
    {
        [Header("Examination Settings")]
        [SerializeField] private string examinationText = "An interesting object.";
        [SerializeField] private bool changeTextAfterFirstExamine = false;
        [SerializeField] private string subsequentExaminationText = "";

        [Header("Evidence Settings (if secondary type is Collect)")]
        [SerializeField] private bool isValidEvidence = false;

        private bool hasBeenExamined = false;

        public bool IsValidEvidence => isValidEvidence;

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
            hasBeenExamined = true;
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
    }
}
