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
    /// A third person controller that allows movement and jumping
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Third Person Controller", 5)]
    public class FingersThirdPersonControllerScript : MonoBehaviour
    {
        /// <summary>Player main rigid body</summary>
        [Header("Player")]
        [Tooltip("Player main rigid body")]
        public Rigidbody Player;

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

        /// <summary>Camera z offset</summary>
        [Header("Camera")]
        [Tooltip("Camera z offset")]
        [Range(1.0f, 100.0f)]
        public float CameraZOffset = 10.0f;

        /// <summary>Camera y offset</summary>
        [Tooltip("Camera y offset")]
        [Range(0.0f, 100.0f)]
        public float CameraYOffset = 5.0f;

        /// <summary>Zoom dampening. This causes the zoom to stop faster with higher values. Set to 0 for no zoom at all.</summary>
        [Tooltip("Zoom dampening. This causes the zoom to stop faster with higher values. Set to 0 for no zoom at all.")]
        [Range(0.0f, 100.0f)]
        public float ZoomDampening = 10.0f;

        /// <summary>Min/max camera z distance from player</summary>
        [Tooltip("Min/max camera z distance from player")]
        public Vector2 CameraZDistanceRange = new Vector2(5.0f, 30.0f);

        private float scaleVelocity = 1.0f;

        private float? forwardSpeed;
        private float? sideSpeed;
        private readonly Collider[] tempResults = new Collider[8];

        private void Update()
        {
            if (Camera.main == null)
            {
                return;
            }

            // face player in same direction as camera
            Vector3 cameraRotation = Camera.main.transform.rotation.eulerAngles;
            cameraRotation.x = cameraRotation.z = 0.0f; // only rotate player around y axis
            Player.transform.rotation = Quaternion.Euler(cameraRotation);

            // camera zoom velocity
            if (ZoomDampening > 0.0f && Mathf.Abs(1.0f - scaleVelocity) > 0.001f)
            {
                CameraZOffset = Mathf.Clamp(CameraZOffset * scaleVelocity, CameraZDistanceRange.x, CameraZDistanceRange.y);
                scaleVelocity = Mathf.Lerp(scaleVelocity, 1.0f, Time.deltaTime * ZoomDampening);
            }

            // position camera from player
            Vector3 pos = Player.transform.position;
            pos += (Player.transform.forward * -CameraZOffset);
            pos += (Player.transform.up * CameraYOffset);
            Camera.main.transform.position = pos;
            Camera.main.transform.LookAt(Player.transform);

            // calculate new velocity
            Vector3 velRight = Vector3.zero;
            Vector3 velForward = Vector3.zero;
            Vector3 velUp = new Vector3(0.0f, Player.velocity.y, 0.0f);
            if (forwardSpeed != null)
            {
                velForward = Player.transform.forward * forwardSpeed.Value;
            }
            if (sideSpeed != null)
            {
                velRight = Player.transform.right * sideSpeed.Value;
            }
            Vector3 vel = velRight + velForward + velUp;
            Player.velocity = vel;

            // reduce jump timer
            jumpTimer -= Time.deltaTime;

            // Debug.Log("Velocity: " + Player.velocity.x.ToString() + ", " + Player.velocity.y.ToString() + ", " + Player.velocity.z.ToString());
        }

        /// <summary>
        /// Moved event. Call this from your script to perform movement.
        /// </summary>
        /// <param name="panAmount">The amount of the pan that will cause movement</param>
        public void Moved(Vector2 panAmount)
        {
            sideSpeed = panAmount.x * MoveSpeed;
            forwardSpeed = Mathf.Sign(panAmount.y) * Mathf.Pow(Mathf.Abs(panAmount.y), MovePower) * MoveSpeed;
        }

        /// <summary>
        /// Jumped event. Call this from your script to perform a jump.
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

        /// <summary>
        /// Scaled event. Call this from your script to initiate zoom the main camera in and out.
        /// </summary>
        /// <param name="scaleGesture">Scale gesture.</param>
        public void Scaled(GestureRecognizer scaleGesture)
        {
            if (scaleGesture.State == GestureRecognizerState.Executing)
            {
                float scaleMultiplier = (scaleGesture as ScaleGestureRecognizer).ScaleMultiplier;
                if (scaleMultiplier != 1.0f)
                {
                    scaleVelocity = scaleMultiplier;
                }
            }
        }
    }
}
