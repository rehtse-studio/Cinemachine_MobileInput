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
    /// Allows dragging multiple objects with just one gesture
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Multi Drag", 1)]
    public class FingersMultiDragComponentScript : MonoBehaviour
    {
        private class DragState
        {
            public GameObject GameObject;
            public Vector3 Offset;
        }

        /// <summary>Whether to drag on x axis</summary>
        [Tooltip("Whether to drag on x axis")]
        public bool DragX = true;

        /// <summary>Whether to drag on y axis</summary>
        [Tooltip("Whether to drag on y axis")]
        public bool DragY = true;

        /// <summary>The tag of the object(s) to drag - if the tag contains this, the object can drag</summary>
        [Tooltip("The tag of the object(s) to drag - if the tag contains this, the object can drag")]
        public string TagToDrag;

        /// <summary>The name of the object(s) to drag - if the name contains this, the object can drag</summary>
        [Tooltip("The name of the object(s) to drag - if the name contains this, the object can drag")]
        public string NameToDrag;

        // game object to drag state
        private readonly Dictionary<GameObject, DragState> draggingObjectsByGameObject = new Dictionary<GameObject, DragState>();

        // touch id to drag state
        private readonly Dictionary<int, DragState> draggingObjectsByTouchId = new Dictionary<int, DragState>();

        // set of all touches
        private readonly HashSet<int> allTouchIds = new HashSet<int>();

        // temp list of touch ids to remove
        private readonly List<int> currentTouchIds = new List<int>();

        /// <summary>
        /// Pan gesture to drag objects with
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        private bool CanDrag(GameObject obj)
        {
            return (string.IsNullOrEmpty(NameToDrag) && string.IsNullOrEmpty(TagToDrag)) ||
                (!string.IsNullOrEmpty(NameToDrag) && obj.name.Contains(NameToDrag)) ||
                (!string.IsNullOrEmpty(TagToDrag) && obj.tag.Contains(TagToDrag));
        }

        private void OnEnable()
        {
            PanGesture = new PanGestureRecognizer { MinimumNumberOfTouchesToTrack = 1, MaximumNumberOfTouchesToTrack = 32, ThresholdUnits = 0.0f };
            PanGesture.StateUpdated += Pan_StateUpdated;
            FingersScript.Instance.AddGesture(PanGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
            }
        }

        private void StartDrag(GestureTouch touch)
        {
            // check if we hit something, if so start dragging it otherwise reset the gesture
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.X, touch.Y, 0.0f));
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.5f);
            foreach (Collider2D hit in hits)
            {
                if (CanDrag(hit.gameObject))
                {
                    // store the drag state along with an offset to reposition relative to the touch point and object center
                    DragState state = new DragState
                    {
                        GameObject = hit.gameObject,
                        Offset = (hit.transform.position - worldPos)
                    };
                    draggingObjectsByGameObject[hit.gameObject] = state;
                    draggingObjectsByTouchId[touch.Id] = state;
                    break;
                }
            }
        }

        private void UpdateDragStates()
        {
            DragState state;
            currentTouchIds.AddRange(allTouchIds);
            foreach (GestureTouch touch in PanGesture.CurrentTrackedTouches)
            {
                if (draggingObjectsByTouchId.TryGetValue(touch.Id, out state))
                {
                    // position the object relative to the touch location and offset
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.X, touch.Y, 0.0f));
                    Vector3 newPos = (worldPos + state.Offset);
                    newPos.x = (DragX ? newPos.x : state.GameObject.transform.position.x);
                    newPos.y = (DragY ? newPos.y : state.GameObject.transform.position.y);
                    state.GameObject.transform.position = newPos;
                }
                else if (!allTouchIds.Contains(touch.Id))
                {
                    allTouchIds.Add(touch.Id);
                    StartDrag(touch);
                }
                if (PanGesture.State != GestureRecognizerState.Ended)
                {
                    currentTouchIds.Remove(touch.Id);
                }
            }

            // remove touches that are no longer in the gesture
            foreach (int touchId in currentTouchIds)
            {
                allTouchIds.Remove(touchId);
                if (draggingObjectsByTouchId.TryGetValue(touchId, out state))
                {
                    draggingObjectsByTouchId.Remove(touchId);
                    draggingObjectsByGameObject.Remove(state.GameObject);
                }
            }
            currentTouchIds.Clear();
        }

        private void Pan_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            switch (gesture.State)
            {
                case GestureRecognizerState.Began:
                case GestureRecognizerState.Executing:
                case GestureRecognizerState.Ended:
                    UpdateDragStates();
                    break;
            }
        }
    }
}
