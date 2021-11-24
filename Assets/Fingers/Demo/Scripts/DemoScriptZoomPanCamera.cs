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
    /// Zoom pan demo script
    /// </summary>
    public class DemoScriptZoomPanCamera : MonoBehaviour
    {
        /// <summary>
        /// Orthographic text box change, changes the main camera to orthographic or non-orthographic
        /// </summary>
        /// <param name="orthographic">Orthographic flag</param>
        public void OrthographicCameraOptionChanged(bool orthographic)
        {
            Camera.main.orthographic = orthographic;
        }
    }
}
