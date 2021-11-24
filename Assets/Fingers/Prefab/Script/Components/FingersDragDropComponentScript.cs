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
    /// Allows a long tap and hold to move an object around and release it at a new point. Add this script to the object to drag.
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Drag and Drop", 0)]
    public class FingersDragDropComponentScript : MonoBehaviour
    {
        /// <summary>The camera to use to convert screen coordinates to world coordinates. Defaults to Camera.main.</summary>
        [Tooltip("The camera to use to convert screen coordinates to world coordinates. Defaults to Camera.main.")]
        public Camera[] Cameras;

        /// <summary>Whether to bring the object to the front when a gesture executes on it. Only used for 2D objects, ignored for 3D objects.</summary>
        [Tooltip("Whether to bring the object to the front when a gesture executes on it. Only used for 2D objects, ignored for 3D objects.")]
        public bool BringToFront = true;

        /// <summary>Scale to increase object by when a drag starts. When drags stops, scale is returned to normal.</summary>
        [Tooltip("Scale to increase object by when a drag starts. When drags stops, scale is returned to normal.")]
        [Range(1.0f, 1.5f)]
        public float DragScale = 1.1f;

        /// <summary>
        /// Long press gesture to start drag
        /// </summary>
        public LongPressGestureRecognizer LongPressGesture { get; private set; }

        private Rigidbody rigidBody;
        private Rigidbody2D rigidBody2D;
        private SpriteRenderer spriteRenderer;
        private int startSortOrder;
        private float panZ;
        private Vector3 panOffset;
        private RectTransform rt;
        private Canvas canvas;

        private Vector3 ScreenToWorldPoint(Camera camera, float x, float y, float z)
        {
            if (rt != null && canvas != null)
            {
                Vector3 canvasPoint = canvas.ScreenToCanvasPoint(new Vector2(x, y));
                return canvasPoint;
            }
            return camera.ScreenToWorldPoint(new Vector3(x, y, z));
        }

        private void LongPressGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            Camera camera;
            FingersScript.StartOrResetGesture(r, BringToFront, Cameras, gameObject, spriteRenderer, GestureRecognizerComponentScriptBase.GestureObjectMode.RequireIntersectWithGameObject, out camera);
            if (r.State == GestureRecognizerState.Began)
            {
                transform.localScale *= DragScale;
                panZ = camera.WorldToScreenPoint(transform.position).z;
                panOffset = transform.position - ScreenToWorldPoint(camera, r.FocusX, r.FocusY, panZ);
                if (DragStarted != null)
                {
                    DragStarted.Invoke(this, System.EventArgs.Empty);
                }
            }
            else if (r.State == GestureRecognizerState.Executing)
            {
                Vector3 gestureScreenPoint = new Vector3(r.FocusX, r.FocusY, panZ);
                Vector3 gestureWorldPoint = ScreenToWorldPoint(camera, gestureScreenPoint.x, gestureScreenPoint.y, gestureScreenPoint.z) + panOffset;
                if (rigidBody != null)
                {
                    rigidBody.MovePosition(gestureWorldPoint);
                }
                else if (rigidBody2D != null)
                {
                    rigidBody2D.MovePosition(gestureWorldPoint);
                }
                else
                {
                    transform.position = gestureWorldPoint;
                }
                if (DragUpdated != null)
                {
                    DragUpdated.Invoke(this, System.EventArgs.Empty);
                }
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                transform.localScale /= DragScale;
                if (spriteRenderer != null && BringToFront)
                {
                    spriteRenderer.sortingOrder = startSortOrder;
                }
                if (DragEnded != null)
                {
                    DragEnded.Invoke(this, System.EventArgs.Empty);
                }
            }
        }

        private void OnEnable()
        {
            if ((Cameras == null || Cameras.Length == 0) && Camera.main != null)
            {
                Cameras = new Camera[] { Camera.main };
            }
            LongPressGesture = new LongPressGestureRecognizer();
            LongPressGesture.StateUpdated += LongPressGestureUpdated;
            rigidBody = GetComponent<Rigidbody>();
            rigidBody2D = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            rt = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            if (spriteRenderer != null)
            {
                startSortOrder = spriteRenderer.sortingOrder;
            }
            FingersScript.Instance.AddGesture(LongPressGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(LongPressGesture);
            }
        }

        /// <summary>
        /// Fires when a drag starts
        /// </summary>
        public event System.EventHandler DragStarted;

        /// <summary>
        /// Fires when a drag updates
        /// </summary>
        public event System.EventHandler DragUpdated;

        /// <summary>
        /// Fires when a drag ends
        /// </summary>
        public event System.EventHandler DragEnded;
    }
}
