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
    /// Multi-drag component script
    /// </summary>
    public class DemoScriptMultiDrag : MonoBehaviour
    {
        /// <summary>
        /// Asteroid prefab
        /// </summary>
        [Tooltip("Asteroid prefab")]
        public GameObject AsteroidPrefab;

        /// <summary>
        /// Number of asteroids to spawn
        /// </summary>
        [Tooltip("Number of asteroids to spawn")]
        [Range(1, 50)]
        public int SpawnCount = 20;

        private void Start()
        {
            // randomly position and scale the asteroids in the field of view
            Bounds bounds = new Bounds
            {
                min = Camera.main.ViewportToWorldPoint(Vector3.zero),
                max = Camera.main.ViewportToWorldPoint(Vector3.one)
            };

            for (int i = 0; i < SpawnCount; i++)
            {
                GameObject obj = GameObject.Instantiate(AsteroidPrefab);
                float scale = Random.Range(0.2f, 0.5f);
                obj.transform.localScale = new Vector3(scale, scale, 1.0f);
                obj.transform.position = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0.0f);
            }
        }
    }
}
