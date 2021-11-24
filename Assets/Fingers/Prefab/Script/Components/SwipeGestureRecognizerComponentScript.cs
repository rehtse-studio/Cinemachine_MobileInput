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
    /// Wraps a swipe gesture in a component
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Gesture/Fingers Swipe Gesture", 2)]
    public class SwipeGestureRecognizerComponentScript : GestureRecognizerComponentScript<SwipeGestureRecognizer>
    {
        /// <summary>The swipe direction required to recognize the gesture (default is any)</summary>
        [Header("Swipe gesture properties")]
        [Tooltip("The swipe direction required to recognize the gesture (default is any)")]
        public SwipeGestureRecognizerDirection Direction = SwipeGestureRecognizerDirection.Any;

        /// <summary>The minimum distance the swipe must travel to be recognized.</summary>
        [Tooltip("The minimum distance the swipe must travel to be recognized.")]
        [Range(0.01f, 10.0f)]
        public float MinimumDistanceUnits = 1.0f;

        /// <summary>The minimum units per second the swipe must travel to be recognized.</summary>
        [Tooltip("The minimum units per second the swipe must travel to be recognized.")]
        [Range(0.01f, 10.0f)]
        public float MinimumSpeedUnits = 3.0f;

        /// <summary>For set directions, this is the amount that the swipe must be proportionally in that direction vs the other direction. For example, a swipe down gesture will need to move in the y axis by this multiplier more versus moving along the x axis.</summary>
        [Tooltip("For set directions, this is the amount that the swipe must be proportionally in that direction " +
            "vs the other direction. For example, a swipe down gesture will need to move in the y axis " +
            "by this multiplier more versus moving along the x axis.")]
        [Range(0.0f, 5.0f)]
        public float DirectionThreshold = 1.5f;

        /// <summary>Controls how the swipe gesture ends. See SwipeGestureRecognizerSwipeMode enum for more details.</summary>
        [Tooltip("Controls how the swipe gesture ends. See SwipeGestureRecognizerSwipeMode enum for more details.")]
        public SwipeGestureRecognizerEndMode EndMode = SwipeGestureRecognizerEndMode.EndImmediately;

        /// <summary>Whether to fail if the gesture changes direction mid swipe</summary>
        [Tooltip("Whether to fail if the gesture changes direction mid swipe")]
        public bool FailOnDirectionChange;

        /// <summary>Whether to send begin and executing states. Default is true. If false, only possible, ended or failed state is sent.</summary>
        [Tooltip("Whether to send begin and executing states. Default is true. If false, only possible, ended or failed state is sent.")]
        public bool SendBeginExecutingStates = true;

        /// <summary>
        /// OnEnable
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.Direction = Direction;
            Gesture.MinimumDistanceUnits = MinimumDistanceUnits;
            Gesture.MinimumSpeedUnits = MinimumSpeedUnits;
            Gesture.DirectionThreshold = DirectionThreshold;
            Gesture.EndMode = EndMode;
            Gesture.FailOnDirectionChange = FailOnDirectionChange;
            Gesture.SendBeginExecutingStates = SendBeginExecutingStates;
        }

    }
}
