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
    /// Multiple finger at once tap demo script
    /// </summary>
    public class DemoScriptMultiFingerTap : MonoBehaviour
    {
        /// <summary>
        /// Status text
        /// </summary>
        public UnityEngine.UI.Text statusText;

        private void Start()
        {
            TapGestureRecognizer oneFingerTap = new TapGestureRecognizer();
            oneFingerTap.StateUpdated += TapCallback;
            TapGestureRecognizer twoFingerTap = new TapGestureRecognizer();
            twoFingerTap.MinimumNumberOfTouchesToTrack = twoFingerTap.MaximumNumberOfTouchesToTrack = 2;
            twoFingerTap.StateUpdated += TapCallback;
            TapGestureRecognizer threeFingerTap = new TapGestureRecognizer();
            threeFingerTap.MinimumNumberOfTouchesToTrack = threeFingerTap.MaximumNumberOfTouchesToTrack = 3;
            threeFingerTap.StateUpdated += TapCallback;
            FingersScript.Instance.AddGesture(oneFingerTap);
            FingersScript.Instance.AddGesture(twoFingerTap);
            FingersScript.Instance.AddGesture(threeFingerTap);
            oneFingerTap.RequireGestureRecognizerToFail = twoFingerTap;
            twoFingerTap.RequireGestureRecognizerToFail = threeFingerTap;
            FingersScript.Instance.ShowTouches = true;
        }

        private void TapCallback(DigitalRubyShared.GestureRecognizer tapGesture)
        {
            if (tapGesture.State == GestureRecognizerState.Ended)
            {
                statusText.text = string.Format("Tap gesture finished, touch count: {0}", (tapGesture as TapGestureRecognizer).TapTouches.Count);
                Debug.Log(statusText.text);
            }
        }
    }
}
