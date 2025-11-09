using UnityEngine;
using NotAllNeighbours.Core.Enums;

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
}