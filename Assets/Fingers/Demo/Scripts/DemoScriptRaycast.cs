//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// Demo raycast script, checks a tap and sees which object was tapped
    /// </summary>
    public class DemoScriptRaycast : MonoBehaviour
    {
        /// <summary>
        /// Asteroids
        /// </summary>
        [Tooltip("Asteroids")]
        public GameObject[] Asteroids;

        /// <summary>
        /// Non-asteroids
        /// </summary>
        [Tooltip("Non-asteroids")]
        public GameObject[] OtherObjects;

        private TapGestureRecognizer[] TapGestures;

        private void CreateGesture(ref int index, GameObject obj)
        {
            TapGestures[index] = new TapGestureRecognizer
            {
                PlatformSpecificView = obj
            };
            TapGestures[index].StateUpdated += TapGestureUpdated;
            FingersScript.Instance.AddGesture(TapGestures[index]);
            index++;
        }

        private void Start()
        {
            TapGestures = new TapGestureRecognizer[Asteroids.Length + OtherObjects.Length];
            int index = 0;
            foreach (GameObject obj in Asteroids)
            {
                CreateGesture(ref index, obj);
            }
            foreach (GameObject obj in OtherObjects)
            {
                CreateGesture(ref index, obj);
            }
        }

        private void TapGestureUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Debug.Log("Tapped on " + gesture.PlatformSpecificView);

                List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.PointerEventData eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                eventData.position = new Vector2(gesture.FocusX, gesture.FocusY);
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);

                Debug.Log("Event system raycast count: " + results.Count + ", objects: " + string.Join(",", results.Select(r => r.gameObject.name).ToArray()));
            }
        }
    }
}
