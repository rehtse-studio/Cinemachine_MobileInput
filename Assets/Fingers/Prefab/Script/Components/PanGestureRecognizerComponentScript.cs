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
    /// Wraps a pan gesture in a component
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Gesture/Fingers Pan Gesture", 1)]
    public class PanGestureRecognizerComponentScript : GestureRecognizerComponentScript<PanGestureRecognizer>
    {
        /// <summary>How many units away the pan must move to execute.</summary>
        [Header("Pan gesture properties")]
        [Tooltip("How many units away the pan must move to execute.")]
        [Range(0.0f, 1.0f)]
        public float ThresholdUnits = 0.2f;

        /// <summary>
        /// The speed in units per second which if the pan gesture drops under, ThresholdUnits will be re-enabled and
        /// the gesture will not send execution events until the threshold units is exceeded again.
        /// Default is 0. 0.1f is a good value if you need to place objects exactly.
        /// </summary>
        [Tooltip("The speed in units per second which if the pan gesture drops under, ThresholdUnits will be re-enabled and the gesture will not send execution events until the threshold units is exceeded again. Default is 0. 0.1f is a good value if you need to place objects exactly.")]
        [Range(0.0f, 1.0f)]
        public float SpeedUnitsToRestartThresholdUnits;

        /// <summary>
        /// OnEnable
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.ThresholdUnits = ThresholdUnits;
            Gesture.SpeedUnitsToRestartThresholdUnits = SpeedUnitsToRestartThresholdUnits;
        }
    }
}
