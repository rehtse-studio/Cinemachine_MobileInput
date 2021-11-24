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
    /// DPad demo script
    /// </summary>
    public class DemoScriptDPad : MonoBehaviour
    {
        /// <summary>
        /// First dpad script
        /// </summary>
        [Tooltip("Fingers DPad Script #1")]
        public FingersDPadScript DPadScript;

        /// <summary>
        /// Second dpad script
        /// </summary>
        [Tooltip("Fingers DPad Script #2")]
        public FingersDPadScript DPadScript2;

        /// <summary>
        /// First mover
        /// </summary>
        [Tooltip("Object to move with the first dpad")]
        public GameObject Mover;

        /// <summary>
        /// Second mover
        /// </summary>
        [Tooltip("Object to move with the second dpad")]
        public GameObject Mover2;

        /// <summary>
        /// Move speed
        /// </summary>
        [Tooltip("Units per second to move the square with dpad")]
        public float Speed = 250.0f;

        //[Tooltip("Whether dpad moves to touch start location")]
        //public bool MoveDPadToGestureStartLocation;

        private Vector3 startPos;
        private Vector3 startPos2;

        private void Awake()
        {
            DPadScript.DPadItemTapped = DPadTapped;
            DPadScript.DPadItemPanned = DPadPanned;
            DPadScript2.DPadItemTapped = DPadTapped;
            DPadScript2.DPadItemPanned = DPadPanned;
            startPos = Mover.transform.position;
            startPos2 = Mover2.transform.position;
            //DPadScript.MoveDPadToGestureStartLocation = MoveDPadToGestureStartLocation;
            //DPadScript2.MoveDPadToGestureStartLocation = MoveDPadToGestureStartLocation;
        }

        private void DPadTapped(FingersDPadScript script, FingersDPadItem item, TapGestureRecognizer gesture)
        {
            if ((item & FingersDPadItem.Center) != FingersDPadItem.None)
            {
                GameObject mover = (script == DPadScript ? Mover : Mover2);
                mover.transform.position = (script == DPadScript ? startPos : startPos2);
            }
        }

        private void DPadPanned(FingersDPadScript script, FingersDPadItem item, PanGestureRecognizer gesture)
        {
            GameObject mover = (script == DPadScript ? Mover : Mover2);
            Vector3 pos = mover.transform.position;
            float move = Speed * Time.deltaTime;
            if ((item & FingersDPadItem.Right) != FingersDPadItem.None)
            {
                pos.x += move;
            }
            if ((item & FingersDPadItem.Left) != FingersDPadItem.None)
            {
                pos.x -= move;
            }
            if ((item & FingersDPadItem.Up) != FingersDPadItem.None)
            {
                pos.y += move;
            }
            if ((item & FingersDPadItem.Down) != FingersDPadItem.None)
            {
                pos.y -= move;
            }
            mover.transform.position = pos;
        }
    }
}
