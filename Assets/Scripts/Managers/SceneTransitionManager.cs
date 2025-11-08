using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace NotAllNeighbours.Managers
{
    /// <summary>
    /// Manages scene transitions with smooth fade effects for door interactions
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("Loading Settings")]
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private float minLoadingTime = 0.5f;

        private bool isTransitioning = false;
        private static SceneTransitionManager instance;

        public static SceneTransitionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SceneTransitionManager>();
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
            DontDestroyOnLoad(gameObject);

            // Initialize fade image
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            }

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Transition to a new scene with fade effect
        /// </summary>
        public void TransitionToScene(string sceneName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("SceneTransitionManager: Already transitioning to a scene");
                return;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneTransitionManager: Scene name is null or empty");
                return;
            }

            StartCoroutine(TransitionCoroutine(sceneName));
        }

        /// <summary>
        /// Transition to a scene by build index
        /// </summary>
        public void TransitionToScene(int sceneIndex)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("SceneTransitionManager: Already transitioning to a scene");
                return;
            }

            if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"SceneTransitionManager: Invalid scene index {sceneIndex}");
                return;
            }

            StartCoroutine(TransitionCoroutine(sceneIndex));
        }

        private IEnumerator TransitionCoroutine(string sceneName)
        {
            isTransitioning = true;

            // Fade out
            yield return StartCoroutine(FadeOut());

            // Show loading indicator
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(true);
            }

            // Start loading scene
            float startTime = Time.realtimeSinceStartup;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Wait for scene to load
            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            // Ensure minimum loading time for smooth transition
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (elapsedTime < minLoadingTime)
            {
                yield return new WaitForSeconds(minLoadingTime - elapsedTime);
            }

            // Activate the scene
            asyncLoad.allowSceneActivation = true;

            // Wait for scene activation
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Hide loading indicator
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }

            // Fade in
            yield return StartCoroutine(FadeIn());

            isTransitioning = false;
        }

        private IEnumerator TransitionCoroutine(int sceneIndex)
        {
            isTransitioning = true;

            // Fade out
            yield return StartCoroutine(FadeOut());

            // Show loading indicator
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(true);
            }

            // Start loading scene
            float startTime = Time.realtimeSinceStartup;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            asyncLoad.allowSceneActivation = false;

            // Wait for scene to load
            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            // Ensure minimum loading time for smooth transition
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (elapsedTime < minLoadingTime)
            {
                yield return new WaitForSeconds(minLoadingTime - elapsedTime);
            }

            // Activate the scene
            asyncLoad.allowSceneActivation = true;

            // Wait for scene activation
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Hide loading indicator
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }

            // Fade in
            yield return StartCoroutine(FadeIn());

            isTransitioning = false;
        }

        private IEnumerator FadeOut()
        {
            if (fadeImage == null) yield break;

            float elapsedTime = 0f;
            Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                fadeImage.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            fadeImage.color = endColor;
        }

        private IEnumerator FadeIn()
        {
            if (fadeImage == null) yield break;

            float elapsedTime = 0f;
            Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                fadeImage.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            fadeImage.color = endColor;
        }

        public bool IsTransitioning()
        {
            return isTransitioning;
        }
    }
}
