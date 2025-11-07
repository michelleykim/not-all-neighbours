using UnityEngine;

namespace NotAllNeighbours.Interaction
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        InteractionType GetInteractionType();
        bool CanInteract();
        void OnHoverEnter();
        void OnHoverExit();
        void OnInteract();
    }

    public enum InteractionType
    {
        Examine,        // Basic inspection
        Collect,        // Pick up evidence
        Use,           // Interact with object
        Talk,          // Dialogue with NPC
        Investigate,   // Detailed inspection (zoom)
        Document,      // Photography/recording
        Door           // Room transition
    }
}