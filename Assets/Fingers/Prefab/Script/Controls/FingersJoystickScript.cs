//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalRubyShared
{
    /// <summary>
    /// Represents a joystick that can move in arbitrary direction or 8 axis
    /// </summary>
    public class FingersJoystickScript : MonoBehaviour
    {
        /// <summary>The image to move around like a joystick</summary>
        [Header("Joystick appearance")]
        [Tooltip("The image to move around like a joystick.")]
        public Image JoystickImage;

        /// <summary>The sprite to set on JoystickImage for idle state.</summary>
        [Tooltip("The sprite to set on JoystickImage for idle state.")]
        public Sprite JoystickSpriteIdle;

        /// <summary>The sprite to set on JoystickImage for active state.</summary>
        [Tooltip("The sprite to set on JoystickImage for active state.")]
        public Sprite JoystickSpriteActive;

        /// <summary>The background image of the joystick</summary>
        [Tooltip("The background image of the joystick")]
        public Image JoystickBackground;

        /// <summary>The sprite to set on JoystickBackground for idle state.</summary>
        [Tooltip("The sprite to set on JoystickBackground for idle state.")]
        public Sprite JoystickBackgroundSpriteIdle;

        /// <summary>The sprite to set on JoystickBackground for active state.</summary>
        [Tooltip("The sprite to set on JoystickBackground for active state.")]
        public Sprite JoystickBackgroundSpriteActive;

        /// <summary>Dead zones for the joystick. Below the min, no movement. Above the max, max movement.</summary>
        [Header("Joystick Behavior")]
        [MinMaxSlider]
        [Tooltip("Dead zones for the joystick. Below the min, no movement. Above the max, max movement.")]
        public Vector2 DeadZones = new Vector2(0.1f, 0.9f);

        /// <summary>The input curve, as the joystick moves from min dead zone to max dead zone, it will follow the curve to max movement.</summary>
        [Tooltip("The input curve, as the joystick moves from min dead zone to max dead zone, it will follow the curve to max movement.")]
        public AnimationCurve InputCurve = new AnimationCurve { keys = new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f) } };

        /// <summary>In eight axis mode, the joystick can only move up, down, left, right or diagonally. No in between.</summary>
        [Tooltip("In eight axis mode, the joystick can only move up, down, left, right or diagonally. No in between.")]
        public bool EightAxisMode;

        /// <summary>Reduce the max range of the joystick based on radius of the joystick</summary>
        [Tooltip("Reduce the max range of the joystick based on radius of the joystick")]
        [Range(0.5f, 1.0f)]
        public float MaxRangeRadiusMultiplier = 1.0f;

        /// <summary>The speed at which the joystick moves with the finger if it is dragged beyond the edge. 0 for no follow, 1 for very close follow.</summary>
        [Tooltip("The speed at which the joystick moves with the finger if it is dragged beyond the edge. 0 for no follow, 1 for very close follow.")]
        [Range(0.0f, 1.0f)]
        public float FollowSpeed = 0.0f;

        /// <summary>Whether to move the joystick when the pan gesture starts.</summary>
        [Tooltip("How the joystick will be started.")]
        public JoystickStartMode StartMode = JoystickStartMode.MoveToPanStart;

        /// <summary>Unity 2019+ required. Restrict joystick position to this collider2D, null for no restriction. Place colider in joystick parent object.</summary>
        [Tooltip("Unity 2019+ required. Restrict joystick position to this collider2D, null for no restriction. Place collider in joystick parent object.")]
        public Collider2D RestrictJoystickPositionTo;

        /// <summary>Whether to return the joystick to home position if MoveJoystickToGestureStartLocation is set and joystick gesture ends.</summary>
        [Tooltip("Whether to return the joystick to home position if MoveJoystickToGestureStartLocation is true and joystick gesture ends.")]
        public bool ReturnToHomePosition;

        /// <summary>Horizontal input axis name if cross platform input integration is desired.</summary>
        [Header("Cross Platform Input")]
        [Tooltip("Horizontal input axis name if cross platform input integration is desired.")]
        public string CrossPlatformInputHorizontalAxisName;

        /// <summary>Vertical input axis name if cross platform input integration is desired.</summary>
        [Tooltip("Vertical input axis name if cross platform input integration is desired.")]
        public string CrossPlatformInputVerticalAxisName;

        /// <summary>
        /// Fires when joystick changes
        /// </summary>
        [Header("Callbacks")]
        public GestureRecognizerComponentEventVector2 JoystickCallback;

        private object crossPlatformInputHorizontalAxisObject;
        private object crossPlatformInputVerticalAxisObject;
        private bool crossPlatformInputNewlyRegistered;
        private Vector2 startCenter;
        private Vector2 homePosition;
        private RectTransform rectTransform;

        private readonly PanGestureRecognizer panGesture = new PanGestureRecognizer();
        /// <summary>
        /// Pan gesture
        /// </summary>
        public PanGestureRecognizer PanGesture { get { return panGesture; } }

        /// <summary>
        /// Fires when the joystick moves
        /// </summary>
        public System.Action<FingersJoystickScript, Vector2> JoystickExecuted;

        /// <summary>
        /// The last amount the joystick was moved, the value sent to the callback
        /// </summary>
        public Vector2 CurrentAmount { get; private set; }

        /// <summary>
        /// Whether the joystick is currently executing
        /// </summary>
        public bool Executing { get; private set; }

        /// <summary>
        /// Reset the joystick state
        /// </summary>
        public void Reset()
        {
            PanGesture.Reset();
            SetImagePosition(startCenter);
            Executing = false;
        }

        private void SetImagePosition(Vector2 pos)
        {
            if (JoystickImage != null)
            {
                JoystickImage.rectTransform.anchoredPosition = pos;
            }
        }

        private Vector2 UpdateForEightAxisMode(Vector2 amount, float maxOffset)
        {
            if (EightAxisMode)
            {
                float absX = Mathf.Abs(amount.x);
                float absY = Mathf.Abs(amount.y);
                if (absX > absY * 1.5f)
                {
                    amount.y = 0.0f;
                    amount.x = Mathf.Sign(amount.x) * maxOffset;
                }
                else if (absY > absX * 1.5)
                {
                    amount.x = 0.0f;
                    amount.y = Mathf.Sign(amount.y) * maxOffset;
                }
                else
                {
                    amount.x = Mathf.Sign(amount.x) * maxOffset * 0.7f;
                    amount.y = Mathf.Sign(amount.y) * maxOffset * 0.7f;
                }
            }
            return amount;
        }

        private void ExecuteCallback(Vector2 amount)
        {
            CurrentAmount = amount;
            if (JoystickExecuted != null)
            {
                JoystickExecuted(this, amount);
            }
            if (JoystickCallback != null)
            {
                JoystickCallback.Invoke(amount);
            }
        }

        private Vector2 ComputeJoystickPowerCircle(Vector2 offset, Vector2 offsetNormalized, float extent)
        {
            // map offset into the dead zones and apply curve
            float radiusPercent = Mathf.Min(1.0f, offset.magnitude / extent);
            if (radiusPercent < DeadZones.x)
            {
                // not enough power yet
                return Vector2.zero;
            }
            else if (radiusPercent > DeadZones.y)
            {
                // maximum power
                return offsetNormalized;
            }

            float deadZoneLerp = (radiusPercent - DeadZones.x);
            float deadZoneGap = (DeadZones.y - DeadZones.x);
            deadZoneLerp /= deadZoneGap;
            Vector2 joystickPower = offsetNormalized * InputCurve.Evaluate(deadZoneLerp);
            return joystickPower;
        }

        private bool StartJoystick(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (Executing)
            {
                return true;
            }

            if (StartMode == JoystickStartMode.MoveToPanStart)
            {
                Vector3 worldPoint;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform.parent as RectTransform, new Vector2(gesture.FocusX, gesture.FocusY), null, out worldPoint);
                rectTransform.position = new Vector3(worldPoint.x, worldPoint.y, rectTransform.position.z);
            }
            else
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform as RectTransform, new Vector2(gesture.FocusX, gesture.FocusY), null, out localPoint);
                if (!rectTransform.rect.Contains(localPoint))
                {
                    return false;
                }
            }

            if (JoystickImage != null)
            {
                startCenter = JoystickImage.rectTransform.anchoredPosition;
            }

            ExecuteCallback(Vector2.zero);
            SetActiveState();
            return true;
        }

        private void PanGestureUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                if (StartJoystick(gesture))
                {
                    // clamp joystick movement to max values
                    float diameter = (rectTransform.rect.width + rectTransform.rect.height) * 0.5f;
                    float radius = diameter * 0.5f;
                    float maxOffset = radius * MaxRangeRadiusMultiplier;
                    Vector2 gestureOffset = new Vector2(gesture.FocusX, gesture.FocusY);
                    Vector2 offset;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, gestureOffset, null, out offset);
                    float distanceFromCenter = offset.magnitude;
                    float overshoot = Mathf.Max(0.0f, distanceFromCenter - radius);
                    Vector2 offsetNormalized = offset.normalized;
                    Vector2 followVelocity = (offsetNormalized * overshoot * FollowSpeed);

                    // check distance from center, clamp to distance
                    offset = Vector2.ClampMagnitude(offset, maxOffset);

                    // don't bother if no motion
                    if (offset == Vector2.zero)
                    {
                        return;
                    }

                    // handle eight axis offset param
                    offset = UpdateForEightAxisMode(offset, maxOffset);

                    // move image
                    SetImagePosition(startCenter + offset);

                    // compute power in circle or square
                    offsetNormalized = ComputeJoystickPowerCircle(offset, offsetNormalized, radius);
                    ExecuteCallback(offsetNormalized);

                    if (crossPlatformInputHorizontalAxisObject != null)
                    {
                        FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputHorizontalAxisObject, offset.x);
                    }
                    if (crossPlatformInputVerticalAxisObject != null)
                    {
                        FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputVerticalAxisObject, offset.y);
                    }

                    rectTransform.Translate(followVelocity);
                }
            }
            else if (gesture.State == GestureRecognizerState.Began)
            {
                StartJoystick(gesture);
            }
            else if (gesture.State == GestureRecognizerState.Ended)
            {
                // return to center
                SetImagePosition(startCenter);

                // reset state
                SetIdleState();

                // final callback
                ExecuteCallback(Vector2.zero);

                if (ReturnToHomePosition)
                {
                    // move to home
                    rectTransform.position = homePosition;
                }
            }

#if UNITY_2019_1_OR_NEWER

            if (RestrictJoystickPositionTo != null)
            {
                Vector2 closestPoint = RestrictJoystickPositionTo.ClosestPoint(rectTransform.position);
                rectTransform.position = closestPoint;
            }

#endif

        }

        private void SetIdleState()
        {
            Executing = false;
            if (JoystickImage != null && JoystickSpriteIdle != null)
            {
                JoystickImage.sprite = JoystickSpriteIdle;
            }
            if (JoystickBackground != null && JoystickBackgroundSpriteActive != null)
            {
                JoystickBackground.sprite = JoystickBackgroundSpriteIdle;
            }
        }

        private void SetActiveState()
        {
            Executing = true;
            if (JoystickImage != null && JoystickSpriteActive != null)
            {
                JoystickImage.sprite = JoystickSpriteActive;
            }
            if (JoystickBackground != null && JoystickBackgroundSpriteActive != null)
            {
                JoystickBackground.sprite = JoystickBackgroundSpriteActive;
            }
        }

        private void OnEnable()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            PanGesture.ThresholdUnits = 0.0f;
            PanGesture.AllowSimultaneousExecutionWithAllGestures();
            PanGesture.StateUpdated += PanGestureUpdated;

#if UNITY_EDITOR

            if (JoystickImage != null && JoystickImage.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError("Fingers joystick script requires that if JoystickImage is set, the Canvas is in ScreenSpaceOverlay mode.");
            }

#endif

            if (!string.IsNullOrEmpty(CrossPlatformInputHorizontalAxisName) && !string.IsNullOrEmpty(CrossPlatformInputVerticalAxisName))
            {
                crossPlatformInputHorizontalAxisObject = FingersCrossPlatformInputReflectionScript.RegisterVirtualAxis(CrossPlatformInputHorizontalAxisName, out crossPlatformInputNewlyRegistered);
                crossPlatformInputVerticalAxisObject = FingersCrossPlatformInputReflectionScript.RegisterVirtualAxis(CrossPlatformInputVerticalAxisName, out crossPlatformInputNewlyRegistered);
            }

            FingersScript.Instance.AddGesture(PanGesture);
            SetIdleState();
            homePosition = rectTransform.position;
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
                Reset();
            }
        }

        private void OnDestroy()
        {
            if (crossPlatformInputNewlyRegistered && !string.IsNullOrEmpty(CrossPlatformInputHorizontalAxisName) && !string.IsNullOrEmpty(CrossPlatformInputVerticalAxisName))
            {
                FingersCrossPlatformInputReflectionScript.UnRegisterVirtualAxis(CrossPlatformInputHorizontalAxisName);
                FingersCrossPlatformInputReflectionScript.UnRegisterVirtualAxis(CrossPlatformInputVerticalAxisName);
            }
        }
    }

    /// <summary>
    /// Joystick start modes
    /// </summary>
    public enum JoystickStartMode
    {
        /// <summary>
        /// Start only if pan gesture starts inside the joystick 
        /// </summary>
        MustStartInside = 0,

        /// <summary>
        /// Move joystick to pan start
        /// </summary>
        MoveToPanStart = 1,

        /// <summary>
        /// Do not move joystick, but start joystick if pan moves inside joystick
        /// </summary>
        PanInsideOrPanMovesInside = 2
    }
}