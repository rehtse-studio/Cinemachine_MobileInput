//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DigitalRubyShared
{
    /// <summary>
    /// Allows two finger pan, scale and rotate on a game object
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Pan Rotate Scale", 4)]
    public class FingersPanRotateScaleComponentScript : MonoBehaviour
    {
        /// <summary>
        /// Double tap reset mode
        /// </summary>
        public enum _DoubleTapResetMode
        {
            /// <summary>
            /// No reset on double tap
            /// </summary>
            Off = 0,

            /// <summary>
            /// Reset scale and rotation on double tap
            /// </summary>
            ResetScaleRotation = 1,

            /// <summary>
            /// Reset scale, rotation and position on double tap
            /// </summary>
            ResetScaleRotationPosition = 2
        }

        /// <summary>The cameras to use to convert screen coordinates to world coordinates. Defaults to Camera.main.</summary>
        [Header("Setup")]
        [Tooltip("The cameras to use to convert screen coordinates to world coordinates. Defaults to Camera.main.")]
        public Camera[] Cameras;

        /// <summary>Whether to bring the object to the front when a gesture executes on it</summary>
        [Tooltip("Whether to bring the object to the front when a gesture executes on it")]
        public bool BringToFront = true;

        /// <summary>Whether the gestures in this script can execute simultaneously with all other gestures.</summary>
        [Tooltip("Whether the gestures in this script can execute simultaneously with all other gestures.")]
        public bool AllowExecutionWithAllGestures;

        /// <summary>The mode to execute in, can be require over game object or allow on any game object.</summary>
        [Tooltip("The mode to execute in, can be require over game object or allow on any game object.")]
        public GestureRecognizerComponentScriptBase.GestureObjectMode Mode = GestureRecognizerComponentScriptBase.GestureObjectMode.RequireIntersectWithGameObject;

        /// <summary>The minimum and maximum scale as a percentage of the original scale of the object. 0,0 for no limits. -1,-1 for no scaling.</summary>
        [Tooltip("The minimum and maximum scale as a percentage of the original scale of the object. 0,0 for no limits. -1,-1 for no scaling.")]
        public Vector2 MinMaxScale;

        /// <summary>The threshold in units touches must move apart or together to begin scaling.</summary>
        [Tooltip("The threshold in units touches must move apart or together to begin scaling.")]
        [Range(0.0f, 1.0f)]
        public float ScaleThresholdUnits = 0.15f;

        /// <summary>Whether to add a double tap to reset the transform of the game object this script is on. This must be set in the inspector and not changed.</summary>
        [Header("Enable / Disable Gestures")]
        [Tooltip("Whether to add a double tap to reset the transform of the game object this script is on. This must be set in the inspector and not changed.")]
        public _DoubleTapResetMode DoubleTapResetMode;

        /// <summary>Whether to allow panning. Can be set during editor or runtime.</summary>
        [Tooltip("Whether to allow panning. Can be set during editor or runtime.")]
        public bool AllowPan = true;

        /// <summary>Whether to allow scaling. Can be set during editor or runtime.</summary>
        [Tooltip("Whether to allow scaling. Can be set during editor or runtime.")]
        public bool AllowScale = true;

        /// <summary>Whether to allow rotating. Can be set during editor or runtime.</summary>
        [Tooltip("Whether to allow rotating. Can be set during editor or runtime.")]
        public bool AllowRotate = true;

        /// <summary>Gesture state updated event</summary>
        [Tooltip("Gesture state updated event")]
        public GestureRecognizerComponentStateUpdatedEvent StateUpdated;

        /// <summary>
        /// Allow moving the target
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// Allow scaling the target
        /// </summary>
        public ScaleGestureRecognizer ScaleGesture { get; private set; }

        /// <summary>
        /// Allow rotating the target
        /// </summary>
        public RotateGestureRecognizer RotateGesture { get; private set; }

        /// <summary>
        /// The double tap gesture or null if DoubleTapToReset was false when this script started up
        /// </summary>
        public TapGestureRecognizer DoubleTapGesture { get; private set; }

        private Rigidbody2D rigidBody2D;
        private Rigidbody rigidBody;
        private SpriteRenderer spriteRenderer;
        private CanvasRenderer canvasRenderer;
        private Transform _transform;
        private int startSortOrder;
        private float panZ;
        private Vector3 panOffset;
        private Vector3? startScale;

        private struct SavedState
        {
            public Vector3 Scale;
            public Quaternion Rotation;
            public Vector3 Position;
        }
        private readonly Dictionary<Transform, SavedState> savedStates = new Dictionary<Transform, SavedState>();

        private void PanGestureUpdated(DigitalRubyShared.GestureRecognizer panGesture)
        {
            if (!AllowPan)
            {
                panGesture.Reset();
                return;
            }

            Camera camera;
            GameObject obj = FingersScript.StartOrResetGesture(panGesture, BringToFront, Cameras, gameObject, spriteRenderer, Mode, out camera);
            if (camera == null)
            {
                panGesture.Reset();
                return;
            }

            StateUpdated.Invoke(PanGesture);
            if (panGesture.State == GestureRecognizerState.Began)
            {
                SetStartState(panGesture, obj, false);
            }
            else if (panGesture.State == GestureRecognizerState.Executing && _transform != null)
            {
                if (PanGesture.ReceivedAdditionalTouches)
                {
                    panZ = camera.WorldToScreenPoint(_transform.position).z;
                    if (canvasRenderer == null)
                    {
                        panOffset = _transform.position - camera.ScreenToWorldPoint(new Vector3(panGesture.FocusX, panGesture.FocusY, panZ));
                    }
                    else
                    {
                        Vector2 screenToCanvasPoint = canvasRenderer.GetComponentInParent<Canvas>().ScreenToCanvasPoint(new Vector2(panGesture.FocusX, panGesture.FocusY));
                        panOffset = new Vector3(screenToCanvasPoint.x - _transform.position.x, screenToCanvasPoint.y - _transform.position.y, 0.0f);
                    }
                }
                Vector3 gestureScreenPoint = new Vector3(panGesture.FocusX, panGesture.FocusY, panZ);
                Vector3 gestureWorldPoint = camera.ScreenToWorldPoint(gestureScreenPoint) + panOffset;
                if (rigidBody != null)
                {
                    rigidBody.MovePosition(gestureWorldPoint);
                }
                else if (rigidBody2D != null)
                {
                    rigidBody2D.MovePosition(gestureWorldPoint);
                }
                else if (canvasRenderer != null)
                {
                    _transform.position = gestureScreenPoint - panOffset;
                }
                else
                {
                    _transform.position = gestureWorldPoint;
                }
            }
            else if (panGesture.State == GestureRecognizerState.Ended)
            {
                if (spriteRenderer != null && BringToFront)
                {
                    spriteRenderer.sortingOrder = startSortOrder;
                }
                ClearStartState();
            }
        }

        private void ScaleGestureUpdated(DigitalRubyShared.GestureRecognizer scaleGesture)
        {
            if (!AllowScale)
            {
                scaleGesture.Reset();
                return;
            }
            else if (MinMaxScale.x < 0.0f || MinMaxScale.y < 0.0f || startScale == null)
            {
                // no scaling
                return;
            }

            Camera camera;
            GameObject obj = FingersScript.StartOrResetGesture(scaleGesture, BringToFront, Cameras, gameObject, spriteRenderer, Mode, out camera);
            if (camera == null)
            {
                scaleGesture.Reset();
                return;
            }

            StateUpdated.Invoke(PanGesture);
            if (scaleGesture.State == GestureRecognizerState.Began)
            {
                SetStartState(scaleGesture, obj, false);
            }
            else if (scaleGesture.State == GestureRecognizerState.Executing && _transform != null)
            {
                // assume uniform scale
                Vector3 scale = new Vector3
                (
                    (_transform.localScale.x * ScaleGesture.ScaleMultiplier),
                    (_transform.localScale.y * ScaleGesture.ScaleMultiplier),
                    (_transform.localScale.z * ScaleGesture.ScaleMultiplier)
                );
                if (MinMaxScale.x > 0.0f || MinMaxScale.y > 0.0f)
                {
                    float minValue = Mathf.Min(MinMaxScale.x, MinMaxScale.y);
                    float maxValue = Mathf.Max(MinMaxScale.x, MinMaxScale.y);
                    scale.x = Mathf.Clamp(scale.x, startScale.Value.x * minValue, startScale.Value.x * maxValue);
                    scale.y = Mathf.Clamp(scale.y, startScale.Value.y * minValue, startScale.Value.y * maxValue);
                    scale.z = Mathf.Clamp(scale.z, startScale.Value.z * minValue, startScale.Value.z * maxValue);
                }

                // don't mess with z scale for 2D
                scale.z = (rigidBody2D == null && spriteRenderer == null && canvasRenderer == null ? scale.z : _transform.localScale.z);
                _transform.localScale = scale;
            }
            else if (scaleGesture.State == GestureRecognizerState.Ended)
            {
                ClearStartState();
            }
        }

        private void RotateGestureUpdated(DigitalRubyShared.GestureRecognizer rotateGesture)
        {
            if (!AllowRotate)
            {
                rotateGesture.Reset();
                return;
            }

            Camera camera;
            GameObject obj = FingersScript.StartOrResetGesture(rotateGesture, BringToFront, Cameras, gameObject, spriteRenderer, Mode, out camera);
            if (camera == null)
            {
                rotateGesture.Reset();
                return;
            }

            StateUpdated.Invoke(rotateGesture);
            if (rotateGesture.State == GestureRecognizerState.Began)
            {
                SetStartState(rotateGesture, obj, false);
            }
            else if (rotateGesture.State == GestureRecognizerState.Executing && _transform != null)
            {
                if (rigidBody != null)
                {
                    float angle = RotateGesture.RotationDegreesDelta;
                    Quaternion rotation = Quaternion.AngleAxis(angle, camera.transform.forward);
                    rigidBody.MoveRotation(rigidBody.rotation * rotation);
                }
                else if (rigidBody2D != null)
                {
                    rigidBody2D.MoveRotation(rigidBody2D.rotation + RotateGesture.RotationDegreesDelta);
                }
                else if (canvasRenderer != null)
                {
                    _transform.Rotate(Vector3.forward, RotateGesture.RotationDegreesDelta, Space.Self);
                }
                else
                {
                    _transform.Rotate(camera.transform.forward, RotateGesture.RotationDegreesDelta, Space.Self);
                }
            }
            else if (rotateGesture.State == GestureRecognizerState.Ended)
            {
                ClearStartState();
            }
        }

        private void DoubleTapGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            if (DoubleTapResetMode == _DoubleTapResetMode.Off)
            {
                r.Reset();
                return;
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                Camera camera = FingersScript.GetCameraForGesture(r, Cameras);
                GameObject obj = FingersScript.GestureIntersectsObject(r, camera, gameObject, Mode);
                SavedState state;
                if (obj != null && savedStates.TryGetValue(obj.transform, out state))
                {
                    obj.transform.rotation = state.Rotation;
                    obj.transform.localScale = state.Scale;
                    if (DoubleTapResetMode == _DoubleTapResetMode.ResetScaleRotationPosition)
                    {
                        obj.transform.position = state.Position;
                    }
                    savedStates.Remove(obj.transform);
                }
            }
        }

        private void ClearStartState()
        {
            if (Mode != GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
            {
                return;
            }
            else if (PanGesture.State != GestureRecognizerState.Executing &&
                RotateGesture.State != GestureRecognizerState.Executing &&
                ScaleGesture.State != GestureRecognizerState.Executing)
            {
                rigidBody2D = null;
                rigidBody = null;
                spriteRenderer = null;
                canvasRenderer = null;
                _transform = null;
            }
        }

        private bool SetStartState(DigitalRubyShared.GestureRecognizer gesture, GameObject obj, bool force)
        {
            if (!force && Mode != GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
            {
                return false;
            }
            else if (obj == null)
            {
                ClearStartState();
                return false;
            }
            else if (_transform == null)
            {
                rigidBody2D = obj.GetComponent<Rigidbody2D>();
                rigidBody = obj.GetComponent<Rigidbody>();
                spriteRenderer = obj.GetComponent<SpriteRenderer>();
                canvasRenderer = obj.GetComponent<CanvasRenderer>();
                if (spriteRenderer != null)
                {
                    startSortOrder = spriteRenderer.sortingOrder;
                }
                _transform = (rigidBody == null ? (rigidBody2D == null ? obj.transform : rigidBody2D.transform) : rigidBody.transform);
                if (DoubleTapResetMode != _DoubleTapResetMode.Off && !savedStates.ContainsKey(_transform))
                {
                    savedStates[_transform] = new SavedState { Rotation = _transform.rotation, Scale = _transform.localScale, Position = _transform.position };
                }
                if (startScale == null)
                {
                    startScale = _transform.localScale;
                }
            }
            else if (_transform != obj.transform)
            {
                if (gesture != null)
                {
                    gesture.Reset();
                }
                return false;
            }
            return true;
        }

        private void OnEnable()
        {
            if ((Cameras == null || Cameras.Length == 0) && Camera.main != null)
            {
                Cameras = new Camera[] { Camera.main };
            }
            PanGesture = new PanGestureRecognizer { MaximumNumberOfTouchesToTrack = 2, ThresholdUnits = 0.01f };
            PanGesture.StateUpdated += PanGestureUpdated;
            ScaleGesture = new ScaleGestureRecognizer();
            ScaleGesture.ThresholdUnits = ScaleThresholdUnits;
            ScaleGesture.StateUpdated += ScaleGestureUpdated;
            RotateGesture = new RotateGestureRecognizer();
            RotateGesture.StateUpdated += RotateGestureUpdated;
            if (Mode != GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
            {
                SetStartState(null, gameObject, true);
            }
            if (DoubleTapResetMode != _DoubleTapResetMode.Off)
            {
                DoubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
                DoubleTapGesture.StateUpdated += DoubleTapGestureUpdated;
            }
            if (AllowExecutionWithAllGestures)
            {
                PanGesture.AllowSimultaneousExecutionWithAllGestures();
                RotateGesture.AllowSimultaneousExecutionWithAllGestures();
                ScaleGesture.AllowSimultaneousExecutionWithAllGestures();
                if (DoubleTapGesture != null)
                {
                    DoubleTapGesture.AllowSimultaneousExecutionWithAllGestures();
                }
            }
            else
            {
                PanGesture.AllowSimultaneousExecution(ScaleGesture);
                PanGesture.AllowSimultaneousExecution(RotateGesture);
                ScaleGesture.AllowSimultaneousExecution(RotateGesture);
                if (DoubleTapGesture != null)
                {
                    DoubleTapGesture.AllowSimultaneousExecution(ScaleGesture);
                    DoubleTapGesture.AllowSimultaneousExecution(RotateGesture);
                    DoubleTapGesture.AllowSimultaneousExecution(PanGesture);
                }
            }
            if (Mode == GestureRecognizerComponentScriptBase.GestureObjectMode.RequireIntersectWithGameObject)
            {
                RotateGesture.PlatformSpecificView = gameObject;
                PanGesture.PlatformSpecificView = gameObject;
                ScaleGesture.PlatformSpecificView = gameObject;
                if (DoubleTapGesture != null)
                {
                    DoubleTapGesture.PlatformSpecificView = gameObject;
                }
            }
            FingersScript.Instance.AddGesture(PanGesture);
            FingersScript.Instance.AddGesture(ScaleGesture);
            FingersScript.Instance.AddGesture(RotateGesture);
            FingersScript.Instance.AddGesture(DoubleTapGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(ScaleGesture);
                FingersScript.Instance.RemoveGesture(RotateGesture);
                FingersScript.Instance.RemoveGesture(DoubleTapGesture);
            }
        }
    }
}
