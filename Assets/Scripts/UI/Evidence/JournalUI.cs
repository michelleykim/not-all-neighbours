using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
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
    private bool awakeCompleted = false; // Track if Awake finished successfully
    private bool listenersAdded = false; // Track if button listeners were added

    // Public property for accessing page flip duration
    public float PageFlipDuration => pageFlipDuration;

    private void Awake()
    {
      Debug.Log("JournalUI: Awake called");

      // Get or add components
      if (canvasGroup == null && journalPanel != null)
      {
        canvasGroup = journalPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
          canvasGroup = journalPanel.AddComponent<CanvasGroup>();
          Debug.Log("JournalUI: Created new CanvasGroup component");
        }
      }

      // Ensure CanvasGroup doesn't block raycasts
      if (canvasGroup != null)
      {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        Debug.Log($"JournalUI: CanvasGroup configured - blocksRaycasts: {canvasGroup.blocksRaycasts}, interactable: {canvasGroup.interactable}");
      }

      audioSource = GetComponent<AudioSource>();
      if (audioSource == null)
      {
        audioSource = gameObject.AddComponent<AudioSource>();
      }

      // Set up button listeners
      Debug.Log($"JournalUI: Setting up button listeners - previousButton: {previousButton != null}, nextButton: {nextButton != null}, closeButton: {closeButton != null}");

      if (previousButton != null)
      {
        previousButton.onClick.AddListener(ShowPreviousPhoto);
        Debug.Log("JournalUI: Previous button listener added");
      }
      else
      {
        Debug.LogWarning("JournalUI: previousButton is null - cannot add listener");
      }

      if (nextButton != null)
      {
        nextButton.onClick.AddListener(ShowNextPhoto);
        Debug.Log("JournalUI: Next button listener added");
      }
      else
      {
        Debug.LogWarning("JournalUI: nextButton is null - cannot add listener");
      }

      if (closeButton != null)
      {
        closeButton.onClick.AddListener(CloseJournal);
        Debug.Log("JournalUI: Close button listener added");
        listenersAdded = true; // Mark that at least one listener was added
      }
      else
      {
        Debug.LogWarning("JournalUI: closeButton is null - cannot add listener");
      }

      if (showAllDaysToggle != null)
        showAllDaysToggle.onValueChanged.AddListener(OnShowAllDaysToggled);

      if (dayFilterDropdown != null)
        dayFilterDropdown.onValueChanged.AddListener(OnDayFilterChanged);

      // Hide journal by default
      if (journalPanel != null)
      {
        journalPanel.SetActive(false);
        Debug.Log("JournalUI: Journal panel hidden by default");
      }
      else
      {
        Debug.LogWarning("JournalUI: journalPanel is null!");
      }

      awakeCompleted = true;
      Debug.Log($"JournalUI: Awake completed - listenersAdded: {listenersAdded}");
    }

    /// <summary>
    /// Open journal with list of evidence photos
    /// </summary>
    public void OpenJournal(List<JournalEntry> photos, int totalEvidencePieces = 30)
    {
      currentPhotos = new List<JournalEntry>(photos);

      // Show most recent photo first (last in the list)
      currentPhotoIndex = currentPhotos.Count > 0 ? currentPhotos.Count - 1 : 0;
      filterDay = -1; // Show all by default

      if (journalPanel != null)
      {
        journalPanel.SetActive(true);
      }

      // Ensure CanvasGroup allows interaction when journal is open
      if (canvasGroup != null)
      {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        Debug.Log($"JournalUI: CanvasGroup set to interactable and blocksRaycasts");
      }

      UpdateEvidenceCounter(photos.Count, totalEvidencePieces);
      PopulateDayFilter();
      DisplayCurrentPhoto();

      Debug.Log($"JournalUI: Opened journal with {photos.Count} photos, showing photo {currentPhotoIndex + 1}");

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
      Debug.Log("JournalUI: CloseJournal button pressed");

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
      if (currentPhotos.Count == 0)
      {
        Debug.Log("JournalUI: ShowPreviousPhoto - no photos available");
        return;
      }

      int previousIndex = currentPhotoIndex;
      currentPhotoIndex--;
      if (currentPhotoIndex < 0)
      {
        currentPhotoIndex = currentPhotos.Count - 1;
      }

      Debug.Log($"JournalUI: ShowPreviousPhoto - moved from photo {previousIndex + 1} to {currentPhotoIndex + 1}");

      PlayPageFlipSound();
      DisplayCurrentPhoto();
    }

    /// <summary>
    /// Show next photo in album
    /// </summary>
    public void ShowNextPhoto()
    {
      if (currentPhotos.Count == 0)
      {
        Debug.Log("JournalUI: ShowNextPhoto - no photos available");
        return;
      }

      int previousIndex = currentPhotoIndex;
      currentPhotoIndex++;
      if (currentPhotoIndex >= currentPhotos.Count)
      {
        currentPhotoIndex = 0;
      }

      Debug.Log($"JournalUI: ShowNextPhoto - moved from photo {previousIndex + 1} to {currentPhotoIndex + 1}");

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

      // Log photo information (placeholder for actual photo display)
      Debug.Log($"JournalUI: Displaying photo {currentPhotoIndex + 1}/{currentPhotos.Count} - " +
                $"'{entry.objectName}' (Day {entry.dayTaken}) - {entry.description}");

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
    private global::System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
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
    private global::System.Collections.IEnumerator FadeAndClose()
    {
      yield return FadeCanvasGroup(canvasGroup, 1f, 0f, 0.3f);

      if (journalPanel != null)
      {
        journalPanel.SetActive(false);
        Debug.Log("JournalUI: Journal panel closed");
      }
    }

    /// <summary>
    /// Check if journal is currently open
    /// </summary>
    public bool IsOpen()
    {
      return journalPanel != null && journalPanel.activeSelf;
    }

    /// <summary>
    /// Verify button setup and configuration - call this to diagnose button issues
    /// </summary>
    [ContextMenu("Verify Button Setup")]
    public void VerifyButtonSetup()
    {
      Debug.Log("=== JournalUI Button Verification ===");

      // Check if Awake was called
      Debug.Log($"Awake Completed: {awakeCompleted}");
      Debug.Log($"Listeners Added: {listenersAdded}");

      if (!awakeCompleted)
      {
        Debug.LogError("  !! Awake() has not run yet! This component may not be initialized.");
      }

      if (!listenersAdded)
      {
        Debug.LogError("  !! No button listeners were added! Check if buttons are assigned in Inspector.");
      }

      // Check journal panel
      Debug.Log($"journalPanel: {(journalPanel != null ? "Assigned" : "NULL")}");
      if (journalPanel != null)
      {
        Debug.Log($"  - Active: {journalPanel.activeSelf}");
        Debug.Log($"  - ActiveInHierarchy: {journalPanel.activeInHierarchy}");
      }

      // Check canvas group
      Debug.Log($"canvasGroup: {(canvasGroup != null ? "Assigned" : "NULL")}");
      if (canvasGroup != null)
      {
        Debug.Log($"  - alpha: {canvasGroup.alpha}");
        Debug.Log($"  - interactable: {canvasGroup.interactable}");
        Debug.Log($"  - blocksRaycasts: {canvasGroup.blocksRaycasts}");
      }

      // Check buttons
      Debug.Log($"closeButton: {(closeButton != null ? "Assigned" : "NULL")}");
      if (closeButton != null)
      {
        Debug.Log($"  - GameObject: {closeButton.gameObject.name}");
        Debug.Log($"  - Active: {closeButton.gameObject.activeInHierarchy}");
        Debug.Log($"  - Interactable: {closeButton.interactable}");
        Debug.Log($"  - Persistent Listeners: {closeButton.onClick.GetPersistentEventCount()}");

        // Check if Button component exists and is enabled
        var buttonComponent = closeButton.GetComponent<Button>();
        Debug.Log($"  - Button Component: {(buttonComponent != null ? "Present" : "MISSING")}");
        if (buttonComponent != null)
        {
          Debug.Log($"  - Component Enabled: {buttonComponent.enabled}");
        }
      }

      Debug.Log($"previousButton: {(previousButton != null ? "Assigned" : "NULL")}");
      if (previousButton != null)
      {
        Debug.Log($"  - GameObject: {previousButton.gameObject.name}");
        Debug.Log($"  - Active: {previousButton.gameObject.activeInHierarchy}");
        Debug.Log($"  - Interactable: {previousButton.interactable}");
        Debug.Log($"  - Persistent Listeners: {previousButton.onClick.GetPersistentEventCount()}");
      }

      Debug.Log($"nextButton: {(nextButton != null ? "Assigned" : "NULL")}");
      if (nextButton != null)
      {
        Debug.Log($"  - GameObject: {nextButton.gameObject.name}");
        Debug.Log($"  - Active: {nextButton.gameObject.activeInHierarchy}");
        Debug.Log($"  - Interactable: {nextButton.interactable}");
        Debug.Log($"  - Persistent Listeners: {nextButton.onClick.GetPersistentEventCount()}");
      }

      // Check for EventSystem
      UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
      Debug.Log($"EventSystem: {(eventSystem != null ? "Found" : "MISSING!")}");
      if (eventSystem != null)
      {
        Debug.Log($"  - Enabled: {eventSystem.enabled}");
        Debug.Log($"  - GameObject: {eventSystem.gameObject.name}");
      }

      Debug.Log("=== NOTE: Listener count shows 0 because AddListener() adds runtime listeners,");
      Debug.Log("=== not persistent ones. Check console for 'listener added' messages from Awake.");
      Debug.Log("=================================");
    }

    /// <summary>
    /// Test method to manually trigger close button functionality
    /// </summary>
    [ContextMenu("Test Close Button")]
    public void TestCloseButton()
    {
      Debug.Log("JournalUI: TestCloseButton() called manually");
      CloseJournal();
    }

    /// <summary>
    /// Test method to manually trigger previous button functionality
    /// </summary>
    [ContextMenu("Test Previous Button")]
    public void TestPreviousButton()
    {
      Debug.Log("JournalUI: TestPreviousButton() called manually");
      ShowPreviousPhoto();
    }

    /// <summary>
    /// Test method to manually trigger next button functionality
    /// </summary>
    [ContextMenu("Test Next Button")]
    public void TestNextButton()
    {
      Debug.Log("JournalUI: TestNextButton() called manually");
      ShowNextPhoto();
    }
  }
}