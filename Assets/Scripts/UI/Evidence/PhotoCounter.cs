using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NotAllNeighbours.UI.Evidence
{
  /// <summary>
  /// Displays photo counter in HUD (X/5 remaining today)
  /// According to GDD: 5 photos per day limit
  /// </summary>
  public class PhotoCounterUI : MonoBehaviour
  {
    [Header("Display Elements")]
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private Image counterIcon;
    [SerializeField] private Image[] photoSlots; // Visual slots for 5 photos

    [Header("Colors")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color usedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color warningColor = Color.red;

    [Header("Warning")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private float warningDisplayTime = 2f;

    private int currentPhotos = 0;
    private int maxPhotos = 5;

    /// <summary>
    /// Update photo counter display
    /// </summary>
    public void UpdatePhotoCount(int current, int max)
    {
      currentPhotos = current;
      maxPhotos = max;

      // Update text
      if (counterText != null)
      {
        counterText.text = $"Photos: {current}/{max}";

        // Warning color when no photos left
        if (current >= max)
        {
          counterText.color = warningColor;
        }
        else if (current >= max - 1)
        {
          counterText.color = Color.yellow;
        }
        else
        {
          counterText.color = availableColor;
        }
      }

      // Update visual slots
      UpdatePhotoSlots();
    }

    /// <summary>
    /// Update visual photo slot indicators
    /// </summary>
    private void UpdatePhotoSlots()
    {
      if (photoSlots == null || photoSlots.Length == 0) return;

      for (int i = 0; i < photoSlots.Length; i++)
      {
        if (photoSlots[i] != null)
        {
          // Mark slots as used/available
          photoSlots[i].color = (i < currentPhotos) ? usedColor : availableColor;
        }
      }
    }

    /// <summary>
    /// Show warning when max photos reached
    /// </summary>
    public void ShowMaxPhotosWarning()
    {
      if (warningPanel != null)
      {
        warningPanel.SetActive(true);
      }

      if (warningText != null)
      {
        warningText.text = "Maximum photos for today reached!\nWait until tomorrow for more.";
      }

      // Auto-hide after delay
      if (warningDisplayTime > 0)
      {
        Invoke(nameof(HideWarning), warningDisplayTime);
      }
    }

    /// <summary>
    /// Hide warning panel
    /// </summary>
    private void HideWarning()
    {
      if (warningPanel != null)
      {
        warningPanel.SetActive(false);
      }
    }

    /// <summary>
    /// Reset counter for new day
    /// </summary>
    public void ResetForNewDay()
    {
      UpdatePhotoCount(0, maxPhotos);
    }

    /// <summary>
    /// Check if photos are available
    /// </summary>
    public bool HasPhotosAvailable()
    {
      return currentPhotos < maxPhotos;
    }
  }
}