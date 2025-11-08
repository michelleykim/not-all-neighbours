using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using NotAllNeighbours.Data.Evidence;

namespace NotAllNeighbours.UI.Evidence
{
  /// <summary>
  /// Photo album-style journal interface for viewing collected evidence
  /// According to GDD: Players flip through photos like an album
  /// </summary>
  public class JournalUI : MonoBehaviour
  {
    [Header("Journal Panel")]
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Photo Display")]
    [SerializeField] private Image photoDisplay;
    [SerializeField] private TextMeshProUGUI photoTitleText;
    [SerializeField] private TextMeshProUGUI photoDescriptionText;
    [SerializeField] private TextMeshProUGUI photoDateText;
    [SerializeField] private TextMeshProUGUI photoCounterText; // "Photo 3/15"

    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    [Header("Evidence Counter")]
    [SerializeField] private TextMeshProUGUI evidenceCounterText; // "Evidence: 12/30"
    [SerializeField] private TextMeshProUGUI winConditionText; // "Need 15+ for good ending"

    [Header("Day Filter")]
    [SerializeField] private TMP_Dropdown dayFilterDropdown;
    [SerializeField] private Toggle showAllDaysToggle;

    [Header("Animation")]
    [SerializeField] private float pageFlipDuration = 0.3f;
    [SerializeField] private AudioClip pageFlipSound;

    private AudioSource audioSource;
    private List<JournalEntry> currentPhotos = new List<JournalEntry>();
    private int currentPhotoIndex = 0;
    private int filterDay = -1; // -1 = show all days

    private void Awake()
    {
      // Get or add components
      if (canvasGroup == null && journalPanel != null)
      {
        canvasGroup = journalPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
          canvasGroup = journalPanel.AddComponent<CanvasGroup>();
        }
      }

      audioSource = GetComponent<AudioSource>();
      if (audioSource == null)
      {
        audioSource = gameObject.AddComponent<AudioSource>();
      }

      // Set up button listeners
      if (previousButton != null)
        previousButton.onClick.AddListener(ShowPreviousPhoto);

      if (nextButton != null)
        nextButton.onClick.AddListener(ShowNextPhoto);

      if (closeButton != null)
        closeButton.onClick.AddListener(CloseJournal);

      if (showAllDaysToggle != null)
        showAllDaysToggle.onValueChanged.AddListener(OnShowAllDaysToggled);

      if (dayFilterDropdown != null)
        dayFilterDropdown.onValueChanged.AddListener(OnDayFilterChanged);

      // Hide journal by default
      if (journalPanel != null)
      {
        journalPanel.SetActive(false);
      }
    }

    /// <summary>
    /// Open journal with list of evidence photos
    /// </summary>
    public void OpenJournal(List<JournalEntry> photos, int totalEvidencePieces = 30)
    {
      currentPhotos = new List<JournalEntry>(photos);
      currentPhotoIndex = 0;
      filterDay = -1; // Show all by default

      if (journalPanel != null)
      {
        journalPanel.SetActive(true);
      }

      UpdateEvidenceCounter(photos.Count, totalEvidencePieces);
      PopulateDayFilter();
      DisplayCurrentPhoto();

      // Fade in animation
      if (canvasGroup != null)
      {
        StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, 0.3f));
      }
    }

    /// <summary>
    /// Close the journal
    /// </summary>
    public void CloseJournal()
    {
      if (canvasGroup != null)
      {
        StartCoroutine(FadeAndClose());
      }
      else if (journalPanel != null)
      {
        journalPanel.SetActive(false);
      }
    }

    /// <summary>
    /// Show previous photo in album
    /// </summary>
    public void ShowPreviousPhoto()
    {
      if (currentPhotos.Count == 0) return;

      currentPhotoIndex--;
      if (currentPhotoIndex < 0)
      {
        currentPhotoIndex = currentPhotos.Count - 1;
      }

      PlayPageFlipSound();
      DisplayCurrentPhoto();
    }

    /// <summary>
    /// Show next photo in album
    /// </summary>
    public void ShowNextPhoto()
    {
      if (currentPhotos.Count == 0) return;

      currentPhotoIndex++;
      if (currentPhotoIndex >= currentPhotos.Count)
      {
        currentPhotoIndex = 0;
      }

      PlayPageFlipSound();
      DisplayCurrentPhoto();
    }

    /// <summary>
    /// Display the currently selected photo
    /// </summary>
    private void DisplayCurrentPhoto()
    {
      if (currentPhotos.Count == 0)
      {
        DisplayEmptyJournal();
        return;
      }

      JournalEntry entry = currentPhotos[currentPhotoIndex];

      // Display photo
      if (photoDisplay != null)
      {
        photoDisplay.sprite = entry.photo;
        photoDisplay.enabled = entry.photo != null;
      }

      // Display info
      if (photoTitleText != null)
      {
        photoTitleText.text = entry.objectName;
      }

      if (photoDescriptionText != null)
      {
        photoDescriptionText.text = entry.description;
      }

      if (photoDateText != null)
      {
        photoDateText.text = $"Day {entry.dayTaken}";
      }

      if (photoCounterText != null)
      {
        photoCounterText.text = $"Photo {currentPhotoIndex + 1}/{currentPhotos.Count}";
      }

      // Update navigation buttons
      UpdateNavigationButtons();
    }

    /// <summary>
    /// Display empty journal message
    /// </summary>
    private void DisplayEmptyJournal()
    {
      if (photoDisplay != null)
      {
        photoDisplay.enabled = false;
      }

      if (photoTitleText != null)
      {
        photoTitleText.text = "No Evidence Collected";
      }

      if (photoDescriptionText != null)
      {
        photoDescriptionText.text = "Start photographing suspicious objects to collect evidence.";
      }

      if (photoDateText != null)
      {
        photoDateText.text = "";
      }

      if (photoCounterText != null)
      {
        photoCounterText.text = "0 Photos";
      }

      // Disable navigation
      if (previousButton != null)
        previousButton.interactable = false;

      if (nextButton != null)
        nextButton.interactable = false;
    }

    /// <summary>
    /// Update evidence counter display (X/30)
    /// </summary>
    private void UpdateEvidenceCounter(int collected, int total)
    {
      if (evidenceCounterText != null)
      {
        evidenceCounterText.text = $"Evidence: {collected}/{total}";

        // Color code based on progress
        if (collected >= 15)
        {
          evidenceCounterText.color = Color.green; // Good ending possible
        }
        else if (collected >= 10)
        {
          evidenceCounterText.color = Color.yellow; // Getting close
        }
        else
        {
          evidenceCounterText.color = Color.white;
        }
      }

      if (winConditionText != null)
      {
        if (collected >= 15)
        {
          winConditionText.text = "✓ Enough evidence for good ending!";
          winConditionText.color = Color.green;
        }
        else
        {
          int needed = 15 - collected;
          winConditionText.text = $"Need {needed} more for good ending";
          winConditionText.color = Color.yellow;
        }
      }
    }

    /// <summary>
    /// Update navigation button states
    /// </summary>
    private void UpdateNavigationButtons()
    {
      bool hasPhotos = currentPhotos.Count > 0;

      if (previousButton != null)
        previousButton.interactable = hasPhotos;

      if (nextButton != null)
        nextButton.interactable = hasPhotos;
    }

    /// <summary>
    /// Populate day filter dropdown with available days
    /// </summary>
    private void PopulateDayFilter()
    {
      if (dayFilterDropdown == null) return;

      dayFilterDropdown.ClearOptions();

      List<string> dayOptions = new List<string> { "All Days" };
      HashSet<int> daysWithPhotos = new HashSet<int>();

      foreach (var photo in currentPhotos)
      {
        daysWithPhotos.Add(photo.dayTaken);
      }

      List<int> sortedDays = new List<int>(daysWithPhotos);
      sortedDays.Sort();

      foreach (int day in sortedDays)
      {
        dayOptions.Add($"Day {day}");
      }

      dayFilterDropdown.AddOptions(dayOptions);
    }

    /// <summary>
    /// Handle day filter dropdown change
    /// </summary>
    private void OnDayFilterChanged(int index)
    {
      if (index == 0)
      {
        filterDay = -1; // All days
      }
      else
      {
        // Extract day number from dropdown option
        // This is simplified - you'd parse the actual day value
        filterDay = index; // Approximate
      }

      // Refilter photos (implementation would need full photo list)
      DisplayCurrentPhoto();
    }

    /// <summary>
    /// Handle show all days toggle
    /// </summary>
    private void OnShowAllDaysToggled(bool showAll)
    {
      if (dayFilterDropdown != null)
      {
        dayFilterDropdown.interactable = !showAll;
      }

      if (showAll)
      {
        filterDay = -1;
        DisplayCurrentPhoto();
      }
    }

    /// <summary>
    /// Play page flip sound effect
    /// </summary>
    private void PlayPageFlipSound()
    {
      if (audioSource != null && pageFlipSound != null)
      {
        audioSource.PlayOneShot(pageFlipSound);
      }
    }

    /// <summary>
    /// Fade canvas group over time
    /// </summary>
    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
      float elapsedTime = 0f;
      cg.alpha = startAlpha;

      while (elapsedTime < duration)
      {
        elapsedTime += Time.deltaTime;
        cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
        yield return null;
      }

      cg.alpha = endAlpha;
    }

    /// <summary>
    /// Fade out and close journal
    /// </summary>
    private System.Collections.IEnumerator FadeAndClose()
    {
      yield return FadeCanvasGroup(canvasGroup, 1f, 0f, 0.3f);

      if (journalPanel != null)
      {
        journalPanel.SetActive(false);
      }
    }

    /// <summary>
    /// Check if journal is currently open
    /// </summary>
    public bool IsOpen()
    {
      return journalPanel != null && journalPanel.activeSelf;
    }
  }
}