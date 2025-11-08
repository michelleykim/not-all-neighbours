using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace NotAllNeighbours.Evidence
{
    /// <summary>
    /// Handles photography mechanics for evidence collection
    /// Right-click to photograph objects, flash effect, photo count UI
    /// </summary>
    public class PhotographySystem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image flashImage;
        [SerializeField] private TextMeshProUGUI photoCountText;
        [SerializeField] private GameObject photoTakenIndicator;

        [Header("Flash Settings")]
        [SerializeField] private float flashDuration = 0.3f;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Audio")]
        [SerializeField] private AudioSource cameraAudioSource;
        [SerializeField] private AudioClip shutterSound;

        [Header("Feedback")]
        [SerializeField] private float indicatorDisplayTime = 2f;

        private JournalSystem journalSystem;
        private bool isFlashing = false;
        private static PhotographySystem instance;

        public static PhotographySystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PhotographySystem>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            // Initialize flash image
            if (flashImage != null)
            {
                flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            }

            if (photoTakenIndicator != null)
            {
                photoTakenIndicator.SetActive(false);
            }
        }

        private void Start()
        {
            journalSystem = JournalSystem.Instance;

            if (journalSystem != null)
            {
                journalSystem.OnPhotoAdded += OnPhotoAdded;
                UpdatePhotoCountUI();
            }
        }

        private void OnDestroy()
        {
            if (journalSystem != null)
            {
                journalSystem.OnPhotoAdded -= OnPhotoAdded;
            }
        }

        /// <summary>
        /// Take a photograph of an object for evidence
        /// </summary>
        public bool TakePhoto(string objectName, string description, bool isValidEvidence)
        {
            if (journalSystem == null)
            {
                Debug.LogError("PhotographySystem: JournalSystem not found!");
                return false;
            }

            if (!journalSystem.CanTakePhoto)
            {
                ShowMaxPhotosWarning();
                return false;
            }

            // Check if already photographed
            if (journalSystem.HasPhotographedToday(objectName))
            {
                Debug.Log($"PhotographySystem: Already photographed {objectName} today");
                return false;
            }

            // Create a screenshot/sprite for the photo (placeholder for now)
            Sprite photoSprite = CapturePhoto();

            // Add to journal
            bool success = journalSystem.AddPhoto(objectName, description, photoSprite, isValidEvidence);

            if (success)
            {
                // Play camera effects
                PlayCameraFlash();
                PlayShutterSound();
                UpdatePhotoCountUI();
            }

            return success;
        }

        /// <summary>
        /// Capture a photo sprite (placeholder implementation)
        /// In production, this would capture actual screen/camera view
        /// </summary>
        private Sprite CapturePhoto()
        {
            // TODO: Implement actual screenshot capture
            // For now, return null - real implementation would use RenderTexture
            return null;
        }

        /// <summary>
        /// Play camera flash effect
        /// </summary>
        private void PlayCameraFlash()
        {
            if (isFlashing) return;

            if (flashImage != null)
            {
                StartCoroutine(FlashCoroutine());
            }
        }

        private IEnumerator FlashCoroutine()
        {
            isFlashing = true;

            float elapsedTime = 0f;

            while (elapsedTime < flashDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / flashDuration;
                float alpha = flashCurve.Evaluate(t);

                if (flashImage != null)
                {
                    flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
                }

                yield return null;
            }

            if (flashImage != null)
            {
                flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            }

            isFlashing = false;
        }

        /// <summary>
        /// Play camera shutter sound
        /// </summary>
        private void PlayShutterSound()
        {
            if (cameraAudioSource != null && shutterSound != null)
            {
                cameraAudioSource.PlayOneShot(shutterSound);
            }
        }

        /// <summary>
        /// Update the photo count UI display
        /// </summary>
        private void UpdatePhotoCountUI()
        {
            if (photoCountText != null && journalSystem != null)
            {
                photoCountText.text = $"Photos: {journalSystem.PhotosToday}/{journalSystem.MaxPhotosPerDay}";
            }
        }

        /// <summary>
        /// Called when a photo is added to the journal
        /// </summary>
        private void OnPhotoAdded(JournalEntry entry)
        {
            UpdatePhotoCountUI();
            ShowPhotoTakenIndicator();
        }

        /// <summary>
        /// Show indicator that photo was taken
        /// </summary>
        private void ShowPhotoTakenIndicator()
        {
            if (photoTakenIndicator != null)
            {
                StartCoroutine(ShowIndicatorCoroutine());
            }
        }

        private IEnumerator ShowIndicatorCoroutine()
        {
            photoTakenIndicator.SetActive(true);
            yield return new WaitForSeconds(indicatorDisplayTime);
            photoTakenIndicator.SetActive(false);
        }

        /// <summary>
        /// Show warning when max photos reached
        /// </summary>
        private void ShowMaxPhotosWarning()
        {
            Debug.LogWarning("PhotographySystem: Maximum photos for today reached!");
            // TODO: Show UI warning message
        }

        /// <summary>
        /// Check if player can take more photos
        /// </summary>
        public bool CanTakePhoto()
        {
            return journalSystem != null && journalSystem.CanTakePhoto;
        }

        /// <summary>
        /// Get remaining photo count for today
        /// </summary>
        public int GetRemainingPhotos()
        {
            if (journalSystem == null) return 0;
            return journalSystem.MaxPhotosPerDay - journalSystem.PhotosToday;
        }
    }
}
