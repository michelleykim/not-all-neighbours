using UnityEngine;

namespace NotAllNeighbours.Interaction
{
    /// <summary>
    /// Interactable object that can be photographed for evidence
    /// Right-click to photograph and add to journal
    /// </summary>
    public class CollectableObject : InteractableObject
    {
        [Header("Evidence Settings")]
        [SerializeField] private bool isValidEvidence = false;
        [SerializeField] private string evidenceDescription = "Potential evidence";
        [SerializeField] private bool canOnlyBePhotographedWhenZoomed = false;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject photographedIndicator;
        [SerializeField] private bool disableAfterPhotograph = false;

        private bool hasBeenPhotographed = false;

        public bool IsValidEvidence => isValidEvidence;
        public bool HasBeenPhotographed => hasBeenPhotographed;
        public bool CanOnlyBePhotographedWhenZoomed => canOnlyBePhotographedWhenZoomed;

        protected override void Awake()
        {
            base.Awake();

            if (photographedIndicator != null)
            {
                photographedIndicator.SetActive(false);
            }
        }

        public override void OnInteract()
        {
            // Mark as photographed
            MarkAsPhotographed();

            base.OnInteract();
        }

        /// <summary>
        /// Mark this object as photographed
        /// </summary>
        public void MarkAsPhotographed()
        {
            hasBeenPhotographed = true;

            // Show indicator
            if (photographedIndicator != null)
            {
                photographedIndicator.SetActive(true);
            }

            // Disable interaction if configured
            if (disableAfterPhotograph)
            {
                SetInteractable(false);
            }

            Debug.Log($"CollectableObject: {gameObject.name} has been photographed");
        }

        /// <summary>
        /// Get the evidence description for the journal
        /// </summary>
        public string GetEvidenceDescription()
        {
            return evidenceDescription;
        }

        /// <summary>
        /// Check if this object can be photographed right now
        /// </summary>
        public bool CanPhotograph(bool isZoomedIn)
        {
            if (canOnlyBePhotographedWhenZoomed && !isZoomedIn)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set whether this is valid evidence (for dynamic validation)
        /// </summary>
        public void SetValidEvidence(bool valid)
        {
            isValidEvidence = valid;
        }
    }
}
