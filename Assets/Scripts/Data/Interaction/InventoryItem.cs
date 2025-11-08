using UnityEngine;
using System;

namespace NotAllNeighbours.Data.Interaction
{
  /// <summary>
  /// Represents an item that can be stored in the inventory
  /// </summary>
  [Serializable]
  public class InventoryItem
  {
    public string itemID;
    public string itemName;
    public string description;
    public Sprite icon;
    public GameObject itemPrefab;
    public bool isEvidence;
    public bool isQuestItem;
  }
}