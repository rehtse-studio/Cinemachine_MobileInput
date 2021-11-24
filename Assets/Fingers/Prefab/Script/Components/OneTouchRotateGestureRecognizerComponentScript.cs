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
    /// Wraps a one touch rotate gesture in a component
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Gesture/Fingers Rotate Gesture (One Touch)", 4)]
    public class OneTouchRotateGestureRecognizerComponentScript : GestureRecognizerComponentScript<OneTouchRotateGestureRecognizer>
    {
        /// <summary>Angle threshold in radians that must be met before rotation starts - this is the amount of rotation that must happen to start the gesture.</summary>
        [Header("One touch rotate gesture properties")]
        [Tooltip("Angle threshold in radians that must be met before rotation starts - this is the amount of rotation that must happen to start the gesture.")]
        [Range(0.01f, 0.5f)]
        public float AngleThreshold = 0.05f;

        /// <summary>The gesture focus must change distance by this number of units from the start focus in order to start.</summary>
        [Tooltip("The gesture focus must change distance by this number of units from the start focus in order to start.")]
        [Range(0.0f, 1.0f)]
        public float ThresholdUnits = 0.15f;

        /// <summary>
        /// OnEnable
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.AngleThreshold = AngleThreshold;
            Gesture.ThresholdUnits = ThresholdUnits;
            Gesture.MinimumNumberOfTouchesToTrack = MinimumNumberOfTouchesToTrack =
                Gesture.MaximumNumberOfTouchesToTrack = MaximumNumberOfTouchesToTrack = 1;
        }
    }
}
