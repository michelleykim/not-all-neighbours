using UnityEngine;
using System.Collections.Generic;
using System;
using NotAllNeighbours.Data.Evidence;
using NotAllNeighbours.UI.Evidence;

namespace NotAllNeighbours.Evidence
{
  /// <summary>
  /// Manages the player's journal of evidence photographs
  /// Maximum 5 photos per day, invalid evidence auto-removed after validation
  /// </summary>
  public class JournalSystem : MonoBehaviour
  {
    [Header("UI Components")]
    [SerializeField] private JournalUI journalUI;

    [Header("Settings")]
    [SerializeField] private int maxPhotosPerDay = 5;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int totalEvidencePieces = 30; // Per killer, from GDD

    [Header("Current Session")]
    [SerializeField] private List<JournalEntry> todaysPhotos = new List<JournalEntry>();
    [SerializeField] private List<JournalEntry> allValidEvidence = new List<JournalEntry>();

    // Events
    public event Action<JournalEntry> OnPhotoAdded;
    public event Action<JournalEntry> OnPhotoRemoved;
    public event Action<int> OnDayAdvanced;
    public event Action OnPhotosValidated;

    private static JournalSystem instance;

    public static JournalSystem Instance
    {
      get
      {
        if (instance == null)
        {
          instance = FindFirstObjectByType<JournalSystem>();
        }
        return instance;
      }
    }

    public int CurrentDay => currentDay;
    public int PhotosToday => todaysPhotos.Count;
    public int MaxPhotosPerDay => maxPhotosPerDay;
    public bool CanTakePhoto => todaysPhotos.Count < maxPhotosPerDay;

    private void Awake()
    {
      if (instance != null && instance != this)
      {
        Destroy(gameObject);
        return;
      }

      instance = this;

      // Auto-find JournalUI if not assigned
      if (journalUI == null)
      {
        journalUI = FindFirstObjectByType<JournalUI>();
        if (journalUI == null)
        {
          Debug.LogWarning("JournalSystem: JournalUI not found! Please assign it in the inspector.");
        }
      }
    }

    /// <summary>
    /// Add a photograph to today's journal entries
    /// </summary>
    public bool AddPhoto(string objectName, string description, Sprite photo, bool isValidEvidence)
    {
      if (!CanTakePhoto)
      {
        Debug.LogWarning($"JournalSystem: Cannot take more photos today (max {maxPhotosPerDay})");
        return false;
      }

      string entryID = $"photo_{currentDay}_{todaysPhotos.Count}_{Guid.NewGuid().ToString().Substring(0, 8)}";
      JournalEntry entry = new JournalEntry(entryID, objectName, description, photo, currentDay);
      entry.isValidEvidence = isValidEvidence;

      todaysPhotos.Add(entry);
      OnPhotoAdded?.Invoke(entry);

      Debug.Log($"JournalSystem: Added photo of {objectName} ({todaysPhotos.Count}/{maxPhotosPerDay})");
      return true;
    }

    /// <summary>
    /// Remove a specific photo from the journal
    /// </summary>
    public void RemovePhoto(string entryID)
    {
      JournalEntry entry = todaysPhotos.Find(e => e.entryID == entryID);
      if (entry != null)
      {
        todaysPhotos.Remove(entry);
        OnPhotoRemoved?.Invoke(entry);
        Debug.Log($"JournalSystem: Removed photo {entryID}");
      }
    }

    /// <summary>
    /// Advance to the next day and validate evidence
    /// Invalid photos are automatically removed
    /// </summary>
    public void AdvanceDay()
    {
      currentDay++;

      // Validate and process yesterday's photos
      ValidateEvidence();

      // Clear today's photos for new day
      todaysPhotos.Clear();

      OnDayAdvanced?.Invoke(currentDay);
      Debug.Log($"JournalSystem: Advanced to day {currentDay}");
    }

    /// <summary>
    /// Validate evidence - remove invalid photos, keep valid ones
    /// Called automatically when advancing to next day
    /// </summary>
    private void ValidateEvidence()
    {
      List<JournalEntry> photosToRemove = new List<JournalEntry>();

      foreach (var entry in todaysPhotos)
      {
        entry.hasBeenValidated = true;

        if (entry.isValidEvidence)
        {
          // Keep valid evidence
          allValidEvidence.Add(entry);
          Debug.Log($"JournalSystem: Validated evidence - {entry.objectName}");
        }
        else
        {
          // Mark invalid photos for removal
          photosToRemove.Add(entry);
          Debug.Log($"JournalSystem: Removed invalid photo - {entry.objectName}");
        }
      }

      // Remove invalid photos
      foreach (var invalidPhoto in photosToRemove)
      {
        todaysPhotos.Remove(invalidPhoto);
        OnPhotoRemoved?.Invoke(invalidPhoto);
      }

      OnPhotosValidated?.Invoke();
    }

    /// <summary>
    /// Get all valid evidence collected across all days
    /// </summary>
    public List<JournalEntry> GetAllValidEvidence()
    {
      return new List<JournalEntry>(allValidEvidence);
    }

    /// <summary>
    /// Get today's photos (unvalidated)
    /// </summary>
    public List<JournalEntry> GetTodaysPhotos()
    {
      return new List<JournalEntry>(todaysPhotos);
    }

    /// <summary>
    /// Get the number of photos taken today
    /// </summary>
    public int GetPhotoCountToday()
    {
      return todaysPhotos.Count;
    }

    /// <summary>
    /// Check if a specific object has already been photographed today
    /// </summary>
    public bool HasPhotographedToday(string objectName)
    {
      return todaysPhotos.Exists(e => e.objectName == objectName);
    }

    /// <summary>
    /// Open journal UI to view evidence
    /// </summary>
    public void OpenJournal()
    {
      if (journalUI != null)
      {
        journalUI.OpenJournal(GetAllValidEvidence(), totalEvidencePieces);
      }
      else
      {
        Debug.LogWarning("JournalSystem: Cannot open journal - JournalUI is null");
      }
    }

    /// <summary>
    /// Close journal UI
    /// </summary>
    public void CloseJournal()
    {
      if (journalUI != null)
      {
        journalUI.CloseJournal();
      }
    }

    /// <summary>
    /// Clear all journal data (for testing/debugging)
    /// </summary>
    public void ClearJournal()
    {
      todaysPhotos.Clear();
      allValidEvidence.Clear();
      currentDay = 1;
      Debug.Log("JournalSystem: Journal cleared");
    }
  }
}
