using UnityEngine;
using System.Collections.Generic;

namespace NotAllNeighbours.Camera
{
    /// <summary>
    /// Represents a single fixed camera position in a room.
    /// Used for first-person investigation with limited movement.
    /// </summary>
    public class CameraPosition : MonoBehaviour
    {
        [Header("Position Information")]
        [Tooltip("Descriptive name for this camera position (e.g., 'Doorway Overview')")]
        [SerializeField] private string positionName = "Camera Position";
        
        [Tooltip("Description shown to player when investigating")]
        [TextArea(2, 4)]
        [SerializeField] private string positionDescription = "A viewpoint for investigation";
        
        [Tooltip("Index in the room's camera sequence")]
        [SerializeField] private int positionIndex = 0;

        [Header("Camera Settings")]
        [Tooltip("Default rotation when player switches to this position")]
        [SerializeField] private Vector3 defaultRotationEuler = Vector3.zero;
        
        [Tooltip("Field of view for this camera position")]
        [Range(30f, 90f)]
        [SerializeField] private float fieldOfView = 60f;

        [Header("Rotation Limits")]
        [Tooltip("Minimum vertical angle (looking down)")]
        [Range(-90f, 0f)]
        [SerializeField] private float minVerticalAngle = -60f;
        
        [Tooltip("Maximum vertical angle (looking up)")]
        [Range(0f, 90f)]
        [SerializeField] private float maxVerticalAngle = 60f;
        
        [Tooltip("Allow full 360-degree horizontal rotation")]
        [SerializeField] private bool allowFullHorizontalRotation = true;

        [Header("Investigation Data")]
        [Tooltip("Is this a primary investigation focus point?")]
        [SerializeField] private bool isInvestigationFocus = false;
        
        [Tooltip("Objects visible from this position")]
        [SerializeField] private List<GameObject> visibleInteractables = new List<GameObject>();
        
        [Tooltip("Optional ambient sound for this viewpoint")]
        [SerializeField] private AudioClip ambientSound = null;

        [Header("Gizmo Settings")]
        [Tooltip("Color for gizmo visualization in editor")]
        [SerializeField] private Color gizmoColor = Color.cyan;
        
        [Tooltip("Show FOV cone in editor")]
        [SerializeField] private bool showFOVCone = true;

        // Cached properties
        private Quaternion _defaultRotation;

        #region Properties
        public string PositionName => positionName;
        public string PositionDescription => positionDescription;
        public int PositionIndex => positionIndex;
        public float FieldOfView => fieldOfView;
        public float MinVerticalAngle => minVerticalAngle;
        public float MaxVerticalAngle => maxVerticalAngle;
        public bool AllowFullHorizontalRotation => allowFullHorizontalRotation;
        public bool IsInvestigationFocus => isInvestigationFocus;
        public AudioClip AmbientSound => ambientSound;
        public Quaternion DefaultRotation => _defaultRotation;
        public Vector3 Position => transform.position;
        #endregion

        private void Awake()
        {
            // Cache the default rotation
            _defaultRotation = Quaternion.Euler(defaultRotationEuler);
        }

        /// <summary>
        /// Validates if a given rotation is within allowed limits
        /// </summary>
        public bool IsWithinRotationLimits(Quaternion rotation)
        {
            // Convert to euler angles
            Vector3 euler = rotation.eulerAngles;
            
            // Normalize x rotation to -180 to 180 range
            float xAngle = euler.x;
            if (xAngle > 180f) xAngle -= 360f;
            
            // Check vertical limits
            if (xAngle < minVerticalAngle || xAngle > maxVerticalAngle)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Clamps a rotation to valid limits for this position
        /// </summary>
        public Quaternion ClampRotation(Quaternion rotation)
        {
            Vector3 euler = rotation.eulerAngles;
            
            // Normalize and clamp vertical rotation
            float xAngle = euler.x;
            if (xAngle > 180f) xAngle -= 360f;
            xAngle = Mathf.Clamp(xAngle, minVerticalAngle, maxVerticalAngle);
            
            // Keep horizontal rotation as-is (full 360 degrees allowed)
            return Quaternion.Euler(xAngle, euler.y, 0f);
        }

        /// <summary>
        /// Gets list of visible interactable objects
        /// </summary>
        public List<GameObject> GetVisibleInteractables()
        {
            // Remove null references
            visibleInteractables.RemoveAll(obj => obj == null);
            return visibleInteractables;
        }

        /// <summary>
        /// Adds an interactable object to the visible list
        /// </summary>
        public void AddVisibleInteractable(GameObject obj)
        {
            if (obj != null && !visibleInteractables.Contains(obj))
            {
                visibleInteractables.Add(obj);
            }
        }

        #region Editor Visualization
        private void OnDrawGizmos()
        {
            // Draw position sphere
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, 0.2f);
            
            // Draw wire sphere for selection radius
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            
            // Draw position label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                $"{positionName}\n[{positionIndex}]"
            );
            #endif
        }

        private void OnDrawGizmosSelected()
        {
            if (!showFOVCone) return;

            // Draw FOV cone
            Gizmos.color = isInvestigationFocus ? Color.yellow : Color.cyan;
            
            // Draw forward direction
            Vector3 forward = Quaternion.Euler(defaultRotationEuler) * Vector3.forward;
            Gizmos.DrawRay(transform.position, forward * 3f);
            
            // Draw FOV cone lines
            float halfFOV = fieldOfView * 0.5f;
            Quaternion leftRot = Quaternion.Euler(0, -halfFOV, 0);
            Quaternion rightRot = Quaternion.Euler(0, halfFOV, 0);
            Quaternion upRot = Quaternion.Euler(-halfFOV, 0, 0);
            Quaternion downRot = Quaternion.Euler(halfFOV, 0, 0);
            
            Vector3 leftDir = leftRot * forward;
            Vector3 rightDir = rightRot * forward;
            Vector3 upDir = upRot * forward;
            Vector3 downDir = downRot * forward;
            
            Gizmos.DrawRay(transform.position, leftDir * 2f);
            Gizmos.DrawRay(transform.position, rightDir * 2f);
            Gizmos.DrawRay(transform.position, upDir * 2f);
            Gizmos.DrawRay(transform.position, downDir * 2f);
            
            // Draw vertical rotation limits
            Gizmos.color = Color.red;
            Vector3 minVerticalDir = Quaternion.Euler(minVerticalAngle, 0, 0) * forward;
            Vector3 maxVerticalDir = Quaternion.Euler(maxVerticalAngle, 0, 0) * forward;
            Gizmos.DrawRay(transform.position, minVerticalDir * 1.5f);
            Gizmos.DrawRay(transform.position, maxVerticalDir * 1.5f);
        }
        #endregion

        #region Validation
        private void OnValidate()
        {
            // Ensure min/max are correctly ordered
            if (minVerticalAngle > maxVerticalAngle)
            {
                float temp = minVerticalAngle;
                minVerticalAngle = maxVerticalAngle;
                maxVerticalAngle = temp;
            }
            
            // Update cached rotation
            _defaultRotation = Quaternion.Euler(defaultRotationEuler);
        }
        #endregion
    }
}