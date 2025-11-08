using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NotAllNeighbours.Interaction
{
    [System.Serializable]
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
    
    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxInventorySize = 50;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<InventoryItem> onItemAdded;
        [SerializeField] private UnityEvent<InventoryItem> onItemRemoved;
        [SerializeField] private UnityEvent onInventoryChanged;
        
        private List<InventoryItem> inventory = new List<InventoryItem>();
        private Dictionary<string, int> itemCounts = new Dictionary<string, int>();
        
        public bool AddItem(InventoryItem item)
        {
            if (inventory.Count >= maxInventorySize)
            {
                Debug.LogWarning("Inventory is full!");
                return false;
            }
            
            inventory.Add(item);
            
            // Track item count
            if (itemCounts.ContainsKey(item.itemID))
            {
                itemCounts[item.itemID]++;
            }
            else
            {
                itemCounts[item.itemID] = 1;
            }
            
            onItemAdded?.Invoke(item);
            onInventoryChanged?.Invoke();
            
            Debug.Log($"Added {item.itemName} to inventory. Total: {itemCounts[item.itemID]}");
            return true;
        }
        
        public bool RemoveItem(string itemID)
        {
            InventoryItem item = inventory.Find(i => i.itemID == itemID);
            
            if (item != null)
            {
                inventory.Remove(item);
                
                if (itemCounts.ContainsKey(itemID))
                {
                    itemCounts[itemID]--;
                    if (itemCounts[itemID] <= 0)
                    {
                        itemCounts.Remove(itemID);
                    }
                }
                
                onItemRemoved?.Invoke(item);
                onInventoryChanged?.Invoke();
                
                return true;
            }
            
            return false;
        }
        
        public bool HasItem(string itemID)
        {
            return itemCounts.ContainsKey(itemID) && itemCounts[itemID] > 0;
        }
        
        public int GetItemCount(string itemID)
        {
            return itemCounts.ContainsKey(itemID) ? itemCounts[itemID] : 0;
        }
        
        public List<InventoryItem> GetAllItems()
        {
            return new List<InventoryItem>(inventory);
        }
        
        public List<InventoryItem> GetEvidenceItems()
        {
            return inventory.FindAll(item => item.isEvidence);
        }
        
        public void ClearInventory()
        {
            inventory.Clear();
            itemCounts.Clear();
            onInventoryChanged?.Invoke();
        }
    }
}