//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DigitalRubyShared
{
    /// <summary>
    /// A pan gesture detects movement of a touch
    /// </summary>
    public class PanGestureRecognizer : DigitalRubyShared.GestureRecognizer
    {
        private bool needsDistanceThreshold;
        private float startX;
        private float startY;
        private readonly Stopwatch timeBelowSpeedUnitsToRestartThresholdUnits = new Stopwatch();

        private void ProcessTouches(bool resetFocus)
        {
            bool firstFocus = CalculateFocus(CurrentTrackedTouches, resetFocus);

            if (firstFocus)
            {
                timeBelowSpeedUnitsToRestartThresholdUnits.Reset();
                timeBelowSpeedUnitsToRestartThresholdUnits.Start();
                if (ThresholdUnits <= 0.0f)
                {
                    // we can start right away, no minimum move threshold
                    needsDistanceThreshold = false;
                    SetState(GestureRecognizerState.Began);
                }
                else
                {
                    needsDistanceThreshold = true;
                    SetState(GestureRecognizerState.Possible);
                }
                startX = FocusX;
                startY = FocusY;
            }
            else if (!needsDistanceThreshold && (State == GestureRecognizerState.Began || State == GestureRecognizerState.Executing))
            {
                if (SpeedUnitsToRestartThresholdUnits > 0.0f && Distance(VelocityX, VelocityY) < SpeedUnitsToRestartThresholdUnits &&
                    (float)timeBelowSpeedUnitsToRestartThresholdUnits.Elapsed.TotalSeconds >= TimeToRestartThresholdUnits)
                {
                    if (!needsDistanceThreshold)
                    {
                        needsDistanceThreshold = true;
                        startX = FocusX;
                        startY = FocusY;
                    }
                }
                else
                {
                    timeBelowSpeedUnitsToRestartThresholdUnits.Reset();
                    timeBelowSpeedUnitsToRestartThresholdUnits.Start();
                    SetState(GestureRecognizerState.Executing);
                }
            }
            else if (TrackedTouchCountIsWithinRange)
            {
                if (needsDistanceThreshold)
                {
                    float distance = Distance(FocusX - startX, FocusY - startY);
                    if (distance >= ThresholdUnits)
                    {
                        needsDistanceThreshold = false;
                        SetState(GestureRecognizerState.Began);
                    }
                    else if (State != GestureRecognizerState.Executing)
                    {
                        SetState(GestureRecognizerState.Possible);
                    }
                }
                else
                {
                    SetState(GestureRecognizerState.Possible);
                }
            }
        }

        /// <summary>
        /// TouchesBegan
        /// </summary>
        /// <param name="touches"></param>
        protected override void TouchesBegan(System.Collections.Generic.IEnumerable<GestureTouch> touches)
        {
            ProcessTouches(true);
        }

        /// <summary>
        /// TouchesMoved
        /// </summary>
        protected override void TouchesMoved()
        {
            ProcessTouches(false);
        }

        /// <summary>
        /// TouchesEnded
        /// </summary>
        protected override void TouchesEnded()
        {
            ProcessTouches(false);
            if (State == GestureRecognizerState.Possible)
            {
                // didn't move far enough to start a pan, fail the gesture
                SetState(GestureRecognizerState.Failed);
            }
            else
            {
                SetState(GestureRecognizerState.Ended);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PanGestureRecognizer()
        {
            ThresholdUnits = 0.2f;
        }

        /// <summary>
        /// How many units away the pan must move to execute - default is 0.2
        /// </summary>
        /// <value>The threshold in units</value>
        public float ThresholdUnits { get; set; }

        /// <summary>
        /// The speed in units per second which if the pan gesture drops under, ThresholdUnits will be re-enabled and
        /// the gesture will not send execution events until the threshold units is exceeded again.
        /// Both SpeedUnitsToRestartThresholdUnits and TimeToRestartThresholdUnits conditions must be met to re-enable ThresholdUnits.
        /// 0.1 is a good value.
        /// </summary>
        public float SpeedUnitsToRestartThresholdUnits { get; set; }

        /// <summary>
        /// The number of seconds that speed must be below SpeedUnitsToRestartThresholdUnits in order to re-enable ThresholdUnits.
        /// Set to 0 for immediately re-enabling ThresholdUnits if the gesture speed drops below SpeedUnitsToRestartThresholdUnits.
        /// Both SpeedUnitsToRestartThresholdUnits and TimeToRestartThresholdUnits conditions must be met to re-enable ThresholdUnits.
        /// 0.2 is a good value.
        /// </summary>
        public float TimeToRestartThresholdUnits { get; set; }
    }
}

