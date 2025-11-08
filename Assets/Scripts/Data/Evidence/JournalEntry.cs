using UnityEngine;
using System;

namespace NotAllNeighbours.Data.Evidence
{
  /// <summary>
  /// Data structure for a journal entry (photograph)
  /// </summary>
  [Serializable]
  public class JournalEntry
  {
    public string entryID;
    public string objectName;
    public string description;
    public Sprite photo;
    public int dayTaken;
    public bool isValidEvidence;
    public bool hasBeenValidated;

    public JournalEntry(string id, string name, string desc, Sprite photoSprite, int day)
    {
      entryID = id;
      objectName = name;
      description = desc;
      photo = photoSprite;
      dayTaken = day;
      isValidEvidence = false;
      hasBeenValidated = false;
    }
  }
}