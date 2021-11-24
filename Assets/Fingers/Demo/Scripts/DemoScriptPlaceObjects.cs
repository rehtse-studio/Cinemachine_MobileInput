using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// Demo script showing how placed objects can reset another gesture to ensure only one gesture acts at a time
    /// </summary>
    public class DemoScriptPlaceObjects : MonoBehaviour
    {
        /// <summary>Where placed objects should start</summary>
        [Tooltip("Where placed objects should start")]
        public Transform PlaceObjectStart;

        /// <summary>The prefab of the object to place</summary>
        [Tooltip("The prefab of the object to place")]
        public GameObject PrefabToPlace;

        /// <summary>Pan orbit script</summary>
        [Tooltip("Pan orbit script")]
        public FingersPanOrbitComponentScript PanOrbitScript;

        /// <summary>
        /// Callback to place an object
        /// </summary>
        public void PlaceObject()
        {
            GameObject obj = GameObject.Instantiate(PrefabToPlace);
            if (obj != null)
            {
                obj.transform.position = PlaceObjectStart.transform.position;
                FingersPanRotateScaleComponentScript moveScript = obj.GetComponent<FingersPanRotateScaleComponentScript>();
                if (moveScript != null)
                {
                    moveScript.StateUpdated.AddListener(PlacedObjectGestureCallback);
                }
            }
        }

        /// <summary>
        /// Callback when a placed object is acted upon
        /// </summary>
        /// <param name="gesture">The gesture that is acting</param>
        public void PlacedObjectGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began || gesture.State == GestureRecognizerState.Executing)
            {
                // reset orbit script, any touches or gestures being processed will be dropped
                PanOrbitScript.PanGesture.Reset();
                PanOrbitScript.ScaleGesture.Reset();
            }
        }
    }
}
