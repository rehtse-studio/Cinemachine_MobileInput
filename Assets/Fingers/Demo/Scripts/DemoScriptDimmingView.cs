using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// Allows hiding a dimming view
    /// </summary>
    public class DemoScriptDimmingView : MonoBehaviour
    {
        /// <summary>
        /// The content view to hide on tap
        /// </summary>
        public GameObject ContentView;

        private void OnEnable()
        {
            // change default behavior, images should block gestures unless they are the gesture view for the gesture
            FingersScript.Instance.ComponentTypesToDenyPassThrough.Add(typeof(UnityEngine.UI.Image));
        }

        /// <summary>
        /// Tap gesture callback, hides the content view
        /// </summary>
        /// <param name="tapGesture">Tap gesture</param>
        public void TapGestureUpdated(GestureRecognizer tapGesture)
        {
            if (tapGesture.State == GestureRecognizerState.Ended)
            {
                ContentView.SetActive(false);
            }
        }
    }
}
