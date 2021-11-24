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
    /// Wraps a tap gesture in a component
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Gesture/Fingers Tap Gesture", 0)]
    public class TapGestureRecognizerComponentScript : GestureRecognizerComponentScript<TapGestureRecognizer>
    {
        /// <summary>How many taps must execute in order to end the gesture</summary>
        [Header("Tap gesture properties")]
        [Tooltip("How many taps must execute in order to end the gesture")]
        [Range(1, 5)]
        public int NumberOfTapsRequired = 1;

        /// <summary>How many seconds can expire before the tap is released to still count as a tap</summary>
        [Tooltip("How many seconds can expire before the tap is released to still count as a tap")]
        [Range(0.01f, 1.0f)]
        public float ThresholdSeconds = 0.5f;

        /// <summary>How many units away the tap down and up and subsequent taps can be to still be considered - must be greater than 0.</summary>
        [Tooltip("How many units away the tap down and up and subsequent taps can be to still be considered - must be greater than 0.")]
        [Range(0.01f, 1.0f)]
        public float ThresholdUnits = 0.3f;

        /// <summary>How many seconds can expire before subsequent taps for a multiple tap gesture before the gesture fails - default is 0.4.</summary>
        [Tooltip("How many seconds can expire before subsequent taps for a multiple tap gesture before the gesture fails - default is 0.4.")]
        [Range(0.01f, 1.0f)]
        public float SequentialTapThresholdSeconds = 0.4f;

        /// <summary>Whether the tap gesture will immediately send a begin state when a touch is first down. Default is false.</summary>
        [Tooltip("Whether the tap gesture will immediately send a begin state when a touch is first down. Default is false.")]
        public bool SendBeginState;

        /// <summary>
        /// OnEnable
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.NumberOfTapsRequired = NumberOfTapsRequired;
            Gesture.ThresholdSeconds = ThresholdSeconds;
            Gesture.SequentialTapThresholdSeconds = SequentialTapThresholdSeconds;
            Gesture.ThresholdUnits = ThresholdUnits;
            Gesture.SendBeginState = SendBeginState;
        }
    }
}
