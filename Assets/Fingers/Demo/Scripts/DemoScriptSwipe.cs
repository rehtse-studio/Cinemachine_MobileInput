//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRubyShared
{
    /// <summary>
    /// Swipe demo script
    /// </summary>
    public class DemoScriptSwipe : MonoBehaviour
    {
        /// <summary>
        /// Emit this particle system in the swipe direction.
        /// </summary>
        [Tooltip("Emit this particle system in the swipe direction.")]
        public ParticleSystem SwipeParticleSystem;

        /// <summary>
        /// Set the required touches for the swipe.
        /// </summary>
        [Tooltip("Set the required touches for the swipe.")]
        [Range(1, 10)]
        public int SwipeTouchCount = 1;

        /// <summary>
        /// Controls how the swipe gesture ends. See SwipeGestureRecognizerSwipeMode enum for more details.
        /// </summary>
        [Tooltip("Controls how the swipe gesture ends. See SwipeGestureRecognizerSwipeMode enum for more details.")]
        public SwipeGestureRecognizerEndMode SwipeMode = SwipeGestureRecognizerEndMode.EndImmediately;

        /// <summary>
        /// Threshold in seconds to fail swipe gesture if it has not executed, 0 for no threshold
        /// </summary>
        [Tooltip("Threshold in seconds to fail swipe gesture if it has not executed, 0 for no threshold")]
        [Range(0.0f, 10.0f)]
        public float SwipeThresholdSeconds;

        /// <summary>
        /// Restrict swipe to this view
        /// </summary>
        [Tooltip("Restrict swipe to this view")]
        public GameObject Image;

        private SwipeGestureRecognizer swipe;

        private void Start()
        {
            swipe = new SwipeGestureRecognizer();
            swipe.StateUpdated += Swipe_Updated;
            swipe.DirectionThreshold = 0;
            swipe.MinimumNumberOfTouchesToTrack = swipe.MaximumNumberOfTouchesToTrack = SwipeTouchCount;
            swipe.PlatformSpecificView = Image;
            swipe.ThresholdSeconds = SwipeThresholdSeconds;
            FingersScript.Instance.AddGesture(swipe);
            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.StateUpdated += Tap_Updated;
            FingersScript.Instance.AddGesture(tap);
        }

        private void Update()
        {
            swipe.MinimumNumberOfTouchesToTrack = swipe.MaximumNumberOfTouchesToTrack = SwipeTouchCount;
            swipe.EndMode = SwipeMode;
        }

        private void Tap_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Debug.Log("Tap");
            }
        }

        private void Swipe_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            Debug.LogFormat("Swipe state: {0}", gesture.State);

            SwipeGestureRecognizer swipe = gesture as SwipeGestureRecognizer;
            if (swipe.State == GestureRecognizerState.Ended)
            {
                float angle = Mathf.Atan2(-swipe.DistanceY, swipe.DistanceX) * Mathf.Rad2Deg;
                SwipeParticleSystem.transform.rotation = Quaternion.Euler(angle, 90.0f, 0.0f);
                Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(gesture.StartFocusX, gesture.StartFocusY, 0.0f));
                pos.z = 0.0f;
                SwipeParticleSystem.transform.position = pos;
                SwipeParticleSystem.Play();
                Debug.LogFormat("Swipe dir: {0}", swipe.EndDirection);
            }
        }
    }
}