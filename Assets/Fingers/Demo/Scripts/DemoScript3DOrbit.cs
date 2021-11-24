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
    /// Demo script for 3d orbit
    /// </summary>
	public class DemoScript3DOrbit : MonoBehaviour
	{
        private FingersPanOrbitComponentScript orbit;

        private void Start()
        {
            orbit = GetComponent<FingersPanOrbitComponentScript>();
            orbit.OrbitTargetTapped += Orbit_OrbitTargetTapped;
        }

        private void Orbit_OrbitTargetTapped()
        {
            Debug.Log("Capsule tapped");
        }

        private void Update()
		{
            if (FingersScript.Instance.IsKeyDownThisFrame(KeyCode.Escape))
            {
				UnityEngine.SceneManagement.SceneManager.LoadScene(0);
			}
		}
	}
}
