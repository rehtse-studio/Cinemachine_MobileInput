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
    /// Allows zooming and panning a camera using pan and scale gestures. A tap gesture is also included to force the camera to look at the tapped object.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Fingers Gestures/Component/Fingers Zoom Pan Camera", 5)]
    public class FingersZoomPanCameraComponentScript : MonoBehaviour
    {
        /// <summary>Require this area to be visible at all times</summary>
        [Tooltip("Require this area to be visible at all times")]
        public Collider VisibleArea;

        /// <summary>Dampening for velocity when pan is released, lower values reduce velocity faster.</summary>
        [Tooltip("Dampening for velocity when pan is released, lower values reduce velocity faster.")]
        [Range(0.0f, 1.0f)]
        public float Dampening = 0.8f;

        /// <summary>Adjust speed of rotation gesture (two finger rotate). Set to 0 for no rotation allowed.</summary>
        [Tooltip("Adjust speed of rotation gesture (two finger rotate). Set to 0 for no rotation allowed.")]
        [Range(-10.0f, 10.0f)]
        public float RotationSpeed = 0.0f;

        /// <summary>The threshold scale gesture must change in units before executing</summary>
        [Tooltip("The threshold scale gesture must change in units before executing")]
        [Range(0.0f, 1.0f)]
        public float ScaleThreshold = 0.15f;

        /// <summary>The layers that can be tapped on for objects to center the camera on them</summary>
        [Tooltip("The layers that can be tapped on for objects to center the camera on them")]
        public LayerMask TapToCenterLayerMask = -1;

        /// <summary>
        /// Zoom in and out gesture
        /// </summary>
        public ScaleGestureRecognizer ScaleGesture { get; private set; }

        /// <summary>
        /// Move camera gesture
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// Tap gesture to have camera look at tapped object
        /// </summary>
        public TapGestureRecognizer TapGesture { get; private set; }

        /// <summary>
        /// Allows rotating camera around it's forward vector
        /// </summary>
        public RotateGestureRecognizer RotateGesture { get; private set; }

        private Vector3 cameraAnimationTargetPosition;
        private Vector3 velocity;
        private Camera _camera;

        private IEnumerator AnimationCoRoutine()
        {
            Vector3 start = transform.position;

            // animate over 1/2 second
            for (float accumTime = Time.deltaTime; accumTime <= 0.5f; accumTime += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(start, cameraAnimationTargetPosition, accumTime / 0.5f);
                yield return null;
            }
        }

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            if (GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>() == null)
            {
                gameObject.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>();
            }
            if (GetComponent<UnityEngine.EventSystems.Physics2DRaycaster>() == null)
            {
                gameObject.AddComponent<UnityEngine.EventSystems.Physics2DRaycaster>();
            }

            ScaleGesture = new ScaleGestureRecognizer
            {
                ThresholdUnits = ScaleThreshold,
                ZoomSpeed = 6.0f // for a touch screen you'd probably not do this, but if you are using ctrl + mouse wheel then this helps zoom faster
            };
            ScaleGesture.StateUpdated += Gesture_Updated;

            PanGesture = new PanGestureRecognizer();
            PanGesture.StateUpdated += PanGesture_Updated;

            // the scale and pan can happen together
            ScaleGesture.AllowSimultaneousExecution(PanGesture);

            TapGesture = new TapGestureRecognizer();
            TapGesture.StateUpdated += TapGesture_Updated;

            RotateGesture = new RotateGestureRecognizer();
            RotateGesture.StateUpdated += RotateGesture_Updated;
            RotateGesture.AllowSimultaneousExecution(PanGesture);
            RotateGesture.AllowSimultaneousExecution(ScaleGesture);

            FingersScript.Instance.AddGesture(ScaleGesture);
            FingersScript.Instance.AddGesture(PanGesture);
            FingersScript.Instance.AddGesture(TapGesture);
            FingersScript.Instance.AddGesture(RotateGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(ScaleGesture);
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(TapGesture);
                FingersScript.Instance.RemoveGesture(RotateGesture);
            }
        }

        private void LateUpdate()
        {
            if (VisibleArea == null)
            {
                return;
            }

            Bounds visibleAreaBounds = VisibleArea.bounds;
            Vector3 world1 = _camera.ViewportToWorldPoint(Vector3.zero);
            Vector3 world2 = _camera.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, 0.0f));
            Vector3 pos = _camera.transform.position;
            world1.z = pos.z;
            world2.z = _camera.farClipPlane;
            Bounds cameraVisibleArea = new Bounds { min = world1, max = world2 };

            if (_camera.orthographic)
            {
                // move the camera so that the visible area is visible, if necessary
                float leftOverlap = Mathf.Min(0.0f, visibleAreaBounds.max.x - cameraVisibleArea.min.x);
                float rightOverlap = Mathf.Max(0.0f, visibleAreaBounds.min.x - cameraVisibleArea.max.x);
                float topOverlap = Mathf.Min(0.0f, visibleAreaBounds.max.y - cameraVisibleArea.min.y);
                float bottomOverlap = Mathf.Max(0.0f, visibleAreaBounds.min.y - cameraVisibleArea.max.y);
                if (leftOverlap < 0.0f)
                {
                    pos.x += leftOverlap;
                }
                else if (rightOverlap > 0.0f)
                {
                    pos.x += rightOverlap;
                }
                if (topOverlap < 0.0f)
                {
                    pos.y += topOverlap;
                }
                else if (bottomOverlap > 0.0f)
                {
                    pos.y += bottomOverlap;
                }
            }
            else
            {
                // TODO: Implement perspective camera keeping oject in view
            }

            transform.position = pos + (velocity * Time.deltaTime);
            velocity *= Dampening;
        }

        private void TapGesture_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (TapGesture.State != GestureRecognizerState.Ended)
            {
                return;
            }

            Ray ray = _camera.ScreenPointToRay(new Vector3(TapGesture.FocusX, TapGesture.FocusY, 0.0f));
            RaycastHit hit;
            if (Physics.Raycast(ray,  out hit, float.MaxValue, TapToCenterLayerMask))
            {
                // adjust camera x, y to look at the tapped / clicked sphere
                cameraAnimationTargetPosition = new Vector3(hit.transform.position.x, hit.transform.position.y, _camera.transform.position.z);
                StopAllCoroutines();
                StartCoroutine(AnimationCoRoutine());
            }
        }

        private void PanGesture_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (PanGesture.State == GestureRecognizerState.Executing)
            {
                StopAllCoroutines();

                // convert pan coordinates to world coordinates
                // get z position, orthographic this is 0, otherwise it's the z coordinate of all the spheres
                float z = (_camera.orthographic ? 0.0f : 10.0f);
                Vector3 pan = new Vector3(PanGesture.DeltaX, PanGesture.DeltaY, z);
                Vector3 zero = _camera.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, z));
                Vector3 panFromZero = _camera.ScreenToWorldPoint(pan);
                Vector3 panInWorldSpace = zero - panFromZero;
                _camera.transform.Translate(panInWorldSpace);
            }
            else if (PanGesture.State == GestureRecognizerState.Ended)
            {
                float z = (_camera.orthographic ? 0.0f : 10.0f);
                Vector3 zero = _camera.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, z));
                Vector3 one = _camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, z));
                float worldWidth = one.x - zero.x;
                float worldHeight = one.y - zero.y;
                float worldWidthRatio = Screen.width / worldWidth;
                float worldHeightRatio = Screen.height / worldHeight;
                float velocityX = PanGesture.VelocityX / -worldWidthRatio;
                float velocityY = PanGesture.VelocityY / -worldHeightRatio;
                velocity = new Vector3(velocityX, velocityY, 0.0f);
            }
        }

        private void RotateGesture_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (RotationSpeed != 0.0f && gesture.State == GestureRecognizerState.Executing)
            {
                _camera.transform.Rotate(_camera.transform.forward, RotateGesture.RotationDegreesDelta, Space.World);
            }
        }

        private void Gesture_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (ScaleGesture.State != GestureRecognizerState.Executing || ScaleGesture.ScaleMultiplier == 1.0f)
            {
                return;
            }

            // invert the scale so that smaller scales actually zoom out and larger scales zoom in
            float scale = 1.0f + (1.0f - ScaleGesture.ScaleMultiplier);

            if (_camera.orthographic)
            {
                float newOrthographicSize = Mathf.Clamp(_camera.orthographicSize * scale, 1.0f, 100.0f);
                _camera.orthographicSize = newOrthographicSize;
            }
            else
            {
                // get camera look vector
                Vector3 forward = _camera.transform.forward;

                // set the target to the camera x,y and 0 z position
                Vector3 target = transform.position;
                target.z = 0.0f;

                // get distance between camera target and camera position
                float distance = Vector3.Distance(target, transform.position);

                // come up with a new distance based on the scale gesture
                float newDistance = Mathf.Clamp(distance * scale, 1.0f, 100.0f);

                // set the camera position at the new distance
                transform.position = target - (forward * newDistance);
            }
        }
    }
}
