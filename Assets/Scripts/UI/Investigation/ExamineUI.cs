using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace NotAllNeighbours.UI
{
    /// <summary>
    /// UI system for displaying examination text when player examines objects
    /// </summary>
    public class ExamineUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject examinePanel;
        [SerializeField] private TextMeshProUGUI examineText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Display Settings")]
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private bool autoHide = true;

        private Coroutine displayCoroutine;
        private static ExamineUI instance;

        public static ExamineUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<ExamineUI>();
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

            if (examinePanel != null)
            {
                examinePanel.SetActive(false);
            }

            if (canvasGroup == null && examinePanel != null)
            {
                canvasGroup = examinePanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = examinePanel.AddComponent<CanvasGroup>();
                }
            }
        }

        /// <summary>
        /// Display examination text on screen
        /// </summary>
        public void ShowExamineText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("ExamineUI: Attempted to show empty text");
                return;
            }

            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }

            displayCoroutine = StartCoroutine(DisplayTextCoroutine(text));
        }

        /// <summary>
        /// Hide the examine text immediately
        /// </summary>
        public void HideExamineText()
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }

            if (examinePanel != null)
            {
                examinePanel.SetActive(false);
            }
        }

        private IEnumerator DisplayTextCoroutine(string text)
        {
            // Set text
            if (examineText != null)
            {
                examineText.text = text;
            }

            // Show panel
            if (examinePanel != null)
            {
                examinePanel.SetActive(true);
            }

            // Fade in
            if (canvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeSpeed));
            }

            // Wait for display duration
            if (autoHide)
            {
                yield return new WaitForSeconds(displayDuration);

                // Fade out
                if (canvasGroup != null)
                {
                    yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeSpeed));
                }

                // Hide panel
                if (examinePanel != null)
                {
                    examinePanel.SetActive(false);
                }
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float speed)
        {
            float elapsedTime = 0f;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * speed;
                cg.alpha = Mathf.Lerp(start, end, elapsedTime);
                yield return null;
            }

            cg.alpha = end;
        }
    }
}
