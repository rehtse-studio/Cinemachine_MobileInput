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
    /// Demo script component, allows easily adding a gesture through Unity components
    /// </summary>
    public class DemoScriptComponent : MonoBehaviour
    {
        private float scale = 1.0f;
        private float oneTouchScale = 1.0f;

        private void Start()
        {
            FingersScript.Instance.ShowTouches = true;
        }

        /// <summary>
        /// Callback for tap gesture
        /// </summary>
        /// <param name="gesture">Tap gesture</param>
        public void TapGestureExecuted(DigitalRubyShared.GestureRecognizer gesture)
        {
            Debug.LogFormat("Tap gesture executing, state: {0}, pos: {1},{2}", gesture.State, gesture.FocusX, gesture.FocusY);
        }

        /// <summary>
        /// Callback for swipe gesture
        /// </summary>
        /// <param name="gesture">Swipe gesture</param>
        public void SwipeGestureExecuted(DigitalRubyShared.GestureRecognizer gesture)
        {
            Debug.LogFormat("Swipe gesture executing, state: {0}, dir: {1} pos: {2},{3}", gesture.State, (gesture as SwipeGestureRecognizer).EndDirection, gesture.FocusX, gesture.FocusY);
        }

        /// <summary>
        /// Callback for scale gesture
        /// </summary>
        /// <param name="gesture">Scale gesture</param>
        public void ScaleGestureExecuted(DigitalRubyShared.GestureRecognizer gesture)
        {
            scale *= (gesture as ScaleGestureRecognizer).ScaleMultiplier;
            Debug.LogFormat("Scale gesture executing, state: {0}, scale: {1} pos: {2},{3}", gesture.State, scale, gesture.FocusX, gesture.FocusY);
        }

        /// <summary>
        /// Callback for one touch scale gesture
        /// </summary>
        /// <param name="gesture">One touch scale gesture</param>
        public void OneTouchScaleGestureExecuted(DigitalRubyShared.GestureRecognizer gesture)
        {
            oneTouchScale *= (gesture as OneTouchScaleGestureRecognizer).ScaleMultiplier;
            Debug.LogFormat("Scale gesture executing, state: {0}, scale: {1} pos: {2},{3}", gesture.State, oneTouchScale, gesture.FocusX, gesture.FocusY);
        }

        /// <summary>
        /// Callback for rotate gesture
        /// </summary>
        /// <param name="gesture">Rotate gesture</param>
        public void RotateGestureExecuted(DigitalRubyShared.GestureRecognizer gesture)
        {
            Debug.LogFormat("Rotate gesture executing, state: {0}, degrees: {1} pos: {2},{3}", gesture.State, (gesture as RotateGestureRecognizer).RotationDegrees, gesture.FocusX, gesture.FocusY);
        }

        /// <summary>
        /// Callback for pan gesture
        /// </summary>
        /// <param name="gesture">Pan gesture</param>
        public void PanGestureExecuted(DigitalRubyShared.GestureRecognizer gesture)
        {
            Debug.LogFormat("Pan gesture executing, state: {0}, pos: {1},{2}", gesture.State, gesture.FocusX, gesture.FocusY);
        }

        /// <summary>
        /// Callback for long press gesture
        /// </summary>
        /// <param name="gesture">Long press gesture</param>
        public void LongPressGestureExecuted(DigitalRubyShared.GestureRecognizer gesture)
        {
            Debug.LogFormat("Long press gesture executing, state: {0}, pos: {1},{2}", gesture.State, gesture.FocusX, gesture.FocusY);
        }

        /// <summary>
        /// Callback for image gesture
        /// </summary>
        /// <param name="gesture">Image gesture</param>
        public void ImageGestureExecuted(DigitalRubyShared.GestureRecognizer gesture)
        {
            ImageGestureRecognizer imgGesture = gesture as ImageGestureRecognizer;
            if (gesture.State == GestureRecognizerState.Ended)
            {
                if (imgGesture.MatchedGestureImage == null)
                {
                    Debug.Log("Image gesture failed to match.");
                }
                else
                {
                    Debug.Log("Image gesture matched!");
                }
                gesture.Reset();
            }
        }
    }
}
