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
    /// Allows orbiting a target using a pan gesture to drag up and down or left and right to orbit, and pinch to zoom in and out
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Pan Orbit", 2)]
    public class FingersPanOrbitComponentScript : MonoBehaviour
    {
        /// <summary>The transform to orbit around.</summary>
        [Tooltip("The transform to orbit around.")]
        public Transform OrbitTarget;

        /// <summary>The object to orbit around OrbitTarget.</summary>
        [Tooltip("The object to orbit around OrbitTarget.")]
        public Transform Orbiter;

        /// <summary>Whether to look at the target automatically. Set to false if you are controlling this yourself.</summary>
        [Tooltip("Whether to look at the target automatically. Set to false if you are controlling this yourself.")]
        public bool LookAtTarget = true;

        /// <summary>Clamp camera to specified world space bounds, null for no clamp</summary>
        [Tooltip("Clamp camera to specified world space bounds, null for no clamp")]
        public BoxCollider ClampBounds;

        /// <summary>The minimium distance to move to the orbit target, 0 for no minimum.</summary>
        [Tooltip("The minimium distance to move to the orbit target, 0 for no minimum.")]
        [Range(0.1f, 100.0f)]
        public float MinimumDistance = 5.0f;

        /// <summary>The maximum distance to move away from the orbit target, 0 for no maximum.</summary>
        [Tooltip("The maximum distance to move away from the orbit target, 0 for no maximum.")]
        [Range(0.1f, 1000.0f)]
        public float MaximumDistance = 1000.0f;

        /// <summary>The zoom speed</summary>
        [Tooltip("The zoom speed")]
        [Range(0.01f, 100.0f)]
        public float ZoomSpeed = 20.0f;

        /// <summary>The speed at which the orbiter looks at the orbit target is it has panned away from looking direclty at the orbit target.</summary>
        [Tooltip("The speed at which the orbiter looks at the orbit target is it has panned away from looking direclty at the orbit target.")]
        [Range(0.0f, 10.0f)]
        public float ZoomLookAtSpeed = 1.0f;

        /// <summary>The threshold in units before zooming begins to happen. Start distance must change this much in order to start the gesture.</summary>
        [Tooltip("The threshold in units before zooming begins to happen. Start distance must change this much in order to start the gesture.")]
        [Range(0.0f, 3.0f)]
        public float ZoomThresholdUnits = 0.15f;

        /// <summary>The speed (degrees per second) at which to orbit using x delta pan gesture values. Negative or positive values will cause orbit in the opposite direction.</summary>
        [Tooltip("The speed (degrees per second) at which to orbit using x delta pan gesture values. Negative or positive values will cause orbit in the opposite direction.")]
        [Range(-100.0f, 100.0f)]
        public float OrbitXSpeed = -30.0f;

        /// <summary>The maximum degrees to orbit on the x axis from the starting x rotation. 0 for no limit. Set OrbitXSpeed to 0 to disable x orbit.</summary>
        [Tooltip("The maximum degrees to orbit on the x axis from the starting x rotation. 0 for no limit. Set OrbitXSpeed to 0 to disable x orbit.")]
        [Range(0.0f, 360.0f)]
        public float OrbitXMaxDegrees = 0.0f;

        /// <summary>Whether the orbit on the x axis is a pan (move sideways) instead of an orbit.</summary>
        [Tooltip("Whether the orbit on the x axis is a pan (move sideways) instead of an orbit.")]
        public PanOrbitMovementType XAxisMovementType = PanOrbitMovementType.Orbit;

        /// <summary>Speed if OrbitXPan is true</summary>
        [Tooltip("Speed if OrbitXPan is true")]
        [Range(-10.0f, 10.0f)]
        public float OrbitXPanSpeed = 1.0f;

        /// <summary>Set a movement limit from orbit target if OrbitXPan is true. 0 for no limit.</summary>
        [Tooltip("Set a movement limit from orbit target if OrbitXPan is true. 0 for no limit.")]
        [Range(0.0f, 1000.0f)]
        public float OrbitXPanLimit = 100.0f;

        /// <summary>The speed (degrees per second) at which to orbit using y delta pan gesture values. Negative or positive values will cause orbit in the opposite direction.</summary>
        [Tooltip("The speed (degrees per second) at which to orbit using y delta pan gesture values. Negative or positive values will cause orbit in the opposite direction.")]
        [Range(-100.0f, 100.0f)]
        public float OrbitYSpeed = -30.0f;

        /// <summary>The maximum degrees to orbit on the y axis from the starting y rotation. 0 for no limit. Set OrbitYSpeed to 0 to disable y orbit.</summary>
        [Tooltip("The maximum degrees to orbit on the y axis from the starting y rotation. 0 for no limit. Set OrbitYSpeed to 0 to disable y orbit.")]
        [Range(0.0f, 360.0f)]
        public float OrbitYMaxDegrees = 0.0f;

        /// <summary>Whether the orbit on the y axis is a pan (move sideways) instead of an orbit.</summary>
        [Tooltip("Whether the orbit on the y axis is a pan (move sideways) instead of an orbit.")]
        public PanOrbitMovementType YAxisMovementType = PanOrbitMovementType.Orbit;

        /// <summary>Speed if OrbitYPan is true.</summary>
        [Tooltip("Speed if OrbitYPan is true.")]
        [Range(-10.0f, 10.0f)]
        public float OrbitYPanSpeed = 1.0f;

        /// <summary>Set a movement limit from orbit target if OrbitYPan is true. 0 for no limit.</summary>
        [Tooltip("Set a movement limit from orbit target if OrbitYPan is true. 0 for no limit.")]
        [Range(0.0f, 1000.0f)]
        public float OrbitYPanLimit = 100.0f;

        /// <summary>Whether to allow orbit while zooming.</summary>
        [Tooltip("Whether to allow orbit while zooming.")]
        public bool AllowOrbitWhileZooming = true;
        private bool allowOrbitWhileZooming;

        /// <summary>Whether to allow orbit and/or pan on both axis at the same time or to only pick the axis with the greatest movement.</summary>
        [Tooltip("Whether to allow orbit and/or pan on both axis at the same time or to only pick the axis with the greatest movement.")]
        public bool AllowMovementOnBothAxisSimultaneously = true;
        private int lockedAxis = 0; // 0 = none, 1 = x, 2 = y

        /// <summary>How much the velocity of the orbit will cause additional orbit after the gesture stops. 1 for no inertia (orbits forever) or 0 for immediate stop.</summary>
        [Tooltip("How much the velocity of the orbit will cause additional orbit after the gesture stops. 1 for no inertia (orbits forever) or 0 for immediate stop.")]
        [Range(0.0f, 1.0f)]
        public float OrbitInertia = 0.925f;

        /// <summary>The max size for the orbit or pan. An x,y or z value larget than this away from orbit target will be clamped in. Set to 0 for no limit.</summary>
        [Tooltip("The max size for the orbit or pan. An x,y or z value larget than this away from orbit target will be clamped in. Set to 0 for no limit.")]
        public Vector3 OrbitMaximumSize;

        /// <summary>Whether the pan and rotate orbit gestures must start on the orbit target to orbit. The tap gesture always requires that it be on the orbit target.</summary>
        [Tooltip("Whether the pan and rotate orbit gestures must start on the orbit target to orbit. The tap gesture always requires that it be on the orbit target.")]
        public bool RequireOrbitGesturesToStartOnTarget;

        /// <summary>
        /// Types of movement
        /// </summary>
        public enum PanOrbitMovementType
        {
            /// <summary>
            /// Orbit only
            /// </summary>
            Orbit,

            /// <summary>
            /// Pan only
            /// </summary>
            Pan,

            /// <summary>
            /// One touch orbit, two touch pan
            /// </summary>
            OrbitWithTwoFingerPan
        }

        /// <summary>
        /// Scale gesture to zoom in and out
        /// </summary>
        public ScaleGestureRecognizer ScaleGesture { get; private set; }

        /// <summary>
        /// Pan gesture to orbit
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// Tap gesture to tap on orbit target
        /// </summary>
        public TapGestureRecognizer TapGesture { get; private set; }

        private float xDegrees;
        private float yDegrees;
        private Vector2 panVelocity;
        private float zoomSpeed;

        /// <summary>
        /// Fires when the orbit target is tapped
        /// </summary>
        public event System.Action OrbitTargetTapped;

        private void OnEnable()
        {
            // create a scale gesture to zoom orbiter in and out
            ScaleGesture = new ScaleGestureRecognizer { ZoomSpeed = ZoomSpeed };
            ScaleGesture.StateUpdated += ScaleGesture_Updated;
            ScaleGesture.ThresholdUnits = ZoomThresholdUnits;

            // pan gesture
            PanGesture = new PanGestureRecognizer();
            PanGesture.MaximumNumberOfTouchesToTrack = 2;
            PanGesture.StateUpdated += PanGesture_Updated;

            // create a tap gesture that only executes on the target, note that this requires a physics ray caster on the camera
            TapGesture = new TapGestureRecognizer();
            TapGesture.StateUpdated += TapGesture_Updated;
            TapGesture.PlatformSpecificView = OrbitTarget.gameObject;

            if (RequireOrbitGesturesToStartOnTarget)
            {
                ScaleGesture.PlatformSpecificView = OrbitTarget.gameObject;
                PanGesture.PlatformSpecificView = OrbitTarget.gameObject;
            }

            if (LookAtTarget)
            {
                // point oribiter at target
                Orbiter.transform.LookAt(OrbitTarget.transform);
            }

            FingersScript.Instance.AddGesture(ScaleGesture);
            FingersScript.Instance.AddGesture(PanGesture);
            FingersScript.Instance.AddGesture(TapGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(ScaleGesture);
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(TapGesture);
            }
        }

        private void LateUpdate()
        {
            if (allowOrbitWhileZooming != AllowOrbitWhileZooming)
            {
                allowOrbitWhileZooming = AllowOrbitWhileZooming;
                if (allowOrbitWhileZooming)
                {
                    ScaleGesture.AllowSimultaneousExecution(PanGesture);
                }
                else
                {
                    ScaleGesture.DisallowSimultaneousExecution(PanGesture);
                }
            }
            Vector3 startPos = Orbiter.transform.position;
            UpdateOrbit(panVelocity.x, panVelocity.y);
            UpdateZoom();
            ClampDistance(startPos);
            panVelocity *= OrbitInertia;
            zoomSpeed *= OrbitInertia;
            if (ClampBounds != null && !ClampBounds.bounds.Contains(Orbiter.transform.position))
            {
                Vector3 dir = (OrbitTarget.transform.position - Orbiter.transform.position).normalized;
                float dist;
                ClampBounds.bounds.IntersectRay(new Ray(Orbiter.transform.position, dir), out dist);
                Orbiter.transform.position += (dir * dist);
            }
        }

        private bool IntersectRaySphere(Vector3 rayOrigin, Vector3 rayDir, Vector3 sphereCenter, float sphereRadius, out float distanceToSphere, out Vector3 intersectPos)
        {
            Vector3 m = rayOrigin - sphereCenter;
            float b = Vector3.Dot(m, rayDir);
            float c = Vector3.Dot(m, m) - (sphereRadius * sphereRadius);

            // Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
            if (c > 0.0f && b > 0.0f)
            {
                distanceToSphere = 0.0f;
                intersectPos = Vector3.zero;
                return false;
            }
            float discr = (b * b) - c;

            // A negative discriminant corresponds to ray missing sphere 
            if (discr < 0.0f)
            {
                distanceToSphere = 0.0f;
                intersectPos = Vector3.zero;
                return false;
            }

            // Ray now found to intersect sphere, compute smallest t value of intersection
            // If t is negative, ray started inside sphere so clamp t to zero 
            distanceToSphere = Mathf.Max(0.0f, -b - Mathf.Sqrt(discr));

            // set intersect point
            intersectPos = rayOrigin + (distanceToSphere * rayDir);

            return true;
        }

        private void ClampDistance(Vector3 startPos)
        {
            Vector3 orbitPos = Orbiter.transform.position;
            if ((startPos != orbitPos) && (MinimumDistance > 0.0f || MaximumDistance > 0.0f))
            {
                Vector3 targetPos = OrbitTarget.transform.position;
                Vector3 dirFromTarget = (orbitPos - targetPos).normalized;
                Vector3 intersectPos;
                float distanceToSphere;

                // check if moved through min distance sphere, if so put back to start
                if (MinimumDistance > 0.0f && IntersectRaySphere(startPos, (orbitPos - startPos).normalized, targetPos, MinimumDistance, out distanceToSphere, out intersectPos) &&
                    distanceToSphere <= 0.0f)
                {
                    // position orbiter at sphere intersection point plus a tiny bit extra
                    Orbiter.transform.position = targetPos + (dirFromTarget * (MinimumDistance * (1.0f + Mathf.Epsilon)));
                    panVelocity = Vector3.zero;
                    zoomSpeed = 0.0f;
                }
                else
                {
                    float distance = Vector3.Distance(targetPos, orbitPos);
                    float newDistance = Mathf.Clamp(distance, MinimumDistance, MaximumDistance);
                    if (newDistance != distance)
                    {
                        Orbiter.transform.position = targetPos + (dirFromTarget * newDistance);
                        panVelocity = Vector3.zero;
                        zoomSpeed = 0.0f;
                    }
                }
            }
        }

        private void UpdateZoom()
        {
            if (zoomSpeed >= -0.01f && zoomSpeed <= 0.01f)
            {
                zoomSpeed = 0.0f;
                return;
            }
            else if (LookAtTarget)
            {
                Vector3 lookAtDir = (OrbitTarget.transform.position - Orbiter.transform.position).normalized;
                Quaternion lookAtRotation = Quaternion.LookRotation(lookAtDir, Orbiter.transform.up);
                Quaternion currentRotation = Orbiter.transform.rotation;
                Orbiter.transform.rotation = Quaternion.Lerp(currentRotation, lookAtRotation, ZoomLookAtSpeed * Time.deltaTime);
            }
            Orbiter.transform.position += (Orbiter.transform.forward * zoomSpeed * Time.deltaTime);
        }

        private void PerformPan(Vector3 pan, float limit)
        {
            Vector3 pos = Orbiter.transform.position;
            Orbiter.Translate(pan, Space.Self);
            if (limit > 0.0f)
            {
                float distance = Vector3.Distance(Orbiter.transform.position, OrbitTarget.transform.position);
                if (distance > limit)
                {
                    Orbiter.transform.position = pos;
                }
            }
        }

        private void UpdateOrbit(float xVelocity, float yVelocity)
        {
            // orbit the target in either direction depending on pan gesture delta x and y
            if (OrbitXSpeed != 0.0f && yVelocity != 0.0f)
            {
                if (YAxisMovementType == PanOrbitMovementType.Pan || (YAxisMovementType == PanOrbitMovementType.OrbitWithTwoFingerPan && PanGesture.CurrentTrackedTouches.Count > 1))
                {
                    PerformPan(new Vector3(0.0f, yVelocity * (OrbitYPanSpeed) * Time.deltaTime, 0.0f), OrbitYPanLimit);
                }
                else
                {
                    float addAngle = yVelocity * OrbitXSpeed * Time.deltaTime;
                    if (OrbitXMaxDegrees > 0.0f)
                    {
                        float newDegrees = xDegrees + addAngle;
                        if (newDegrees > OrbitXMaxDegrees)
                        {
                            addAngle = OrbitXMaxDegrees - xDegrees;
                        }
                        else if (newDegrees < -OrbitXMaxDegrees)
                        {
                            addAngle = -OrbitXMaxDegrees - xDegrees;
                        }
                    }
                    xDegrees += addAngle;
                    Orbiter.RotateAround(OrbitTarget.transform.position, Orbiter.transform.right, addAngle);
                }
            }
            if (OrbitYSpeed != 0.0f && xVelocity != 0.0f)
            {
                if (XAxisMovementType == PanOrbitMovementType.Pan || (XAxisMovementType == PanOrbitMovementType.OrbitWithTwoFingerPan && PanGesture.CurrentTrackedTouches.Count > 1))
                {
                    PerformPan(new Vector3(xVelocity * (OrbitXPanSpeed) * Time.deltaTime, 0.0f, 0.0f), OrbitXPanLimit);
                }
                else
                {
                    float addAngle = xVelocity * OrbitYSpeed * Time.deltaTime;
                    if (OrbitYMaxDegrees > 0.0f)
                    {
                        float newDegrees = yDegrees + addAngle;
                        if (newDegrees > OrbitYMaxDegrees)
                        {
                            addAngle = OrbitYMaxDegrees - yDegrees;
                        }
                        else if (newDegrees < -OrbitYMaxDegrees)
                        {
                            addAngle = -OrbitYMaxDegrees - yDegrees;
                        }
                    }
                    yDegrees += addAngle;
                    Orbiter.RotateAround(OrbitTarget.transform.position, Vector3.up, addAngle);
                }
            }
        }

        private void TapGesture_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                if (OrbitTargetTapped != null)
                {
                    OrbitTargetTapped.Invoke();
                }
            }
        }

        private void PanGesture_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            // if gesture is not executing, exit function
            if (gesture.State != GestureRecognizerState.Executing)
            {
                if (gesture.State == GestureRecognizerState.Ended)
                {
                    lockedAxis = 0;
                    if (OrbitInertia > 0.0f)
                    {
                        panVelocity = new Vector2(gesture.VelocityX * 0.01f, gesture.VelocityY * 0.01f);
                        if (OrbitXSpeed == 0.0f)
                        {
                            panVelocity.x = 0.0f;
                        }
                        if (OrbitYSpeed == 0.0f)
                        {
                            panVelocity.y = 0.0f;
                        }
                    }
                }
                else if (gesture.State == GestureRecognizerState.Began)
                {
                    panVelocity = Vector2.zero;
                }
                return;
            }
            else
            {
                float xVelocity = gesture.DeltaX;
                float yVelocity = gesture.DeltaY;
                if (PanGestureHasEnoughMovementOnOneAxis(ref xVelocity, ref yVelocity))
                {
                    UpdateOrbit(xVelocity, yVelocity);
                }
            }
        }

        private void ScaleGesture_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            // if gesture is not executing, exit function
            if (gesture.State != GestureRecognizerState.Executing)
            {
                return;
            }

            zoomSpeed += ScaleGesture.ScaleMultiplierRange;
        }

        private bool PanGestureHasEnoughMovementOnOneAxis(ref float xVelocity, ref float yVelocity)
        {
            if (AllowMovementOnBothAxisSimultaneously)
            {
                return true;
            }

            float unitsX = Mathf.Abs(DeviceInfo.PixelsToUnits(PanGesture.DistanceX));
            float unitsY = Mathf.Abs(DeviceInfo.PixelsToUnits(PanGesture.DistanceY));
            if (lockedAxis == 0 && unitsX <= PanGesture.ThresholdUnits && unitsY <= PanGesture.ThresholdUnits)
            {
                return false;
            }
            else if (lockedAxis == 1 || (lockedAxis == 0 && unitsX > unitsY * 3.0f))
            {
                lockedAxis = 1;
                yVelocity = 0.0f;
                return true;
            }
            else if (lockedAxis == 2 || (lockedAxis == 0 && unitsY > unitsX * 3.0f))
            {
                lockedAxis = 2;
                xVelocity = 0.0f;
                return true;
            }
            return false;
        }
    }
}
