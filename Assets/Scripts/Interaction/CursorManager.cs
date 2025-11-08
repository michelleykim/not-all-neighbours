using UnityEngine;

namespace NotAllNeighbours.Interaction
{
    public class CursorManager : MonoBehaviour
    {
        [Header("Cursor Textures")]
        [SerializeField] private Texture2D defaultCursor;
        [SerializeField] private Texture2D examineCursor;
        [SerializeField] private Texture2D collectCursor;
        [SerializeField] private Texture2D talkCursor;
        [SerializeField] private Texture2D doorCursor;
        [SerializeField] private Texture2D useCursor;
        
        [Header("Settings")]
        [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
        
        [Header("References")]
        [SerializeField] private RaycastDetector raycastDetector;

        private void Awake()
        {
            if (raycastDetector == null)
            {
                raycastDetector = FindObjectOfType<RaycastDetector>();
                if (raycastDetector != null)
                {
                    Debug.Log("CursorManager: Auto-found RaycastDetector");
                }
                else
                {
                    Debug.LogWarning("CursorManager: Could not find RaycastDetector!");
                }
            }
        }

        private void Update()
        {
            UpdateCursor();
        }
        
        private void UpdateCursor()
        {
            if (raycastDetector == null) return;
            
            IInteractable hoveredObject = raycastDetector.GetCurrentHoveredObject();
            
            if (hoveredObject != null && hoveredObject.CanInteract())
            {
                InteractionType type = hoveredObject.GetInteractionType();
                SetCursorForInteractionType(type);
            }
            else
            {
                SetDefaultCursor();
            }
        }
        
        private void SetCursorForInteractionType(InteractionType type)
        {
            Texture2D cursorTexture = type switch
            {
                InteractionType.Examine => examineCursor,
                InteractionType.Collect => collectCursor,
                InteractionType.Talk => talkCursor,
                InteractionType.Door => doorCursor,
                InteractionType.Use => useCursor,
                InteractionType.Investigate => examineCursor,
                InteractionType.Document => examineCursor,
                _ => defaultCursor
            };
            
            if (cursorTexture != null)
            {
                Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
            }
        }
        
        private void SetDefaultCursor()
        {
            if (defaultCursor != null)
            {
                Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }
}