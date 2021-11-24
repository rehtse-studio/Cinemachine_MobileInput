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
    /// Pan demo script
    /// </summary>
    public class DemoScriptPan : MonoBehaviour
    {
        /// <summary>
        /// Scroll view for log
        /// </summary>
        [Tooltip("Scroll view for log")]
        public UnityEngine.UI.ScrollRect logView;

        /// <summary>
        /// The min speed before re-enabling threshold units on the pan gesture
        /// </summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("The min speed before re-enabling threshold units on the pan gesture")]
        public float MinimumSpeedBeforeThresholdUnitsIsReEnabled;

        /// <summary>
        /// The min time before re-enabling threshold units on the pan gesture
        /// </summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("The min time before re-enabling threshold units on the pan gesture")]
        public float MinimumTimeBeforeThresholdUnitsIsEnabled;

        private PanGestureRecognizer pan;

        private void Log(string text, params object[] args)
        {
            string logText = string.Format(text, args);
            Debug.Log(logText);
        }

        private void Start()
        {
             pan = new PanGestureRecognizer();
            pan.StateUpdated += Pan_Updated;
            pan.MaximumNumberOfTouchesToTrack = 2;
            FingersScript.Instance.AddGesture(pan);

            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.StateUpdated += Tap_StateUpdated;
            tap.ClearTrackedTouchesOnEndOrFail = true;
            tap.AllowSimultaneousExecution(pan);
            FingersScript.Instance.AddGesture(tap);
        }

        private void Update()
        {
            pan.SpeedUnitsToRestartThresholdUnits = MinimumSpeedBeforeThresholdUnitsIsReEnabled;
            pan.TimeToRestartThresholdUnits = MinimumTimeBeforeThresholdUnitsIsEnabled;
        }

        private void Tap_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Log("Tap gesture, state: {0}, position: {1},{2}", gesture.State, gesture.FocusX, gesture.FocusY);
            }
        }

        private void Pan_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.CurrentTrackedTouches.Count != 0)
            {
                GestureTouch t = gesture.CurrentTrackedTouches[0];
                Log("Pan gesture, state: {0}, position: {1},{2} -> {3},{4}", gesture.State, t.PreviousX, t.PreviousY, t.X, t.Y);
            }
        }
    }
}