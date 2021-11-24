using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// Move and fire demo script
    /// </summary>
    public class DemoScriptMoveFire : MonoBehaviour
    {
        /// <summary>
        /// Mover
        /// </summary>
        [Tooltip("Mover")]
        public GameObject Mover;

        /// <summary>
        /// Projectile
        /// </summary>
        [Tooltip("Projectile")]
        public GameObject ProjectilePrefab;

        /// <summary>
        /// Projectile scale
        /// </summary>
        [Tooltip("Projectile scale")]
        [Range(0.01f, 10.0f)]
        public float ProjectileScale = 0.1f;

        /// <summary>
        /// Move speed
        /// </summary>
        [Range(0.01f, 128.0f)]
        public float MoveSpeed = 8.0f;

        /// <summary>
        /// Fire speed
        /// </summary>
        [Range(1.0f, 1024.0f)]
        public float FireSpeed = 64.0f;

        private TapGestureRecognizer fireGesture;
        private PanGestureRecognizer moveGesture;

        private IEnumerator RemoveProjectile(GameObject projectile)
        {
            yield return new WaitForSeconds(10.0f);
            Destroy(projectile);
        }

        private void TapGestureUpdated(GestureRecognizer gesture)
        {
            if (gesture.State != GestureRecognizerState.Ended)
            {
                return;
            }

            GameObject projectile = GameObject.Instantiate(ProjectilePrefab);
            projectile.transform.position = Mover.transform.position;
            projectile.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, 1.0f);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            Vector2 worldSpace = Camera.main.ScreenToWorldPoint(new Vector2(gesture.FocusX, gesture.FocusY));
            rb.velocity = FireSpeed * (worldSpace - (Vector2)Mover.transform.position).normalized;
            StartCoroutine(RemoveProjectile(projectile));
        }

        private void PanGestureUpdated(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                Vector2 worldSpace = Camera.main.ScreenToWorldPoint(new Vector2(gesture.DeltaX, gesture.DeltaY));
                Vector2 worldSpaceZero = Camera.main.ScreenToWorldPoint(Vector2.zero);
                worldSpace -= worldSpaceZero;
                Mover.transform.Translate(worldSpace.normalized * MoveSpeed * Time.deltaTime);
            }
        }

        private void Start()
        {
            fireGesture = new TapGestureRecognizer();
            fireGesture.StateUpdated += TapGestureUpdated;

            // we want the tap to fail with the pan gesture tap and pick up a new tap with another finger
            fireGesture.ClearTrackedTouchesOnEndOrFail = true;

            moveGesture = new PanGestureRecognizer();
            moveGesture.StateUpdated += PanGestureUpdated;
            moveGesture.AllowSimultaneousExecution(fireGesture);
            FingersScript.Instance.AddGesture(fireGesture);
            FingersScript.Instance.AddGesture(moveGesture);
        }
    }
}
