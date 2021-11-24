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
    /// Allows rotating a transform using a pan gesture, allowing the transform to be rotated around using left/right or up/down pans.
    /// </summary>
    public class FingersRotateCameraComponentScript : MonoBehaviour
    {
        /// <summary>The Transform component to rotate. Defaults to transform that this script is on.</summary>
        [Tooltip("The Transform component to rotate. Defaults to transform that this script is on.")]
        public Transform TransformToRotate;

        /// <summary>Number of touches to register the pan</summary>
        [Tooltip("Number of touches to register the pan")]
        public int NumberOfTouchesRequired = 1;

        /// <summary>The view that must be touched to start the gesture. Leave null to start the gesture anywhere.</summary>
        [Tooltip("The view that must be touched to start the gesture. Leave null to start the gesture anywhere.")]
        public GameObject GestureView;

        /// <summary>Rotation speed</summary>
        [Tooltip("Rotation speed")]
        [Range(-100.0f, 100.0f)]
        public float RotationSpeed = 20.0f;

        /// <summary>Min and max rotation around x axis</summary>
        [Tooltip("Min and max rotation around x axis")]
        public Vector2 RotationXMinMax = new Vector2(-60.0f, 60.0f);

        /// <summary>Rotation dampening. Reduces rotation quickly once gesture ends. Set to 0 for complete dampening.</summary>
        [Tooltip("Rotation dampening. Reduces rotation quickly once gesture ends. Set to 0 for complete dampening.")]
        [Range(0.0f, 1.0f)]
        public float RotationDampening = 0.8f;

        /// <summary>
        /// Pan gesture that performs the rotation
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        private float gestureDeltaXRotation;
        private float gestureDeltaYRotation;
        private Quaternion originalRotation;
        private Vector2 rotationVelocity;

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }

            return Mathf.Clamp(angle, min, max);
        }

        private void ApplyRotation(float x, float y)
        {
            gestureDeltaXRotation = ClampAngle(gestureDeltaXRotation + x, -360.0f, 360.0f);
            gestureDeltaYRotation = ClampAngle(gestureDeltaYRotation + y, RotationXMinMax.x, RotationXMinMax.y);
            Quaternion xQuaternion = Quaternion.AngleAxis(gestureDeltaXRotation, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(gestureDeltaYRotation, Vector3.left);
            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }

        private void PanGestureUpdated(GestureRecognizer r)
        {
            if (TransformToRotate == null)
            {
                return;
            }

            if (r.State == GestureRecognizerState.Executing)
            {
                rotationVelocity = Vector2.zero;
                ApplyRotation(DeviceInfo.PixelsToUnits(r.DeltaX) * RotationSpeed, DeviceInfo.PixelsToUnits(r.DeltaY) * RotationSpeed);
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                rotationVelocity = new Vector2(DeviceInfo.PixelsToUnits(r.VelocityX) * RotationSpeed * 0.01f, DeviceInfo.PixelsToUnits(r.VelocityY) * RotationSpeed * 0.01f);
            }
        }

        private void OnEnable()
        {
            PanGesture = new PanGestureRecognizer();
            PanGesture.StateUpdated += PanGestureUpdated;
            PanGesture.PlatformSpecificView = GestureView;
            FingersScript.Instance.AddGesture(PanGesture);
            TransformToRotate = (TransformToRotate == null ? transform : TransformToRotate);
            originalRotation = (TransformToRotate == null ? Quaternion.identity : TransformToRotate.localRotation);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
            }
        }

        private void Update()
        {
            TransformToRotate = (TransformToRotate == null ? transform : TransformToRotate);
            PanGesture.MinimumNumberOfTouchesToTrack = PanGesture.MaximumNumberOfTouchesToTrack = NumberOfTouchesRequired;
            PanGesture.PlatformSpecificView = GestureView;
            if (rotationVelocity.x != 0.0f || rotationVelocity.y != 0.0f)
            {
                ApplyRotation(rotationVelocity.x, rotationVelocity.y);
                rotationVelocity *= RotationDampening;
            }
        }
    }
}
