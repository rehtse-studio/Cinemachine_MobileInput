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
    /// Allows moving a camera in 3D space using pan, tilt, rotate and zoom mechanics
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Camera Move 3D", 4)]
    public class FingersCameraMove3DComponentScript : MonoBehaviour
    {
        /// <summary>The transform to move, defaults to the transform on this script</summary>
        [Tooltip("The transform to move, defaults to the transform on this script")]
        public Transform Target;

        /// <summary>Controls pan (left/right for strafe, up/down for forward/back) speed in number of world units per screen units panned</summary>
        [Range(-10.0f, 10.0f)]
        [Tooltip("Controls pan (left/right for strafe, up/down for forward/back) speed in number of world units per screen units panned")]
        public float PanSpeed = -1.0f;

        /// <summary>Controls tilt with two finger pan up or down</summary>
        [Range(-10.0f, 10.0f)]
        [Tooltip("Controls tilt with two finger pan up or down")]
        public float TiltSpeed = -1.0f;

        /// <summary>Threshold (in units) tilt gesture must move in order to execute</summary>
        [Tooltip("Threshold (in units) tilt gesture must move in order to execute")]
        public float TiltThreadhold = 0.5f;

        /// <summary>Controls zoom in/out speed</summary>
        [Range(-10.0f, 10.0f)]
        [Tooltip("Controls zoom in/out speed")]
        public float ZoomSpeed = 1.0f;

        /// <summary>Controls rotation speed</summary>
        [Range(-10.0f, 10.0f)]
        [Tooltip("Controls rotation speed")]
        public float RotateSpeed = 1.0f;

        /// <summary>How much to dampen movement, lower values dampen faster</summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("How much to dampen movement, lower values dampen faster")]
        public float Dampening = 0.95f;

        /// <summary>
        /// The pan gesture (left/right)
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// The tilt gesture (up/down)
        /// </summary>
        public PanGestureRecognizer TiltGesture { get; private set; }

        /// <summary>
        /// The scale gesture (zoom in/out)
        /// </summary>
        public ScaleGestureRecognizer ScaleGesture { get; private set; }

        /// <summary>
        /// The rotate gesture (spin in place)
        /// </summary>
        public RotateGestureRecognizer RotateGesture { get; private set; }

        private Vector3 moveVelocity;
        private float tiltVelocity;
        private float angularVelocity;
        private Vector3 zoomVelocity;

        private void PanGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                Quaternion q = Target.rotation;
                q = Quaternion.Euler(0.0f, q.eulerAngles.y, 0.0f);
                moveVelocity += (q * Vector3.right * DeviceInfo.PixelsToUnits(gesture.DeltaX) * Time.deltaTime * PanSpeed * 500.0f);
                moveVelocity += (q * Vector3.forward * DeviceInfo.PixelsToUnits(gesture.DeltaY) * Time.deltaTime * PanSpeed * 500.0f);
            }
        }

        private void TiltGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                tiltVelocity += (DeviceInfo.PixelsToUnits(gesture.DeltaY) * Time.deltaTime * TiltSpeed * 25.0f);
            }
        }

        private void ScaleGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                float zoomSpeed = ScaleGesture.ScaleMultiplierRange;
                zoomVelocity += (Target.forward * zoomSpeed * Time.deltaTime * ZoomSpeed * 100.0f);
            }
        }

        private void RotateGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                angularVelocity += (RotateGesture.RotationDegreesDelta * Time.deltaTime * RotateSpeed * 200.0f);
            }
        }

        private void OnEnable()
        {
            if (Target == null)
            {
                Target = transform;
            }

            PanGesture = new PanGestureRecognizer();
            PanGesture.StateUpdated += PanGestureCallback;
            FingersScript.Instance.AddGesture(PanGesture);

            TiltGesture = new PanGestureRecognizer();
            TiltGesture.StateUpdated += TiltGestureCallback;
            TiltGesture.ThresholdUnits = 0.5f; // higher than normal to not interfere with other gestures
            TiltGesture.MinimumNumberOfTouchesToTrack = TiltGesture.MaximumNumberOfTouchesToTrack = 2;
            FingersScript.Instance.AddGesture(TiltGesture);

            ScaleGesture = new ScaleGestureRecognizer();
            ScaleGesture.StateUpdated += ScaleGestureCallback;
            FingersScript.Instance.AddGesture(ScaleGesture);

            RotateGesture = new RotateGestureRecognizer();
            RotateGesture.StateUpdated += RotateGestureCallback;
            FingersScript.Instance.AddGesture(RotateGesture);

            PanGesture.AllowSimultaneousExecution(ScaleGesture);
            PanGesture.AllowSimultaneousExecution(RotateGesture);
            ScaleGesture.AllowSimultaneousExecution(RotateGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(ScaleGesture);
                FingersScript.Instance.RemoveGesture(RotateGesture);
            }
        }

        private void Update()
        {
            TiltGesture.ThresholdUnits = TiltThreadhold;

            Target.Translate(moveVelocity + zoomVelocity, Space.World);
            Target.Rotate(Vector3.up, angularVelocity, Space.World);
            Target.Rotate(Target.right, tiltVelocity, Space.World);

            moveVelocity *= Dampening;
            tiltVelocity *= Dampening;
            zoomVelocity *= Dampening;
            angularVelocity *= Dampening;
        }
    }
}