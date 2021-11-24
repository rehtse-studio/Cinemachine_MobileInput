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
    /// Wraps a long press gesture in a component
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Gesture/Fingers Long Press Gesture", 3)]
    public class LongPressGestureRecognizerComponentScript : GestureRecognizerComponentScript<LongPressGestureRecognizer>
    {
        /// <summary>The number of seconds that the touch must stay down to begin executing.</summary>
        [Header("Long press gesture properties")]
        [Tooltip("The number of seconds that the touch must stay down to begin executing.")]
        [Range(0.01f, 1.0f)]
        public float MinimumDurationSeconds = 0.6f;

        /// <summary>How many units away the long press can move before failing. After the long press begins, it is allowed to move any distance and stay executing.</summary>
        [Tooltip("How many units away the long press can move before failing. After the long press begins, " +
            "it is allowed to move any distance and stay executing.")]
        [Range(0.01f, 1.0f)]
        public float ThresholdUnits = 0.35f;

        /// <summary>
        /// OnEnable
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.MinimumDurationSeconds = MinimumDurationSeconds;
            Gesture.ThresholdUnits = ThresholdUnits;
        }
    }
}
