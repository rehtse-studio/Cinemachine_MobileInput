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
using UnityEngine.UI;

namespace DigitalRubyShared
{
    /// <summary>
    /// Tap and double tap demo script
    /// </summary>
    public class DemoScriptTapAndDoubleTap : MonoBehaviour
    {
        /// <summary>
        /// Whether to enable double tap
        /// </summary>
        [Tooltip("Whether to enable double tap")]
        public bool EnableDoubleTap = true;

        /// <summary>
        /// Status text
        /// </summary>
        [Tooltip("Status text")]
        public Text StatusText;

        private TapGestureRecognizer tapGesture;
        private TapGestureRecognizer doubleTapGesture;

        private void Start()
        {
            tapGesture = new TapGestureRecognizer { MaximumNumberOfTouchesToTrack = 10 };
            tapGesture.StateUpdated += TapGesture_StateUpdated;
            FingersScript.Instance.AddGesture(tapGesture);

            if (EnableDoubleTap)
            {
                doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
                doubleTapGesture.StateUpdated += DoubleTapGesture_StateUpdated;
                tapGesture.RequireGestureRecognizerToFail = doubleTapGesture;
                FingersScript.Instance.AddGesture(doubleTapGesture);
            }
        }

        private void TapGesture_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            Debug.LogFormat("Single tap state: {0}", gesture.State);
            if (gesture.State == GestureRecognizerState.Ended)
            {
                string msg = string.Format("Single tap at {0},{1}", gesture.FocusX, gesture.FocusY);
                StatusText.text = msg;
                Debug.Log(msg);
            }
        }

        private void DoubleTapGesture_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            Debug.LogFormat("Double tap state: {0}", gesture.State);
            if (gesture.State == GestureRecognizerState.Ended)
            {
                string msg = string.Format("Double tap at {0},{1}", gesture.FocusX, gesture.FocusY);
                StatusText.text = msg;
                Debug.Log(msg);
            }
        }

        private void Update()
        {
            if (FingersScript.Instance.IsKeyDownThisFrame(KeyCode.D))
            {
                tapGesture.Enabled = !tapGesture.Enabled;
                Debug.Log("Tap gesture enabled: " + tapGesture.Enabled);
            }
        }
    }
}
