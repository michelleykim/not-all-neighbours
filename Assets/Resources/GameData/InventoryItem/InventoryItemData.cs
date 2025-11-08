using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Item", menuName = "Not All Neighbours/Inventory Item")]
public class InventoryItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public GameObject itemPrefab;
    public bool isEvidence = true;
    public bool isQuestItem = false;
}