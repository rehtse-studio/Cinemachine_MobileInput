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
    /// A dynamic grid that lays itself out in rows and columns
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(UnityEngine.UI.GridLayoutGroup))]
    [RequireComponent(typeof(UnityEngine.RectTransform))]
    public class DemoScriptDynamicGrid : MonoBehaviour
    {
        /// <summary>
        /// Rows
        /// </summary>
        [Tooltip("Rows")]
        [Range(1, 256)]
        public int Rows = 2;

        /// <summary>
        /// Columns
        /// </summary>
        [Tooltip("Columns")]
        [Range(1, 256)]
        public int Columns = 2;

        private UnityEngine.UI.GridLayoutGroup grid;
        private RectTransform rectTransform;

        private void Start()
        {
            grid = GetComponent<UnityEngine.UI.GridLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            float width = rectTransform.rect.width - (grid.spacing.x * (Columns - 1));
            float height = rectTransform.rect.height - (grid.spacing.y * (Rows - 1));
            grid.cellSize = new Vector2(width / Columns, height / Rows);
        }
    }
}
