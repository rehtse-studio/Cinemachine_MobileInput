using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace DigitalRubyShared
{
    /// <summary>
    /// Utility methods
    /// </summary>
    public static class FingersUtility
    {
        //****************************************************************************************************
        //  static function DrawLine(rect : Rect) : void
        //  static function DrawLine(rect : Rect, color : Color) : void
        //  static function DrawLine(rect : Rect, width : float) : void
        //  static function DrawLine(rect : Rect, color : Color, width : float) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, width : float) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color, width : float) : void
        //****************************************************************************************************

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        public static void DrawLine(this Image img, Rect rect) { DrawLine(img, rect, Color.white, 1.0f); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public static void DrawLine(this Image img, Rect rect, Color color) { DrawLine(img, rect, color, 1.0f); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        /// <param name="width"></param>
        public static void DrawLine(this Image img, Rect rect, float width) { DrawLine(img, rect, Color.white, width); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        /// <param name="width"></param>
        public static void DrawLine(this Image img, Rect rect, Color color, float width) { DrawLine(img, new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        public static void DrawLine(this Image img, Vector2 pointA, Vector2 pointB) { DrawLine(img, pointA, pointB, Color.white, 1.0f); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="color"></param>
        public static void DrawLine(this Image img, Vector2 pointA, Vector2 pointB, Color color) { DrawLine(img, pointA, pointB, color, 1.0f); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="width"></param>
        public static void DrawLine(this Image img, Vector2 pointA, Vector2 pointB, float width) { DrawLine(img, pointA, pointB, Color.white, width); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="color"></param>
        /// <param name="width"></param>
        public static void DrawLine(this Image img, Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            RectTransform imageRectTransform = img.rectTransform;
            img.material.color = color;
            Vector3 differenceVector = pointB - pointA;
            float distance = differenceVector.magnitude;
            if (img.canvas.scaleFactor > 0.0f)
            {
                distance /= img.canvas.scaleFactor;
            }
            imageRectTransform.sizeDelta = new Vector2(distance, width);
            imageRectTransform.pivot = new Vector2(0.0f, 0.5f);
            imageRectTransform.position = pointA;
            float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
            imageRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// Convert from screen to canvas space
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="screenPoint">Screen point</param>
        /// <returns>Canvas space point</returns>
        public static Vector2 ScreenToCanvasPoint(this Canvas canvas, Vector2 screenPoint)
        {
            Vector2 canvasPoint;
            RectTransform contentRectTransform = canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(contentRectTransform, screenPoint,
                (canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera), out canvasPoint);
            Vector2 transformedPoint = canvas.transform.TransformPoint(canvasPoint); // handle scaling or other adjustments set on the canvas
            return transformedPoint;
        }
    }

    /// <summary>
    /// Apply a min/max slider to a field
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        /// <summary>
        /// Min value
        /// </summary>
        public float Min { get; private set; }

        /// <summary>
        /// Max value
        /// </summary>
        public float Max { get; private set; }

        /// <summary>
        /// Constructor with range 0 to 1
        /// </summary>
        public MinMaxSliderAttribute() : this(0.0f, 1.0f) { }

        /// <summary>
        /// Constructor with min and max range
        /// </summary>
        /// <param name="min">Min range</param>
        /// <param name="max">Max range</param>
        public MinMaxSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
