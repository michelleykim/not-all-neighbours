using UnityEngine;
using NotAllNeighbours.Evidence;
using NotAllNeighbours.Data.Evidence;
using System.Collections.Generic;

/// <summary>
/// Test script for JournalUI functionality
/// Attach to a GameObject and call TestJournal() to test the journal with sample photos
/// </summary>
public class JournalTest : MonoBehaviour
{
  /// <summary>
  /// Test the journal with sample photos
  /// Creates 3 test photos and opens the journal
  /// </summary>
  public void TestJournal()
  {
    Debug.Log("JournalTest: Starting journal test");

    if (JournalSystem.Instance == null)
    {
      Debug.LogError("JournalTest: JournalSystem.Instance is null!");
      return;
    }

    // Clear existing photos for clean test
    JournalSystem.Instance.ClearJournal();

    // Create test photos
    CreateTestPhotos();

    // Open the journal to view the test photos
    JournalSystem.Instance.OpenJournal();

    Debug.Log("JournalTest: Journal opened with test photos");
  }

  /// <summary>
  /// Create sample test photos
  /// </summary>
  private void CreateTestPhotos()
  {
    // Create 3 test photos with placeholder sprites
    // Note: These will have null sprites until we assign actual sprites

    // Photo 1 - Valid evidence
    JournalSystem.Instance.AddPhoto(
      objectName: "Bloody Knife",
      description: "A knife with suspicious red stains. Could be evidence of the crime.",
      photo: null, // Placeholder - sprite will be null
      isValidEvidence: true
    );
    Debug.Log("JournalTest: Added photo 1 - Bloody Knife");

    // Photo 2 - Valid evidence
    JournalSystem.Instance.AddPhoto(
      objectName: "Torn Letter",
      description: "A letter with threatening content, torn in half.",
      photo: null, // Placeholder - sprite will be null
      isValidEvidence: true
    );
    Debug.Log("JournalTest: Added photo 2 - Torn Letter");

    // Photo 3 - Red herring (invalid evidence)
    JournalSystem.Instance.AddPhoto(
      objectName: "Coffee Mug",
      description: "An ordinary coffee mug. Probably not important.",
      photo: null, // Placeholder - sprite will be null
      isValidEvidence: false
    );
    Debug.Log("JournalTest: Added photo 3 - Coffee Mug");

    // Advance day to validate photos (this will remove invalid evidence)
    JournalSystem.Instance.AdvanceDay();
    Debug.Log("JournalTest: Advanced day - invalid photos removed");

    // Add one more photo on the new day
    JournalSystem.Instance.AddPhoto(
      objectName: "Fingerprint",
      description: "A clear fingerprint on the window sill.",
      photo: null, // Placeholder - sprite will be null
      isValidEvidence: true
    );
    Debug.Log("JournalTest: Added photo 4 - Fingerprint (Day 2)");
  }

  /// <summary>
  /// Add a single test photo (for quick testing)
  /// </summary>
  public void AddSingleTestPhoto()
  {
    if (JournalSystem.Instance == null)
    {
      Debug.LogError("JournalTest: JournalSystem.Instance is null!");
      return;
    }

    int photoCount = JournalSystem.Instance.PhotosToday;
    JournalSystem.Instance.AddPhoto(
      objectName: $"Test Object {photoCount + 1}",
      description: $"This is test photo number {photoCount + 1}",
      photo: null,
      isValidEvidence: true
    );

    Debug.Log($"JournalTest: Added single test photo (total today: {photoCount + 1})");
  }

  /// <summary>
  /// Clear all journal data
  /// </summary>
  public void ClearJournal()
  {
    if (JournalSystem.Instance == null)
    {
      Debug.LogError("JournalTest: JournalSystem.Instance is null!");
      return;
    }

    JournalSystem.Instance.ClearJournal();
    Debug.Log("JournalTest: Journal cleared");
  }
}
