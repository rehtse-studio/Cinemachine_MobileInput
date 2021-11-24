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
    /// Platform specific view demo script
    /// </summary>
    public class DemoScriptPlatformSpecificView : MonoBehaviour
    {
        /// <summary>
        /// The left panel that will restrict one tap gesture to
        /// </summary>
        [Tooltip("The left panel that will restrict one tap gesture to")]
        public GameObject LeftPanel;

        /// <summary>
        /// The cube that will restrict one tap gesture to
        /// </summary>
        [Tooltip("The cube that will restrict one tap gesture to")]
        public GameObject Cube;

        private void Start()
        {
            // create a tap gesture that only executes on the left panel
            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.StateUpdated += Tap_Updated_Panel;
            tap.PlatformSpecificView = LeftPanel;
            FingersScript.Instance.AddGesture(tap);

            TapGestureRecognizer tap2 = new TapGestureRecognizer();
            tap2.StateUpdated += Tap_Updated_Cube;
            tap2.PlatformSpecificView = Cube;
            FingersScript.Instance.AddGesture(tap2);
        }

        private void Tap_Updated_Cube(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Debug.LogFormat("Tap gesture executed on cube at {0},{1}", gesture.FocusX, gesture.FocusY);
            }
        }

        private void Tap_Updated_Panel(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Debug.LogFormat("Tap gesture executed on panel at {0},{1}", gesture.FocusX, gesture.FocusY);
            }
        }

        private void Update()
        {

        }
    }
}
