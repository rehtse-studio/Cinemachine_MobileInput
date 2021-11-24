using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// This demo script shows how to implement virtual touches. Mapping AR, VR or any other input system to fingers is super easy.
    /// Please look over all the TODO: in this script.
    /// </summary>
    public class DemoScriptVirtualTouches : MonoBehaviour
    {
        /// <summary>
        /// TODO: Remove this for your own script
        /// </summary>
        private const int mouseTouchId = -999;

        /// <summary>
        /// This dictionary contains the list of touches that we are tracking, using id to gesture lookup.
        /// For the mouse we are only tracking the left pointer and using id -999 to track it, since
        /// Unity input touches have positive id values.
        /// </summary>
        private readonly Dictionary<int, GestureTouch> virtualTouches = new Dictionary<int, GestureTouch>();

        private readonly List<int> idsToRemove = new List<int>();

        private void AddTouch(int id, Vector2 position, UnityEngine.TouchPhase phase, float pressure = 1.0f)
        {
            GestureTouch touch = FingersScript.Instance.GestureTouchFromVirtualTouch(id, position, phase, pressure);
            virtualTouches[id] = touch;
        }

        private void CreateGestures()
        {
            // TODO: Replace the code in this method with your own gestures or implement your own gestures in another script
            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.StateUpdated += (gesture) =>
            {
                if (gesture.State == GestureRecognizerState.Ended)
                {
                    Debug.LogFormat("Tap detected at position {0},{1}", gesture.FocusX, gesture.FocusY);
                }
            };
            FingersScript.Instance.AddGesture(tap);

            PanGestureRecognizer pan = new PanGestureRecognizer();
            pan.StateUpdated += (gesture) =>
            {
                if (gesture.State == GestureRecognizerState.Began)
                {
                    Debug.LogFormat("Pan started at position {0},{1}", gesture.FocusX, gesture.FocusY);
                }
                else if (gesture.State == GestureRecognizerState.Executing)
                {
                    Debug.LogFormat("Pan executing at position {0},{1}", gesture.FocusX, gesture.FocusY);
                }
                else if (gesture.State == GestureRecognizerState.Ended)
                {
                    Debug.LogFormat("Pan ended at position {0},{1}", gesture.FocusX, gesture.FocusY);
                }
            };
            FingersScript.Instance.AddGesture(pan);
        }

        private void ProcessMouse()
        {
            // TODO: Remove this method for your own script
            if (FingersScript.Instance.IsMouseDownThisFrame(0))
            {
                AddTouch(mouseTouchId, Input.mousePosition, UnityEngine.TouchPhase.Began);
            }
            else if (FingersScript.Instance.IsMouseUpThisFrame(0))
            {
                AddTouch(mouseTouchId, Input.mousePosition, UnityEngine.TouchPhase.Ended);
            }
            else if (FingersScript.Instance.IsMouseDown(0))
            {
                AddTouch(mouseTouchId, Input.mousePosition, UnityEngine.TouchPhase.Moved);
            }
        }

        private void ProcessTouches()
        {
            // TODO: Remove this method for your own script
            foreach (Touch touch in Input.touches)
            {
                AddTouch(touch.fingerId, touch.position, touch.phase, touch.pressure);
            }
        }

        private void RemoveExpiredTouches()
        {
            // It is critical that you remove any of your touches that have entered the Ended phase
            // This should be done in the VirtualTouchCountHandler callback on FingersScript.Instance.
            foreach (GestureTouch touch in virtualTouches.Values)
            {
                if (touch.TouchPhase == TouchPhase.Ended || touch.TouchPhase == TouchPhase.Cancelled || touch.TouchPhase == TouchPhase.Unknown)
                {
                    idsToRemove.Add(touch.Id);
                }
            }
            foreach (int id in idsToRemove)
            {
                virtualTouches.Remove(id);
            }
            idsToRemove.Clear();
        }

        private int GetVirtualTouchCount()
        {
            // Note: It is important to perform your virtual touch gesture updates in this method instead of the script Update
            //  method, as this code needs to execute before the fingers script Update method. Unity script Update order is
            //  undefined by default, so putting the update code in here ensures the correct Update order.

            // TODO: Remove the ProcessMouse and ProcessTouches methods, you don't need them for your own virtual touches
            ProcessMouse();
            ProcessTouches();

            // TODO: Implement this method for your own virtual touches. For AR / VR, a lot of systems will return positions in range of 0 to 1,
            //  or -1 to 1. In this case, you should map them to a virtual screen size, something like 1920x1080 or equivelant for your aspect ratio.
            // Example mapping function that turns 0 to 1 into 1920x1080:
            // Vector2 mappedPosition = new Vector2(arTouch.x * 1920.0f, arTouch.y * 1080.0f);
            // Example mapping function that turns -1 to 1 into 1920x1080:
            // Vector2 mappedPosition = new Vector2((arTouch.x + 1.0f) * 960.0f, (arTouch.y + 1.0f) * 540.0f);
            ProcessVirtualTouches();

            return virtualTouches.Count;
        }

        private void ProcessVirtualTouches()
        {
            // TODO: Insert your own code here to process your virtual touches, similar to how the mouse and input touches are done.

            // something like this (virtualTouchManager is something you must create):
            /*foreach (Touch touch in virtualTouchManager.touches)
            {
                AddTouch(touch.id, new Vector2(touch.position.x * 1920.0f, touch.position.y * 1080.0f), touch.phase, touch.pressure);
            }*/
        }

        private void OnEnable()
        {
            CreateGestures();

            // setup the virtual touch handlers, this is required to link fingers script to your virtual touches
            FingersScript.Instance.VirtualTouchCountHandler = GetVirtualTouchCount;
            FingersScript.Instance.VirtualTouchObjectHandler = (index) => virtualTouches.ElementAt(index).Value;
            FingersScript.Instance.VirtualTouchUpdateHandler = () => RemoveExpiredTouches();
            FingersScript.Instance.VirtualTouchResetHandler = () => virtualTouches.Clear();
        }
    }
}
