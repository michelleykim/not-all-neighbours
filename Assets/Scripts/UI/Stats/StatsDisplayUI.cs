using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NotAllNeighbours.UI.Stats
{
  /// <summary>
  /// Displays player's Sanity and Clarity stats in the HUD
  /// Separated from DialogueManager/Stats logic
  /// </summary>
  public class StatsDisplayUI : MonoBehaviour
  {
    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI sanityText;
    [SerializeField] private TextMeshProUGUI clarityText;

    [Header("Bar Display (Optional)")]
    [SerializeField] private Image sanityBar;
    [SerializeField] private Image clarityBar;

    [Header("Color Coding")]
    [SerializeField] private Color highColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private float mediumThreshold = 50f;
    [SerializeField] private float lowThreshold = 30f;

    [Header("Warning Effects")]
    [SerializeField] private GameObject sanityWarning;
    [SerializeField] private GameObject clarityWarning;
    [SerializeField] private float warningThreshold = 30f;

    [Header("Animation")]
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float animationSpeed = 2f;

    private float displayedSanity = 100f;
    private float displayedClarity = 100f;
    private float targetSanity = 100f;
    private float targetClarity = 100f;

    private void Start()
    {
      // Initialize warnings as hidden
      if (sanityWarning != null)
        sanityWarning.SetActive(false);

      if (clarityWarning != null)
        clarityWarning.SetActive(false);
    }

    private void Update()
    {
      if (animateChanges)
      {
        AnimateStatChanges();
      }
    }

    /// <summary>
    /// Update sanity display
    /// </summary>
    public void UpdateSanity(float value)
    {
      targetSanity = Mathf.Clamp(value, 0f, 100f);

      if (!animateChanges)
      {
        displayedSanity = targetSanity;
        ApplySanityDisplay();
      }
    }

    /// <summary>
    /// Update clarity display
    /// </summary>
    public void UpdateClarity(float value)
    {
      targetClarity = Mathf.Clamp(value, 0f, 100f);

      if (!animateChanges)
      {
        displayedClarity = targetClarity;
        ApplyClarityDisplay();
      }
    }

    /// <summary>
    /// Animate stat changes smoothly
    /// </summary>
    private void AnimateStatChanges()
    {
      bool changed = false;

      // Animate sanity
      if (!Mathf.Approximately(displayedSanity, targetSanity))
      {
        displayedSanity = Mathf.Lerp(displayedSanity, targetSanity, Time.deltaTime * animationSpeed);

        if (Mathf.Abs(displayedSanity - targetSanity) < 0.1f)
        {
          displayedSanity = targetSanity;
        }

        changed = true;
      }

      // Animate clarity
      if (!Mathf.Approximately(displayedClarity, targetClarity))
      {
        displayedClarity = Mathf.Lerp(displayedClarity, targetClarity, Time.deltaTime * animationSpeed);

        if (Mathf.Abs(displayedClarity - targetClarity) < 0.1f)
        {
          displayedClarity = targetClarity;
        }

        changed = true;
      }

      if (changed)
      {
        ApplySanityDisplay();
        ApplyClarityDisplay();
      }
    }

    /// <summary>
    /// Apply sanity value to UI elements
    /// </summary>
    private void ApplySanityDisplay()
    {
      // Update text
      if (sanityText != null)
      {
        sanityText.text = $"Sanity: {displayedSanity:F0}%";
        sanityText.color = GetColorForValue(displayedSanity);
      }

      // Update bar
      if (sanityBar != null)
      {
        sanityBar.fillAmount = displayedSanity / 100f;
        sanityBar.color = GetColorForValue(displayedSanity);
      }

      // Update warning
      if (sanityWarning != null)
      {
        sanityWarning.SetActive(displayedSanity <= warningThreshold);
      }
    }

    /// <summary>
    /// Apply clarity value to UI elements
    /// </summary>
    private void ApplyClarityDisplay()
    {
      // Update text
      if (clarityText != null)
      {
        clarityText.text = $"Clarity: {displayedClarity:F0}%";
        clarityText.color = GetColorForValue(displayedClarity);
      }

      // Update bar
      if (clarityBar != null)
      {
        clarityBar.fillAmount = displayedClarity / 100f;
        clarityBar.color = GetColorForValue(displayedClarity);
      }

      // Update warning
      if (clarityWarning != null)
      {
        clarityWarning.SetActive(displayedClarity <= warningThreshold);
      }
    }

    /// <summary>
    /// Get color based on stat value (high/medium/low)
    /// </summary>
    private Color GetColorForValue(float value)
    {
      if (value >= mediumThreshold)
      {
        // Lerp between medium and high
        float t = (value - mediumThreshold) / (100f - mediumThreshold);
        return Color.Lerp(mediumColor, highColor, t);
      }
      else if (value >= lowThreshold)
      {
        // Lerp between low and medium
        float t = (value - lowThreshold) / (mediumThreshold - lowThreshold);
        return Color.Lerp(lowColor, mediumColor, t);
      }
      else
      {
        return lowColor;
      }
    }

    /// <summary>
    /// Force immediate update without animation
    /// </summary>
    public void ForceUpdateImmediate(float sanity, float clarity)
    {
      displayedSanity = targetSanity = Mathf.Clamp(sanity, 0f, 100f);
      displayedClarity = targetClarity = Mathf.Clamp(clarity, 0f, 100f);

      ApplySanityDisplay();
      ApplyClarityDisplay();
    }
  }
}