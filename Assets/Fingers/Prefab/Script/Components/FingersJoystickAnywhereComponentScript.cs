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

namespace DigitalRubyShared
{
    /// <summary>
    /// The joystick anywhere script creates a virtual joystick wherever the pan starts. This acts as the joystick center until finger lifts.
    /// When a new pan starts, the new joystick center is wherever that pan started.
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Control/Fingers Joystick Anywhere", 0)]
    public class FingersJoystickAnywhereComponentScript : MonoBehaviour
    {
        /// <summary>The number of units where the pan will be at maximum. For example, if you move this many units (default is inches) or more from the pan start, your move for that axis would be the maximum.</summary>
        [Header("Control")]
        [Tooltip("The number of units where the pan will be at maximum. For example, if you move this many units (default is inches) or more from " +
            "the pan start, your move for that axis would be the maximum.")]
        [Range(0.1f, 5.0f)]
        public float PanUnitsForMaxMove = 1.5f;

        /// <summary>Whether a tap will register as a jump. False to not allow tap to jump.</summary>
        [Tooltip("Whether a tap will register as a jump. False to not allow tap to jump.")]
        public bool TapToJump;

        /// <summary>The game object the pan must happen in, null for anywhere</summary>
        [Tooltip("The game object the pan must happen in, null for anywhere")]
        public GameObject PanPlatformSpecificView;

        /// <summary>The game object the tap must happen in, null for anywhere</summary>
        [Tooltip("The game object the tap must happen in, null for anywhere")]
        public GameObject TapPlatformSpecificView;

        /// <summary>How far the pan gesture must move before executing in units (inches)</summary>
        [Header("Thresholds")]
        [Tooltip("How far the pan gesture must move before executing in units (inches)")]
        [Range(0.0f, 1.0f)]
        public float PanGestureThresholdUnits = 0.1f;

        /// <summary>How far the tap gesture can move and still count as a tap in units (inches)</summary>
        [Tooltip("How far the tap gesture can move and still count as a tap in units (inches)")]
        [Range(0.0f, 5.0f)]
        public float TapGestureThresholdUnits = 4.0f;

        /// <summary>Show where the pan center is currently, null to not do this. This is the center for the current joystick, depending on where the pan started. As the pan moves away from this center, the pan amount increases up to 1.</summary>
        [Header("UI")]
        [Tooltip("Show where the pan center is currently, null to not do this. This is the center for " +
            "the current joystick, depending on where the pan started. As the pan moves away from " +
            "this center, the pan amount increases up to 1.")]
        public UnityEngine.UI.Image PanAnchor;

        /// <summary>Allows drawing a line from pan anchor to current pan to help with seeing the current pan position from anchor.</summary>
        [Tooltip("Allows drawing a line from pan anchor to current pan to help with seeing the current " +
            "pan position from anchor.")]
        public UnityEngine.UI.Image PanAnchorLine;

        /// <summary>Horizontal input axis name if cross platform input integration is desired.</summary>
        [Header("Cross Platform Input")]
        [Tooltip("Horizontal input axis name if cross platform input integration is desired.")]
        public string CrossPlatformInputHorizontalAxisName;

        /// <summary>Vertical input axis name if cross platform input integration is desired.</summary>
        [Tooltip("Vertical input axis name if cross platform input integration is desired.")]
        public string CrossPlatformInputVerticalAxisName;

        /// <summary>Jump input axis name if cross platform input integration is desired. Sends a value of 1.0 if jumped.</summary>
        [Tooltip("Jump input axis name if cross platform input integration is desired. Sends a value of 1.0 if jumped.")]
        public string CrossPlatformInputJumpAxisName;

        /// <summary>The pan/move callback (values are between -1 and 1)</summary>
        [Header("Callbacks")]
        [Tooltip("The pan/move callback (values are between -1 and 1)")]
        public GestureRecognizerComponentEventVector2 PanCallback;

        /// <summary>The tap/jump callback</summary>
        [Tooltip("The tap/jump callback")]
        public GestureRecognizerComponentEvent TapCallback;

        private object crossPlatformInputHorizontalAxisObject;
        private object crossPlatformInputVerticalAxisObject;
        private object crossPlatformInputJumpAxisObject;
        private bool crossPlatformInputAxisMoveNewlyRegistered;
        private bool crossPlatformInputAxisJumpNewlyRegistered;

        public PanGestureRecognizer PanGesture { get; private set; }
        public TapGestureRecognizer TapGesture { get; private set; }

        private System.Action clearJumpAction;

        private Vector2 panLocation;

        /// <summary>
        /// Reset all the gestures in this component script
        /// </summary>
        public void Reset()
        {
            PanGesture.Reset();
            TapGesture.Reset();
            if (PanAnchor != null)
            {
                PanAnchor.enabled = false;
                panLocation = Vector2.zero;
            }
            if (PanAnchorLine != null)
            {
                PanAnchorLine.enabled = false;
            }
        }

        private void Update()
        {
            if (crossPlatformInputVerticalAxisObject != null)
            {
                FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputJumpAxisObject, 0.0f);
            }

            if (PanAnchor != null && PanAnchor.enabled && PanAnchorLine != null && PanAnchorLine.enabled && panLocation != Vector2.zero)
            {
                Vector2 start = PanAnchor.transform.position;
                Vector2 end = panLocation;
                PanAnchorLine.DrawLine(start, end, 1.0f);
            }
        }

        private void OnEnable()
        {
            PanGesture = new PanGestureRecognizer
            {
                PlatformSpecificView = PanPlatformSpecificView,
                ThresholdUnits = PanGestureThresholdUnits
            };
            PanGesture.StateUpdated += Panned;

            TapGesture = new TapGestureRecognizer
            {
                ClearTrackedTouchesOnEndOrFail = true,
                MaximumNumberOfTouchesToTrack = 10,
                PlatformSpecificView = TapPlatformSpecificView,
                ThresholdUnits = TapGestureThresholdUnits
            };
            TapGesture.StateUpdated += Tapped;
            TapGesture.AllowSimultaneousExecution(PanGesture);

            if (!string.IsNullOrEmpty(CrossPlatformInputHorizontalAxisName) && !string.IsNullOrEmpty(CrossPlatformInputVerticalAxisName))
            {
                crossPlatformInputHorizontalAxisObject = FingersCrossPlatformInputReflectionScript.RegisterVirtualAxis(CrossPlatformInputHorizontalAxisName, out crossPlatformInputAxisMoveNewlyRegistered);
                crossPlatformInputVerticalAxisObject = FingersCrossPlatformInputReflectionScript.RegisterVirtualAxis(CrossPlatformInputVerticalAxisName, out crossPlatformInputAxisMoveNewlyRegistered);
            }
            if (!string.IsNullOrEmpty(CrossPlatformInputJumpAxisName))
            {
                crossPlatformInputJumpAxisObject = FingersCrossPlatformInputReflectionScript.RegisterVirtualAxis(CrossPlatformInputJumpAxisName, out crossPlatformInputAxisJumpNewlyRegistered);
            }
            clearJumpAction = ClearJump;

            FingersScript.Instance.AddGesture(PanGesture);
            FingersScript.Instance.AddGesture(TapGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(TapGesture);
            }
        }

        private void OnDestroy()
        {
            if (crossPlatformInputAxisMoveNewlyRegistered && !string.IsNullOrEmpty(CrossPlatformInputHorizontalAxisName) && !string.IsNullOrEmpty(CrossPlatformInputVerticalAxisName))
            {
                FingersCrossPlatformInputReflectionScript.UnRegisterVirtualAxis(CrossPlatformInputHorizontalAxisName);
                FingersCrossPlatformInputReflectionScript.UnRegisterVirtualAxis(CrossPlatformInputVerticalAxisName);
            }
            if (crossPlatformInputAxisJumpNewlyRegistered && !string.IsNullOrEmpty(CrossPlatformInputJumpAxisName))
            {
                FingersCrossPlatformInputReflectionScript.UnRegisterVirtualAxis(CrossPlatformInputJumpAxisName);
            }
        }

        private void ClearJump()
        {
            if (crossPlatformInputVerticalAxisObject != null)
            {
                FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(CrossPlatformInputJumpAxisName, 0.0f);
            }
        }

        private void Panned(GestureRecognizer panGesture)
        {
            if (panGesture.State == GestureRecognizerState.Began)
            {
                if (PanAnchor != null)
                {
                    PanAnchor.enabled = true;
                    PanAnchor.transform.position = new Vector3(panGesture.StartFocusX, panGesture.StartFocusY, PanAnchor.transform.position.z);
                }
                if (PanAnchorLine != null)
                {
                    PanAnchorLine.enabled = true;
                    PanAnchorLine.DrawLine(Vector2.zero, Vector2.zero, 0.0f);
                }
            }
            else if (panGesture.State == GestureRecognizerState.Executing)
            {
                float unitsX = DeviceInfo.PixelsToUnits(panGesture.DistanceX);
                float unitsY = DeviceInfo.PixelsToUnits(panGesture.DistanceY);
                float panX = Mathf.Sign(unitsX) * Mathf.Lerp(0.0f, 1.0f, Mathf.Abs(unitsX) / PanUnitsForMaxMove);
                float panY = Mathf.Sign(unitsY) * Mathf.Lerp(0.0f, 1.0f, Mathf.Abs(unitsY) / PanUnitsForMaxMove);
                if (PanCallback != null)
                {
                    PanCallback.Invoke(new Vector2(panX, panY));
                }
                if (crossPlatformInputHorizontalAxisObject != null)
                {
                    FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputHorizontalAxisObject, panX);
                }
                if (crossPlatformInputVerticalAxisObject != null)
                {
                    FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputVerticalAxisObject, panY);
                }
                if (PanAnchor != null)
                {
                    panLocation = new Vector2(panGesture.FocusX, panGesture.FocusY);
                }
            }
            else if (panGesture.State == GestureRecognizerState.Ended || panGesture.State == GestureRecognizerState.Failed)
            {
                if (PanCallback != null)
                {
                    PanCallback.Invoke(new Vector2(0.0f, 0.0f));
                }
                if (crossPlatformInputHorizontalAxisObject != null)
                {
                    FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputHorizontalAxisObject, 0.0f);
                }
                if (crossPlatformInputVerticalAxisObject != null)
                {
                    FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputVerticalAxisObject, 0.0f);
                }
                if (PanAnchor != null)
                {
                    PanAnchor.enabled = false;
                    panLocation = Vector2.zero;
                }
                if (PanAnchorLine != null)
                {
                    PanAnchorLine.enabled = false;
                }
            }
        }

        private void Tapped(GestureRecognizer tapGesture)
        {
            if (TapToJump && tapGesture.State == GestureRecognizerState.Ended)
            {
                if (TapCallback != null)
                {
                    TapCallback.Invoke();
                }
                if (crossPlatformInputVerticalAxisObject != null)
                {
                    FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputJumpAxisObject, 1.0f);

                    // clear jump axis next frame so the player doesn't keep jumping
                    GestureRecognizer.RunActionAfterDelay(0.001f, clearJumpAction);
                }
            }
        }
    }
}
