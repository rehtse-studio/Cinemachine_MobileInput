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

namespace DigitalRubyShared
{
    /// <summary>
    /// Useful script for using a pan gesture to move an object forward or back along z axis using pan up and down
    /// and left or right using pan left or right
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Pan AR", 5)]
    public class FingersPanARComponentScript : MonoBehaviour
    {
        private struct OrigState
        {
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        /// <summary>The camera to use to convert screen coordinates to world coordinates. Defaults to Camera.main.</summary>
        [Tooltip("The camera to use to convert screen coordinates to world coordinates. Defaults to Camera.main.")]
        public Camera Camera;

        /// <summary>Target game objects. If null, gets set to the transform of this script. These will be raycasted to determine which object gets acted upon.</summary>
        [Tooltip("Target game objects. If null, gets set to the transform of this script. These will be raycasted to determine which object " +
            "gets acted upon.")]
        public List<Transform> Targets;

        /// <summary>The speed at which to move the object forward and backwards.</summary>
        [Range(-10.0f, 10.0f)]
        [Tooltip("The speed at which to move the object forward and backwards.")]
        public float SpeedForwardBack = 2.0f;

        /// <summary>The speed at which to move the object left and right.</summary>
        [Range(-10.0f, 10.0f)]
        [Tooltip("The speed at which to move the object left and right.")]
        public float SpeedLeftRight = 2.0f;

        /// <summary>Rotation speed, set to 0 to disable rotation.</summary>
        [Range(-3.0f, 3.0f)]
        [Tooltip("Rotation speed, set to 0 to disable rotation.")]
        public float RotateSpeed = 3.0f;

        /// <summary>Scale speed. Set to 0 to disable scaling.</summary>
        [Range(0.0f, 10.0f)]
        [Tooltip("Scale speed. Set to 0 to disable scaling.")]
        public float ScaleSpeed = 1.0f;

        /// <summary>Whether a double tap will reset rotation.</summary>
        [Tooltip("Whether a double tap will reset rotation.")]
        public bool DoubleTapToResetRotation = true;

        /// <summary>Allow triple tap gesture to destroy the object.</summary>
        [Tooltip("Allow triple tap gesture to destroy the object.")]
        public bool TripleTapToDestroy;

        /// <summary>Orbit speed (desktop only, right mouse button and drag).</summary>
        [Range(-3.0f, 3.0f)]
        [Tooltip("Orbit speed (desktop only, right mouse button and drag).")]
        public float OrbitSpeed = 0.25f;

        /// <summary>
        /// Pan gesture
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// Rotate gesture
        /// </summary>
        public RotateGestureRecognizer RotateGesture { get; private set; }

        /// <summary>
        /// Scale gesture
        /// </summary>
        public ScaleGestureRecognizer ScaleGesture { get; private set; }

        /// <summary>
        /// Double tap gesture to reset rotation
        /// </summary>
        public TapGestureRecognizer TapGestureReset { get; private set; }

        /// <summary>
        /// Triple tap gesture to destroy
        /// </summary>
        public TapGestureRecognizer TapGestureDestroy { get; private set; }

        /// <summary>
        /// Long press gesture to destroy
        /// </summary>
        public LongPressGestureRecognizer LongPressGesture { get; private set; }

        /// <summary>
        /// Long press callback to do things like show a popup menu
        /// </summary>
        public System.Action<FingersPanARComponentScript> LongPressGestureBegan { get; set; }

        private Vector3? orbitTarget;
        private float prevMouseX;
        private Transform currentTarget;
        private readonly List<KeyValuePair<Transform, OrigState>> origStates = new List<KeyValuePair<Transform, OrigState>>();
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        private readonly List<GestureRecognizer> gestures = new List<GestureRecognizer>();

        private Transform SelectCurrentTarget(float x, float y)
        {
            if (EventSystem.current == null)
            {
                Debug.LogError(GetType().Name + " requires an EventSystem in the scene");
                return null;
            }
            else if (currentTarget != null)
            {
                return currentTarget;
            }

            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = new Vector2(x, y);
            raycastResults.Clear();
            EventSystem.current.RaycastAll(data, raycastResults);
            foreach (RaycastResult result in raycastResults)
            {
                foreach (Transform t in Targets)
                {
                    if (result.gameObject.transform == t)
                    {
                        return (currentTarget = t);
                    }
                }
            }
            return null;
        }

        private void PushGesture(GestureRecognizer gesture)
        {
            if (!gestures.Contains(gesture))
            {
                gestures.Add(gesture);
            }
        }

        private void PopGesture(GestureRecognizer gesture)
        {
            gestures.Remove(gesture);
        }

        private void PanGestureStateUpdated(GestureRecognizer gesture)
        {
            if (currentTarget == null && gesture.State == GestureRecognizerState.Began)
            {
                SelectCurrentTarget(gesture.FocusX, gesture.FocusY);
                PushGesture(gesture);
            }
            else if (currentTarget != null && gesture.State == GestureRecognizerState.Executing && orbitTarget == null)
            {
                Vector3 right = Camera.transform.right;
                right.y = 0.0f;
                right = right.normalized;
                Vector3 forward = Camera.transform.forward;
                forward.y = 0.0f;
                forward = forward.normalized;
                currentTarget.Translate(right * gesture.DeltaX * Time.deltaTime * SpeedLeftRight, Space.World);
                currentTarget.Translate(forward * gesture.DeltaY * Time.deltaTime * SpeedForwardBack, Space.World);
            }
            else if (gesture.State == GestureRecognizerState.Ended || gesture.State == GestureRecognizerState.Failed)
            {
                PopGesture(gesture);
            }
        }

        private void RotateGestureStateUpdated(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began)
            {
                SelectCurrentTarget(gesture.FocusX, gesture.FocusY);
                PushGesture(gesture);
            }
            else if (currentTarget != null && gesture.State == GestureRecognizerState.Executing && orbitTarget == null)
            {
                currentTarget.Rotate(currentTarget.up, RotateGesture.RotationDegreesDelta * RotateSpeed);
            }
            else if (gesture.State == GestureRecognizerState.Ended || gesture.State == GestureRecognizerState.Failed)
            {
                PopGesture(gesture);
            }
        }

        private void ScaleGestureStateUpdated(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began)
            {
                SelectCurrentTarget(gesture.FocusX, gesture.FocusY);
                PushGesture(gesture);
            }
            else if (currentTarget != null && gesture.State == GestureRecognizerState.Executing && orbitTarget == null && ScaleSpeed > Mathf.Epsilon)
            {
                currentTarget.localScale *= ScaleGesture.ScaleMultiplier;
            }
            else if (gesture.State == GestureRecognizerState.Ended || gesture.State == GestureRecognizerState.Failed)
            {
                PopGesture(gesture);
            }
        }

        private void TapGestureResetStateUpdated(GestureRecognizer gesture)
        {
            if (currentTarget == null && gesture.State == GestureRecognizerState.Ended && DoubleTapToResetRotation)
            {
                Debug.Log("Double tap on fingers pan ar component script ended");

                if (SelectCurrentTarget(gesture.FocusX, gesture.FocusY) != null)
                {
                    foreach (KeyValuePair<Transform, OrigState> kv in origStates)
                    {
                        if (kv.Key == currentTarget)
                        {
                            currentTarget.rotation = kv.Value.Rotation;
                            currentTarget.localScale = kv.Value.Scale;
                        }
                    }
                    currentTarget = null;
                }
            }
        }

        private void TapGestureDestroyStateUpdated(GestureRecognizer gesture)
        {
            if (currentTarget == null && gesture.State == GestureRecognizerState.Ended && TripleTapToDestroy)
            {
                Debug.Log("Tripe tap on fingers pan ar component script ended");

                if (SelectCurrentTarget(gesture.FocusX, gesture.FocusY) != null)
                {
                    Destroy(currentTarget.gameObject);
                    currentTarget = null;
                }
            }
        }

        private void LongPressGestureStateUpdated(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began)
            {
                Debug.Log("Long tap on fingers pan ar component script began");

                SelectCurrentTarget(gesture.FocusX, gesture.FocusY);
                PushGesture(gesture);
                if (LongPressGestureBegan != null)
                {
                    LongPressGestureBegan.Invoke(this);
                }
                gesture.Reset();
            }
            else if (gesture.State == GestureRecognizerState.Ended || gesture.State == GestureRecognizerState.Failed)
            {
                PopGesture(gesture);
            }
        }

        private void UpdateOrigStates()
        {
            if (Targets == null)
            {
                return;
            }

            for (int i = Targets.Count - 1; i >= 0; i--)
            {
                Transform t = Targets[i];
                if (t == null)
                {
                    Targets.RemoveAt(i);
                }
                else
                {
                    bool exists = false;
                    foreach (KeyValuePair<Transform, OrigState> kv in origStates)
                    {
                        if (kv.Key == t)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        origStates.Add(new KeyValuePair<Transform, OrigState>(t, new OrigState { Rotation = t.rotation, Scale = t.localScale }));
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (Camera == null)
            {
                Camera = Camera.main;
            }

            Targets = (Targets == null || Targets.Count == 0 ? new List<Transform> { transform } : Targets);
            UpdateOrigStates();

            PanGesture = new PanGestureRecognizer();
            PanGesture.StateUpdated += PanGestureStateUpdated;
            FingersScript.Instance.AddGesture(PanGesture);

            RotateGesture = new RotateGestureRecognizer();
            RotateGesture.StateUpdated += RotateGestureStateUpdated;
            RotateGesture.AllowSimultaneousExecution(PanGesture);
            FingersScript.Instance.AddGesture(RotateGesture);

            ScaleGesture = new ScaleGestureRecognizer();
            ScaleGesture.StateUpdated += ScaleGestureStateUpdated;
            ScaleGesture.ZoomSpeed *= ScaleSpeed;
            ScaleGesture.AllowSimultaneousExecution(RotateGesture);
            ScaleGesture.AllowSimultaneousExecution(PanGesture);
            FingersScript.Instance.AddGesture(ScaleGesture);

            TapGestureReset = new TapGestureRecognizer();
            TapGestureReset.NumberOfTapsRequired = 2;
            TapGestureReset.StateUpdated += TapGestureResetStateUpdated;
            FingersScript.Instance.AddGesture(TapGestureReset);

            TapGestureDestroy = new TapGestureRecognizer();
            TapGestureDestroy.NumberOfTapsRequired = 3;
            TapGestureDestroy.StateUpdated += TapGestureDestroyStateUpdated;
            FingersScript.Instance.AddGesture(TapGestureDestroy);
            TapGestureReset.RequireGestureRecognizerToFail = TapGestureDestroy;

            LongPressGesture = new LongPressGestureRecognizer();
            LongPressGesture.StateUpdated += LongPressGestureStateUpdated;
            FingersScript.Instance.AddGesture(LongPressGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(RotateGesture);
                FingersScript.Instance.RemoveGesture(ScaleGesture);
                FingersScript.Instance.RemoveGesture(TapGestureReset);
                FingersScript.Instance.RemoveGesture(TapGestureDestroy);
            }
        }

        private void Update()
        {
            UpdateOrigStates();
            switch (Application.platform)
            {
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    if (currentTarget != null && FingersScript.Instance.IsMouseDownThisFrame(1))
                    {
                        orbitTarget = currentTarget.position;
                        prevMouseX = FingersScript.Instance.MousePosition.x;
                    }
                    else if (FingersScript.Instance.IsMouseUpThisFrame(1))
                    {
                        orbitTarget = null;
                    }
                    if (orbitTarget != null)
                    {
                        Camera.transform.RotateAround(orbitTarget.Value, Vector3.up, (FingersScript.Instance.MousePosition.x - prevMouseX) * Time.deltaTime * OrbitSpeed);
                    }
                    break;
            }

            // if all gestures are gone, remove the target
            if (gestures.Count == 0)
            {
                currentTarget = null;
            }
        }
    }
}
