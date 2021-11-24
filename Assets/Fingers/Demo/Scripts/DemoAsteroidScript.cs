//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;

namespace DigitalRubyShared
{
    /// <summary>
    /// Simple script that destroys an object when it is not visible anymore
    /// </summary>
	public class DemoAsteroidScript : MonoBehaviour
	{
		private void Start ()
		{
		
		}

		private void Update ()
		{
			
		}

		private void OnBecameInvisible()
		{
			GameObject.Destroy(gameObject);
		}
	}
}