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
    /// First person controller that allows using pans and taps to move and jump
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers First Person Controller", 5)]
    public class FingersFirstPersonControllerScript : MonoBehaviour
    {
        /// <summary>Player main rigid body</summary>
        [Header("Player")]
        [Tooltip("Player main rigid body")]
        public Rigidbody Player;

        /// <summary>Player camera</summary>
        [Tooltip("Player camera")]
        public Camera PlayerCamera;

        /// <summary>Player feet collider. Used to determine if jump is possible.</summary>
        [Tooltip("Player feet collider. Used to determine if jump is possible.")]
        public BoxCollider PlayerFeet;

        /// <summary>Move speed</summary>
        [Header("Control")]
        [Tooltip("Move speed")]
        [Range(0.1f, 100.0f)]
        public float MoveSpeed = 5.0f;

        /// <summary>Higher values reduce move speed faster as pan vertical approaches 0.</summary>
        [Tooltip("Higher values reduce move speed faster as pan vertical approaches 0.")]
        [Range(0.0f, 1.0f)]
        public float MovePower = 0.5f;

        /// <summary>Rotation speed for sideways pan</summary>
        [Tooltip("Rotation speed for sideways pan")]
        [Range(0.0f, 10.0f)]
        public float RotateSpeed = 3.0f;

        /// <summary>Higher values reduce rotation faster as pan horizontal approaches 0.</summary>
        [Tooltip("Higher values reduce rotation faster as pan horizontal approaches 0.")]
        [Range(0.0f, 16.0f)]
        public float RotationPower = 2.0f;

        /// <summary>Tilt speed for two finger rotate gesture to look up and down. Set to 0 to disable this.</summary>
        [Tooltip("Tilt speed for two finger rotate gesture to look up and down. Set to 0 to disable this.")]
        [Range(-1.0f, 1.0f)]
        public float TiltSpeed = -1.0f;

        /// <summary>Tilt angle limits if TiltSpeed is non-zero</summary>
        [Tooltip("Tilt angle limits if TiltSpeed is non-zero")]
        public Vector2 TiltLimits = new Vector2(-80.0f, 80.0f);

        /// <summary>How fast the tilt should reset back to default after tilt gesture ends</summary>
        [Tooltip("How fast the tilt should reset back to default after tilt gesture ends")]
        [Range(0.0f, 1.0f)]
        public float TiltResetSpeed = 1.0f;

        /// <summary>Jump speed/power</summary>
        [Tooltip("Jump speed/power")]
        [Range(0.0f, 32.0f)]
        public float JumpSpeed = 10.0f;

        /// <summary>How often the player can jump</summary>
        [Tooltip("How often the player can jump")]
        [Range(0.0f, 3.0f)]
        public float JumpCooldown = 0.3f;

        /// <summary>The layers the player may jump off of</summary>
        [Tooltip("The layers the player may jump off of")]
        public LayerMask JumpMask = -1;

        private float jumpTimer;
        private bool resetTilt;
        private readonly PanGestureRecognizer tiltGesture = new PanGestureRecognizer { MinimumNumberOfTouchesToTrack = 2, MaximumNumberOfTouchesToTrack = 2 };
        private float? forwardSpeed;
        private float? sideSpeed;
        private readonly Collider[] tempResults = new Collider[8];

        private void TiltGesture_StateUpdated(GestureRecognizer gesture)
        {
            if (PlayerCamera != null && gesture.State == GestureRecognizerState.Executing)
            {
                Vector3 currentRotation = PlayerCamera.transform.localRotation.eulerAngles;
                if (currentRotation.x > 270.0f)
                {
                    currentRotation.x -= 360.0f;
                }
                if (currentRotation.y > 270.0f)
                {
                    currentRotation.y -= 360.0f;
                }
                float amountX = DeviceInfo.PixelsToUnits(tiltGesture.DeltaY) * Time.deltaTime * TiltSpeed * 1000.0f;
                float amountY = DeviceInfo.PixelsToUnits(tiltGesture.DeltaX) * Time.deltaTime * TiltSpeed * -1000.0f;
                currentRotation.x = Mathf.Clamp(currentRotation.x + amountX, TiltLimits.x, TiltLimits.y);
                currentRotation.y = Mathf.Clamp(currentRotation.y + amountY, TiltLimits.x, TiltLimits.y);
                PlayerCamera.transform.localRotation = Quaternion.Euler(currentRotation);
                resetTilt = false;
            }
            else if (gesture.State == GestureRecognizerState.Ended)
            {
                resetTilt = true;
            }
        }

        private void OnEnable()
        {
            FingersScript.Instance.AddGesture(tiltGesture);
            tiltGesture.StateUpdated += TiltGesture_StateUpdated;
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(tiltGesture);
            }
            tiltGesture.StateUpdated -= TiltGesture_StateUpdated;
        }

        private void Update()
        {
            if (Camera.main == null)
            {
                return;
            }

            // reset tilt to origin if needed
            if (resetTilt)
            {
                Vector3 currentRotation = PlayerCamera.transform.localRotation.eulerAngles;
                if (currentRotation.x > 270.0f)
                {
                    currentRotation.x -= 360.0f;
                }
                if (currentRotation.y > 270.0f)
                {
                    currentRotation.y -= 360.0f;
                }
                currentRotation.x = Mathf.Lerp(currentRotation.x, 0.0f, TiltResetSpeed * Time.deltaTime * 10.0f);
                currentRotation.y = Mathf.Lerp(currentRotation.y, 0.0f, TiltResetSpeed * Time.deltaTime * 10.0f);
                PlayerCamera.transform.localRotation = Quaternion.Euler(currentRotation);
            }

            // calculate new velocity
            Vector3 velForward = Vector3.zero;
            Vector3 velUp = new Vector3(0.0f, Player.velocity.y, 0.0f);
            if (forwardSpeed != null)
            {
                velForward = Player.transform.forward * forwardSpeed.Value;
            }
            Vector3 vel = velForward + velUp;
            Player.velocity = vel;

            // reduce jump timer
            jumpTimer -= Time.deltaTime;

            // rotate if side speed is set
            if (sideSpeed != null)
            {
                float rotateSpeed = Mathf.Sign(sideSpeed.Value) * RotateSpeed * Mathf.Pow(Mathf.Abs(sideSpeed.Value), RotationPower);
                Player.angularVelocity = new Vector3(0.0f, rotateSpeed, 0.0f);
            }

            // Debug.Log("Velocity: " + Player.velocity.x.ToString() + ", " + Player.velocity.y.ToString() + ", " + Player.velocity.z.ToString());
        }

        /// <summary>
        /// Moved event - you can force movement by calling this method in your script
        /// </summary>
        /// <param name="panAmount"></param>
        public void Moved(Vector2 panAmount)
        {
            sideSpeed = panAmount.x;
            forwardSpeed = Mathf.Sign(panAmount.y) * Mathf.Pow(Mathf.Abs(panAmount.y), MovePower) * MoveSpeed;
        }

        /// <summary>
        /// Jumped event - you can force a jump by calling this method in your script
        /// </summary>
        public void Jumped()
        {
            int resultCount;
            if (jumpTimer <= 0.0f &&
                PlayerFeet != null &&
                (resultCount = Physics.OverlapBoxNonAlloc(PlayerFeet.center + PlayerFeet.transform.position, PlayerFeet.size * 0.5f, tempResults, PlayerFeet.transform.rotation, JumpMask)) > 0)
            {
                bool foundNonPlayer = false;
                for (int i = 0; i < resultCount; i++)
                {
                    if (tempResults[i].transform != Player.transform && tempResults[i].transform != PlayerFeet.transform)
                    {
                        foundNonPlayer = true;
                        break;
                    }
                }
                if (foundNonPlayer)
                {
                    jumpTimer = JumpCooldown;
                    Player.AddForce(0.0f, JumpSpeed * 100.0f, 0.0f, ForceMode.Acceleration);
                }
            }
        }
    }
}
