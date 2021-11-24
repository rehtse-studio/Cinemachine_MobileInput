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
    /// Allow controlling a rigid body in 2D with jump, move and drop through platform ability
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [AddComponentMenu("Fingers Gestures/Component/Fingers Platform Controller", 7)]
    public class FingersPlatformMoveJumpComponentScript : MonoBehaviour
    {
        private Rigidbody2D playerBody;

        /// <summary>Max velocity in x and y direction. The x and y component will be clamped to this speed.</summary>
        [Tooltip("Max velocity in x and y direction. The x and y component will be clamped to this speed.")]
        [Range(1.0f, 128.0f)]
        public float MaxSpeed = 32.0f;

        /// <summary>Force of a jump</summary>
        [Tooltip("Force of a jump")]
        [Range(1.0f, 128.0f)]
        public float JumpForce = 16.0f;

        /// <summary>How far the tap can move to still count as a jump</summary>
        [Tooltip("How far the tap can move to still count as a jump")]
        [Range(0.3f, 5.0f)]
        public float JumpThresholdUnits = 3.0f;

        /// <summary>The jump must happen with this seconds or it fails</summary>
        [Tooltip("The jump must happen with this seconds or it fails")]
        [Range(0.1f, 0.5f)]
        public float JumpThresholdSeconds = 0.3f;

        /// <summary>Move speed multiplier</summary>
        [Tooltip("Move speed multiplier")]
        [Range(0.01f, 10.0f)]
        public float MoveSpeed = 0.1f;

        /// <summary>How far the pan must move before movements starts</summary>
        [Tooltip("How far the pan must move before movements starts")]
        [Range(0.1f, 1.0f)]
        public float MoveThresholdUnits = 0.35f;

        /// <summary>Set the tag of platforms that can be jumped off of</summary>
        [Tooltip("Set the tag of platforms that can be jumped off of")]
        public string PlatformTag = "Platform";

        /// <summary>Set the name of platforms that can be jumped off of</summary>
        [Tooltip("Set the name of platforms that can be jumped off of")]
        public string PlatformName = "Platform";

        /// <summary>
        /// Tap gesture to jump
        /// </summary>
        public TapGestureRecognizer TapGesture { get; private set; }

        /// <summary>
        /// Pan gesture to move
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// Swipe gesture to drop down below platforms
        /// </summary>
        public SwipeGestureRecognizer SwipeGesture { get; private set; }

        private readonly Collider2D[] overlapArray = new Collider2D[4];

        private void OnEnable()
        {
            playerBody = GetComponent<Rigidbody2D>();

            TapGesture = new TapGestureRecognizer();
            TapGesture.StateUpdated += JumpTap_StateUpdated;
            TapGesture.ClearTrackedTouchesOnEndOrFail = true;

            // allow other fingers to jump while panning
            TapGesture.MaximumNumberOfTouchesToTrack = 10;

            // require fast taps
            TapGesture.ThresholdSeconds = JumpThresholdSeconds;

            // allow a little more slide than a normal tap
            TapGesture.ThresholdUnits = JumpThresholdUnits;

            PanGesture = new PanGestureRecognizer();
            PanGesture.StateUpdated += MovePan_StateUpdated;
            PanGesture.ThresholdUnits = 0.35f; // require a little more slide before panning starts

            // jump up and move sideways is allowed
            PanGesture.AllowSimultaneousExecution(TapGesture);

            // swipe down requires no other gestures to be executing
            SwipeGesture = new SwipeGestureRecognizer
            {
                Direction = SwipeGestureRecognizerDirection.Down
            };
            SwipeGesture.StateUpdated += SwipeDown_StateUpdated;
            SwipeGesture.AllowSimultaneousExecution(PanGesture);

            FingersScript.Instance.AddGesture(TapGesture);
            FingersScript.Instance.AddGesture(PanGesture);
            FingersScript.Instance.AddGesture(SwipeGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(TapGesture);
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(SwipeGesture);
            }
        }

        private IEnumerator StopFallThrough(PlatformEffector2D effector)
        {
            yield return new WaitForSeconds(0.35f);
            effector.rotationalOffset = 0.0f;
        }

        private Collider2D FindIntersectingPlatform()
        {
            // find a platform intersecting the player - you could also tag the platform object
            //  or put it in a different layer, I have chosen to look at the object name
            ContactFilter2D filter = new ContactFilter2D();
            int count = playerBody.OverlapCollider(filter, overlapArray);
            for (int i = 0; i < count; i++)
            {
                if ((!string.IsNullOrEmpty(PlatformTag) && overlapArray[i].tag == PlatformTag) ||
                    (!string.IsNullOrEmpty(PlatformName) && overlapArray[i].name.IndexOf(PlatformName, System.StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return overlapArray[i];
                }
            }
            return null;
        }

        private void SwipeDown_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                // if on a platform, drop down
                Collider2D platform = FindIntersectingPlatform();
                if (platform != null)
                {
                    PlatformEffector2D effector = platform.GetComponent<PlatformEffector2D>();
                    if (effector != null)
                    {
                        // allow fall through
                        effector.rotationalOffset = -180.0f;

                        StartCoroutine(StopFallThrough(effector));
                    }
                }
            }
        }

        private void MovePan_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                Vector2 velocity = playerBody.velocity;
                velocity.x += (gesture.VelocityX * Time.deltaTime * MoveSpeed);
                playerBody.velocity = velocity;
            }
        }

        private void JumpTap_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                // if on a platform, jump
                if (FindIntersectingPlatform() != null)
                {
                    // jump, touching a platform
                    Vector2 velocity = playerBody.velocity;
                    velocity.y = JumpForce;
                    playerBody.velocity = velocity;
                }
            }
        }

        private void FixedUpdate()
        {
            Vector2 velocity = playerBody.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -MaxSpeed, MaxSpeed);
            velocity.y = Mathf.Clamp(velocity.y, -MaxSpeed, MaxSpeed);
            playerBody.velocity = velocity;
        }
    }
}
