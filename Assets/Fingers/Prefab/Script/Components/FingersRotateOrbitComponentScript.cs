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
    /// Allows rotating around a target using a two finger rotation gesture
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Rotate Orbit", 3)]
    public class FingersRotateOrbitComponentScript : MonoBehaviour
    {
        /// <summary>The object to orbit</summary>
        [Tooltip("The object to orbit")]
        public Transform OrbitTarget;

        /// <summary>The object that will orbit around OrbitTarget</summary>
        [Tooltip("The object that will orbit around OrbitTarget")]
        public Transform Orbiter;

        /// <summary>The axis to orbit around</summary>
        [Tooltip("The axis to orbit around")]
        public Vector3 Axis = Vector3.up;

        /// <summary>The rotation speed in degrees per second</summary>
        [Tooltip("The rotation speed in degrees per second")]
        [Range(0.01f, 1000.0f)]
        public float RotationSpeed = 500.0f;

        /// <summary>The rotation dampening. Higher rotation stop rotation faster. Set to 0 for no rotation after gesture ends.</summary>
        [Tooltip("The rotation dampening. Higher rotation stop rotation faster. Set to 0 for no rotation after gesture ends.")]
        [Range(0.0f, 100.0f)]
        public float RotationDampening = 10.0f;

        private float rotationDegressDelta;

        /// <summary>
        /// Rotation gesture
        /// </summary>
        public RotateGestureRecognizer RotationGesture { get; private set; }

        private void OnEnable()
        {
            RotationGesture = new RotateGestureRecognizer();
            RotationGesture.StateUpdated += RotationGesture_Updated;

            FingersScript.Instance.AddGesture(RotationGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(RotationGesture);
            }
        }

        private void Update()
        {
            Orbiter.transform.RotateAround(OrbitTarget.transform.position, Axis, rotationDegressDelta * Time.deltaTime * RotationSpeed);
            rotationDegressDelta = Mathf.Lerp(rotationDegressDelta, 0.0f, Time.deltaTime * RotationDampening);
        }

        private void RotationGesture_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                rotationDegressDelta = RotationGesture.RotationDegreesDelta;
            }
        }
    }
}
