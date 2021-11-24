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
    /// DPad item
    /// </summary>
    [System.Flags]
    public enum FingersDPadItem
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Up
        /// </summary>
        Up = 1,

        /// <summary>
        /// Right
        /// </summary>
        Right = 2,

        /// <summary>
        /// Down
        /// </summary>
        Down = 4,

        /// <summary>
        /// Left
        /// </summary>
        Left = 8,

        /// <summary>
        /// Center
        /// </summary>
        Center = 16
    }

    /// <summary>
    /// Represents a dpad that can be used just like a real-controller with 4 directions
    /// </summary>
    public class FingersDPadScript : MonoBehaviour
    {
        /// <summary>The background image to use for the DPad. This should contain up, right, down, left and center in unselected state.</summary>
        [Tooltip("The background image to use for the DPad. This should contain up, right, down, left and center in unselected state.")]
        public UnityEngine.UI.Image DPadBackgroundImage;

        /// <summary>The up image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.</summary>
        [Tooltip("The up image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.")]
        public UnityEngine.UI.Image DPadUpImageSelected;

        /// <summary>The right image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.</summary>
        [Tooltip("The right image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.")]
        public UnityEngine.UI.Image DPadRightImageSelected;

        /// <summary>The down image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.</summary>
        [Tooltip("The down image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.")]
        public UnityEngine.UI.Image DPadDownImageSelected;

        /// <summary>The left image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.</summary>
        [Tooltip("The left image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.")]
        public UnityEngine.UI.Image DPadLeftImageSelected;

        /// <summary>The center image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.</summary>
        [Tooltip("The center image to use for the DPad for selected state. Alpha pixel of less than MinAlphaForTouch will not be selectable.")]
        public UnityEngine.UI.Image DPadCenterImageSelected;

        /// <summary>Touch radius in units (usually inches). Set to lowest for single pixel accuracy, or larger if you want more than one dpad button interactable at once. You'll need to test this to make sure the DPad works how you expect for an average finger size and your screen size.</summary>
        [Tooltip("Touch radius in units (usually inches). Set to lowest for single pixel accuracy, or larger if you want more than one dpad button interactable at once. " +
            "You'll need to test this to make sure the DPad works how you expect for an average finger size and your screen size.")]
        [Range(0.01f, 1.0f)]
        public float TouchRadiusInUnits = 0.125f;

        /// <summary>Horizontal input axis name if cross platform input integration is desired.</summary>
        [Tooltip("Horizontal input axis name if cross platform input integration is desired.")]
        public string CrossPlatformInputHorizontalAxisName;

        /// <summary>Vertical input axis name if cross platform input integration is desired.</summary>
        [Tooltip("Vertical input axis name if cross platform input integration is desired.")]
        public string CrossPlatformInputVerticalAxisName;

        private readonly Collider2D[] overlap = new Collider2D[32];

        private object crossPlatformInputHorizontalAxisObject;
        private object crossPlatformInputVerticalAxisObject;
        private bool crossPlatformInputNewlyRegistered;

#if UNITY_EDITOR

        private void ValidateImages(params UnityEngine.UI.Image[] images)
        {
            foreach (UnityEngine.UI.Image image in images)
            {
                if (image == null || image.canvas.renderMode == RenderMode.WorldSpace)
                {
                    Debug.LogError("Fingers dpad script requires that all images be set and that the Canvas be in ScreenSpace* mode.");
                }
            }
        }

#endif

        private void CheckForOverlap<T>(Vector2 point, T gesture, System.Action<FingersDPadScript, FingersDPadItem, T> action) where T : DigitalRubyShared.GestureRecognizer
        {
            if (action == null)
            {
                return;
            }

            FingersDPadItem item = FingersDPadItem.None;
            int count = Physics2D.OverlapCircleNonAlloc(point, DeviceInfo.UnitsToPixels(TouchRadiusInUnits), overlap);
            float horizontal = 0.0f;
            float vertical = 0.0f;
            for (int i = 0; i < count; i++)
            {
                if (overlap[i].gameObject == DPadCenterImageSelected.gameObject)
                {
                    DPadCenterImageSelected.enabled = true;
                    item |= FingersDPadItem.Center;
                }
                if (overlap[i].gameObject == DPadRightImageSelected.gameObject)
                {
                    DPadRightImageSelected.enabled = true;
                    item |= FingersDPadItem.Right;
                    horizontal = 1.0f;
                }
                if (overlap[i].gameObject == DPadDownImageSelected.gameObject)
                {
                    DPadDownImageSelected.enabled = true;
                    item |= FingersDPadItem.Down;
                    vertical = -1.0f;
                }
                if (overlap[i].gameObject == DPadLeftImageSelected.gameObject)
                {
                    DPadLeftImageSelected.enabled = true;
                    item |= FingersDPadItem.Left;
                    horizontal = -1.0f;
                }
                if (overlap[i].gameObject == DPadUpImageSelected.gameObject)
                {
                    DPadUpImageSelected.enabled = true;
                    item |= FingersDPadItem.Up;
                    vertical = 1.0f;
                }
            }
            if (item != FingersDPadItem.None)
            {
                action(this, item, gesture);
                if (horizontal != 0.0f && crossPlatformInputHorizontalAxisObject != null)
                {
                    FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputHorizontalAxisObject, horizontal);
                }
                if (vertical != 0.0f && crossPlatformInputVerticalAxisObject != null)
                {
                    FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputVerticalAxisObject, vertical);
                }
            }
        }

        private void DisableButtons()
        {
            DPadUpImageSelected.enabled = false;
            DPadRightImageSelected.enabled = false;
            DPadDownImageSelected.enabled = false;
            DPadLeftImageSelected.enabled = false;
            DPadCenterImageSelected.enabled = false;
        }

        private void PanGestureUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began || gesture.State == GestureRecognizerState.Executing)
            {
                /*
                if (gesture.State == GestureRecognizerState.Began && MoveDPadToGestureStartLocation)
                {
                    transform.position = new Vector3(gesture.FocusX, gesture.FocusY, transform.position.z);
                }
                */
                DisableButtons();
                CheckForOverlap(new Vector2(gesture.FocusX, gesture.FocusY), PanGesture, DPadItemPanned);
            }
            else if (gesture.State == GestureRecognizerState.Ended || gesture.State == GestureRecognizerState.Failed)
            {
                DisableButtons();
            }
        }

        private void TapGestureUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                CheckForOverlap(new Vector2(gesture.FocusX, gesture.FocusY), TapGesture, DPadItemTapped);
                DisableButtons();
            }
        }

        private void OnEnable()
        {

#if UNITY_EDITOR

            ValidateImages(DPadBackgroundImage, DPadUpImageSelected, DPadRightImageSelected, DPadDownImageSelected, DPadLeftImageSelected, DPadCenterImageSelected);

#endif

            PanGesture = new PanGestureRecognizer
            {
                PlatformSpecificView = DPadBackgroundImage.gameObject,
                ThresholdUnits = 0.0f
            };
            PanGesture.StateUpdated += PanGestureUpdated;
            FingersScript.Instance.AddGesture(PanGesture);

            TapGesture = new TapGestureRecognizer
            {
                PlatformSpecificView = DPadBackgroundImage.gameObject
            };
            TapGesture.StateUpdated += TapGestureUpdated;
            TapGesture.AllowSimultaneousExecution(PanGesture);

            if (!string.IsNullOrEmpty(CrossPlatformInputHorizontalAxisName) && !string.IsNullOrEmpty(CrossPlatformInputVerticalAxisName))
            {
                crossPlatformInputHorizontalAxisObject = FingersCrossPlatformInputReflectionScript.RegisterVirtualAxis(CrossPlatformInputHorizontalAxisName, out crossPlatformInputNewlyRegistered);
                crossPlatformInputVerticalAxisObject = FingersCrossPlatformInputReflectionScript.RegisterVirtualAxis(CrossPlatformInputVerticalAxisName, out crossPlatformInputNewlyRegistered);
            }

            FingersScript.Instance.AddGesture(TapGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(TapGesture);
            }

            if (crossPlatformInputNewlyRegistered && !string.IsNullOrEmpty(CrossPlatformInputHorizontalAxisName) && !string.IsNullOrEmpty(CrossPlatformInputVerticalAxisName))
            {
                FingersCrossPlatformInputReflectionScript.UnRegisterVirtualAxis(CrossPlatformInputHorizontalAxisName);
                FingersCrossPlatformInputReflectionScript.UnRegisterVirtualAxis(CrossPlatformInputVerticalAxisName);
            }
        }

        /// <summary>
        /// Fires when a dpad item is tapped
        /// </summary>
        public System.Action<FingersDPadScript, FingersDPadItem, TapGestureRecognizer> DPadItemTapped;

        /// <summary>
        /// Fires when a dpad item is panned on
        /// </summary>
        public System.Action<FingersDPadScript, FingersDPadItem, PanGestureRecognizer> DPadItemPanned;

        /// <summary>
        /// Pan gesture
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// Tap gesture
        /// </summary>
        public TapGestureRecognizer TapGesture { get; private set; }

        /*
        /// <summary>
        /// Whether to move the DPad when the pan gesture starts. Set this in Awake.
        /// </summary>
        public bool MoveDPadToGestureStartLocation { get; set; }
        */
    }
}
