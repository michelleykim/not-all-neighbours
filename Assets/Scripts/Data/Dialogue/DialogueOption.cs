using UnityEngine;
using System;

namespace NotAllNeighbours.Data.Dialogue
{
  /// <summary>
  /// Dialogue option with Sanity/Clarity requirements and effects
  /// </summary>
  [Serializable]
  public class DialogueOption
  {
    public string optionText;
    public string responseText;

    [Header("Requirements")]
    public float minSanityRequired = 0f;
    public float minClarityRequired = 0f;

    [Header("Effects")]
    public float sanityChange = 0f;
    public float clarityChange = 0f;

    [Header("Flags")]
    public bool isGaslighting = false;
    public bool revealsClue = false;
    public string nextDialogueID;

    public bool MeetsRequirements(float currentSanity, float currentClarity)
    {
      return currentSanity >= minSanityRequired && currentClarity >= minClarityRequired;
    }
  }
}