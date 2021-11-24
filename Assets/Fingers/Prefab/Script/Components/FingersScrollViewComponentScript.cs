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
    /// Represents a scroll view, very similar to UIScrollView from UIKit.
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers ScrollView", 6)]
    public class FingersScrollViewComponentScript : MonoBehaviour
    {
        /// <summary>The content to act as a scroll view.</summary>
        [Header("Container")]
        [Tooltip("The content to act as a scroll view.")]
        public GameObject ScrollContent;

        /// <summary>The game object of the element containing the scroll view. This is usually a panel with a transparent image.</summary>
        [Tooltip("The game object of the element containing the scroll view. This is usually a panel with a transparent image.")]
        public GameObject ScrollContentContainer;

        /// <summary>Canvas camera, null if canvas is screen space.</summary>
        [Tooltip("Canvas camera, null if canvas is screen space.")]
        public Camera CanvasCamera;

        /// <summary>The max speed for the scroll view. When a pan finishes, velocity will be applied and the content will move a bit more.</summary>
        [Header("Panning")]
        [Tooltip("The max speed for the scroll view. When a pan finishes, velocity will be applied and the content will move a bit more.")]
        [Range(0.01f, 4096.0f)]
        public float MaxSpeed = 1024.0f;

        /// <summary>How quickly pan velocity reduces when a pan finishes. Lower values reduce faster.</summary>
        [Tooltip("How quickly pan velocity reduces when a pan finishes. Lower values reduce faster.")]
        [Range(0.01f, 0.999f)]
        public float PanDampening = 0.95f;

        /// <summary>How quickly the scale velocity reduces when a scale finishes. Lower values reduce faster.</summary>
        [Header("Scaling")]
        [Tooltip("How quickly the scale velocity reduces when a scale finishes. Lower values reduce faster.")]
        [Range(0.01f, 0.999f)]
        public float ScaleDampening = 0.1f;

        /// <summary>How fast the scale gesture scales in and out. Lower values scale more slowly.</summary>
        [Tooltip("How fast the scale gesture scales in and out. Lower values scale more slowly.")]
        [Range(0.0001f, 0.1f)]
        public float ScaleSpeed = 0.01f;

        /// <summary>The minimum scale multiplier.</summary>
        [Tooltip("The minimum scale multiplier.")]
        [Range(0.01f, 1.0f)]
        public float MinimumScale = 0.1f;

        /// <summary>The maximum scale multiplier.</summary>
        [Tooltip("The maximum scale multiplier.")]
        [Range(0.01f, 100.0f)]
        public float MaximumScale = 8.0f;

        /// <summary>How quickly the content bounces back to the center if it is moved or scaled out of bounds. Higher values move to the center faster.</summary>
        [Tooltip("How quickly the content bounces back to the center if it is moved or scaled out of bounds. Higher values move to the center faster.")]
        [Range(0.01f, 1.0f)]
        public float BounceModifier = 0.2f;

        /// <summary>Whether the scale can bounce a little bit beyond the maximum</summary>
        [Tooltip("Whether the scale can bounce a little bit beyond the maximum")]
        public bool BouncyScale = true;

        /// <summary>Rotation speed, set to 0 for no rotation.</summary>
        [Header("Rotation")]
        [Tooltip("Rotation speed, set to 0 for no rotation.")]
        [Range(-1.0f, 1.0f)]
        public float RotationSpeed = 0.0f;

        /// <summary>How quickly rotation reduces. Lower values reduce faster.</summary>
        [Tooltip("How quickly rotation reduces. Lower values reduce faster.")]
        [Range(0.0f, 1.0f)]
        public float RotateDampening = 0.1f;

        /// <summary>The threshold of zoom scale at which a double tap will zoom out instead of zoom in.</summary>
        [Header("Double Tap")]
        [Tooltip("The threshold of zoom scale at which a double tap will zoom out instead of zoom in.")]
        [Range(1.0f, 10.0f)]
        public float DoubleTapZoomOutThreshold = 2.5f;

        /// <summary>The scale at which a double tap will zoom out to.</summary>
        [Tooltip("The scale at which a double tap will zoom out to.")]
        [Range(0.1f, 10.0f)]
        public float DoubleTapZoomOutValue = 0.5f;

        /// <summary>The scale at which a double tap will zoom in to.</summary>
        [Tooltip("The scale at which a double tap will zoom in to.")]
        [Range(0.1f, 10.0f)]
        public float DoubleTapZoomInValue = 4.0f;

        /// <summary>How long a double tap will animate the zoom in and out.</summary>
        [Tooltip("How long a double tap will animate the zoom in and out.")]
        [Range(0.01f, 3.0f)]
        public float DoubleTapAnimationTimeSeconds = 0.5f;

        /// <summary>Optional, a text element to show debug text, useful for debugging the scroll view.</summary>
        [Header("Debug")]
        [Tooltip("Optional, a text element to show debug text, useful for debugging the scroll view.")]
        public UnityEngine.UI.Text DebugText;

        /// <summary>
        /// Scale gesture for zooming in and out
        /// </summary>
        public ScaleGestureRecognizer ScaleGesture { get; private set; }

        /// <summary>
        /// Rotation gesture
        /// </summary>
        public RotateGestureRecognizer RotateGesture { get; private set; }

        /// <summary>
        /// Pan gesture for moving the scroll view around
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// Double tap gesture to zoom in and out
        /// </summary>
        public TapGestureRecognizer DoubleTapGesture { get; private set; }

        private RectTransform contentRectTransform;
        private RectTransform containerRectTransform;

        // pan animation state
        private Vector2 panVelocity;
        private Vector2 panStart;

        // scale animation state
        private float zoomSpeed = 1.0f;
        private Vector2 lastScaleFocus;

        // rotate animation state
        private float rotateSpeed = 0.0f;

        // double tap animation state
        private float doubleTapScaleTimeSecondsRemaining;
        private float doubleTapScaleStart;
        private float doubleTapScaleEnd;
        private Vector2 doubleTapPosStart;
        private Vector2 doubleTapPosEnd;

        /// <summary>
        /// Get a rect that will be fully visible centered around a center point at a scale
        /// TODO: Add a function to zoom to a rect with animation
        /// </summary>
        /// <param name="scale">Scale</param>
        /// <param name="center">Center point</param>
        /// <returns>Rect that is fully visible at scale and center point</returns>
        public Rect ZoomRectForScaleAndCenter(float scale, Vector2 center)
        {
            Rect zoomRect = new Rect();
            Rect rtRect = contentRectTransform.rect;
            scale = Mathf.Clamp(scale, MinimumScale, MaximumScale);

            // The zoom rect is in the content view's coordinates. 
            // At a zoom scale of 1.0, it would be the size of the content bounds.
            // As the zoom scale decreases, so more content is visible, the size of the rect grows.
            zoomRect.height = rtRect.height / scale;
            zoomRect.width = rtRect.width / scale;

            // choose an origin so as to get the right center.
            zoomRect.xMin = center.x - (zoomRect.width / 2.0f);
            zoomRect.yMin = center.y - (zoomRect.height / 2.0f);

            return zoomRect;
        }

        private void WriteDebug(string text, params object[] args)
        {

#if UNITY_EDITOR

            if (DebugText != null && DebugText.isActiveAndEnabled)
            {
                if (DebugText.text.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None).Length > 38)
                {
                    DebugText.text = string.Empty;
                }
                DebugText.text += string.Format(text + System.Environment.NewLine, args);
            }

#endif

        }

        private void HandleDoubleTap()
        {
            // handle double tap
            if (doubleTapScaleTimeSecondsRemaining > 0.0f)
            {
                doubleTapScaleTimeSecondsRemaining = Mathf.Max(0.0f, doubleTapScaleTimeSecondsRemaining - Time.deltaTime);
                float lerp = 1.0f - (doubleTapScaleTimeSecondsRemaining / DoubleTapAnimationTimeSeconds);
                float scaleValue = Mathf.Lerp(doubleTapScaleStart, doubleTapScaleEnd, lerp);
                contentRectTransform.localScale = new Vector3(scaleValue, scaleValue, 1.0f);
                contentRectTransform.anchoredPosition = Vector2.Lerp(doubleTapPosStart, doubleTapPosEnd, lerp);
            }
        }

        private void HandlePan()
        {
            contentRectTransform.anchoredPosition += (panVelocity * Time.deltaTime);
            panVelocity *= PanDampening;
        }

        private void HandleZoom()
        {
            zoomSpeed = Mathf.Lerp(zoomSpeed, 1.0f, ScaleDampening);

            float scaleMultiplier = ScrollContent.transform.localScale.x * zoomSpeed;
            if (ScaleGesture.State != GestureRecognizerState.Executing)
            {
                // bounce back zoom scale
                if (scaleMultiplier > MaximumScale)
                {
                    scaleMultiplier = Mathf.Lerp(scaleMultiplier, MaximumScale, BounceModifier);
                }
                else if (scaleMultiplier < MinimumScale)
                {
                    scaleMultiplier = Mathf.Lerp(scaleMultiplier, MinimumScale, BounceModifier);
                }
            }
            else if (BouncyScale)
            {
                // clamp to a little beyond the max, will snap back when scale gesture finishes
                scaleMultiplier = Mathf.Clamp(scaleMultiplier, MinimumScale * 0.75f, MaximumScale * 1.333f);
            }
            else
            {
                // no scaling beyond min/max
                scaleMultiplier = Mathf.Clamp(scaleMultiplier, MinimumScale, MaximumScale);
            }

            // keep track of where the scale gesture is in local coordinates and then where it moves to after the scale is applied
            //  we need to adjust position with the difference so that the content stays right where it was under the scale gesture
            Vector2 pointBeforeScale, pointAfterScale;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(contentRectTransform, lastScaleFocus, CanvasCamera, out pointBeforeScale);
            contentRectTransform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1.0f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(contentRectTransform, lastScaleFocus, CanvasCamera, out pointAfterScale);
            contentRectTransform.anchoredPosition += ((pointAfterScale - pointBeforeScale) * scaleMultiplier);
        }

        private void HandleRotate()
        {
            if (rotateSpeed != 0.0f)
            {
                contentRectTransform.Rotate(Vector3.forward, rotateSpeed);
                rotateSpeed = Mathf.Lerp(rotateSpeed, 0.0f, RotateDampening);
            }
        }

        private Rect ScaleRect(Rect rect, float scaleX, float scaleY)
        {
            float newWidth = rect.width * scaleX;
            float newHeight = rect.height * scaleY;
            rect.xMin -= ((newWidth - rect.width) * 0.5f);
            rect.yMin -= ((newHeight - rect.height) * 0.5f);
            rect.width = newWidth;
            rect.height = newHeight;
            return rect;
        }

        private void EnsureVisible()
        {
            // ensure content is visible on screen
            Rect rect = ScaleRect(contentRectTransform.rect, contentRectTransform.localScale.x, contentRectTransform.localScale.y);
            Rect visibleRect = containerRectTransform.rect;
            Vector2 pos = contentRectTransform.anchoredPosition;
            rect.position += pos;

            // handle x pos
            if (rect.width <= visibleRect.width)
            {
                pos.x = visibleRect.center.x;
            }
            else if (rect.xMin > visibleRect.xMin)
            {
                pos.x -= (rect.xMin - visibleRect.xMin);
            }
            else if (rect.xMax < visibleRect.xMax)
            {
                pos.x += (visibleRect.xMax - rect.xMax);
            }

            // handle y pos
            if (rect.height <= visibleRect.height)
            {
                pos.y = visibleRect.center.y;
            }
            else if (rect.yMin > visibleRect.yMin)
            {
                pos.y -= (rect.yMin - visibleRect.yMin);
            }
            else if (rect.yMax < visibleRect.yMax)
            {
                pos.y += (visibleRect.yMax - rect.yMax);
            }

            contentRectTransform.anchoredPosition = Vector2.Lerp(contentRectTransform.anchoredPosition, pos, BounceModifier);
        }

        private void OnEnable()
        {
            contentRectTransform = ScrollContent.GetComponent<RectTransform>();
            containerRectTransform = ScrollContentContainer.GetComponent<RectTransform>();

            // create the scale, tap and pan gestures that will manage the scroll view
            ScaleGesture = new ScaleGestureRecognizer();
            ScaleGesture.StateUpdated += Scale_Updated;
            ScaleGesture.PlatformSpecificView = ScrollContentContainer;
            ScaleGesture.ThresholdUnits = 0.0f; // start zooming immediately

            RotateGesture = new RotateGestureRecognizer();
            RotateGesture.StateUpdated += Rotate_Updated;
            RotateGesture.PlatformSpecificView = ScrollContentContainer;
            RotateGesture.AllowSimultaneousExecution(ScaleGesture);

            DoubleTapGesture = new TapGestureRecognizer();
            DoubleTapGesture.NumberOfTapsRequired = 2;
            DoubleTapGesture.StateUpdated += Tap_Updated;
            DoubleTapGesture.PlatformSpecificView = ScrollContentContainer;

            PanGesture = new PanGestureRecognizer();
            PanGesture.MaximumNumberOfTouchesToTrack = 2;
            PanGesture.StateUpdated += Pan_Updated;
            PanGesture.AllowSimultaneousExecution(ScaleGesture);
            PanGesture.AllowSimultaneousExecution(RotateGesture);
            PanGesture.PlatformSpecificView = ScrollContentContainer;

            FingersScript.Instance.AddGesture(ScaleGesture);
            FingersScript.Instance.AddGesture(RotateGesture);
            FingersScript.Instance.AddGesture(DoubleTapGesture);
            FingersScript.Instance.AddGesture(PanGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(ScaleGesture);
                FingersScript.Instance.RemoveGesture(RotateGesture);
                FingersScript.Instance.RemoveGesture(DoubleTapGesture);
                FingersScript.Instance.RemoveGesture(PanGesture);
            }
        }

        private void LateUpdate()
        {
            HandleDoubleTap();
            HandlePan();
            HandleZoom();
            HandleRotate();
            EnsureVisible();
        }

        private void Tap_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (doubleTapScaleTimeSecondsRemaining == 0.0f && gesture.State == GestureRecognizerState.Ended)
            {
                doubleTapScaleStart = contentRectTransform.localScale.x;
                doubleTapScaleTimeSecondsRemaining = DoubleTapAnimationTimeSeconds;
                if (ScrollContent.transform.localScale.x >= DoubleTapZoomOutThreshold)
                {
                    doubleTapScaleEnd = Mathf.Min(MaximumScale, DoubleTapZoomOutValue);
                }
                else
                {
                    doubleTapScaleEnd = Mathf.Max(MinimumScale, DoubleTapZoomInValue);
                }
                doubleTapPosStart = contentRectTransform.anchoredPosition;
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(contentRectTransform, new Vector2(gesture.FocusX, gesture.FocusY), CanvasCamera, out localPoint);
                doubleTapPosEnd = localPoint * -doubleTapScaleEnd;
            }
        }

        private void Scale_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                float scale = (gesture as ScaleGestureRecognizer).ScaleMultiplier;

                if (scale >= 0.999f && scale <= 1.001f)
                {
                    return;
                }
                else if (scale > 1.0f)
                {
                    zoomSpeed += (scale * ScaleSpeed);
                }
                else if (scale < 1.0f)
                {
                    zoomSpeed -= ((1.0f / scale) * ScaleSpeed);
                }
                lastScaleFocus = new Vector2(gesture.FocusX, gesture.FocusY);
            }

            WriteDebug("Scale: {0},{1}", gesture.State, (gesture as ScaleGestureRecognizer).ScaleMultiplier);
        }

        private void Rotate_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                rotateSpeed += (RotateGesture.RotationDegreesDelta * RotationSpeed);
            }
        }

        private void Pan_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began)
            {
                panStart = contentRectTransform.anchoredPosition;
            }
            else if (gesture.State == GestureRecognizerState.Executing)
            {
                Vector2 zero;
                Vector2 offset;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRectTransform, Vector2.zero, CanvasCamera, out zero);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRectTransform, new Vector2(gesture.DistanceX, gesture.DistanceY), CanvasCamera, out offset);
                contentRectTransform.anchoredPosition = panStart + offset - zero;
            }
            else if (gesture.State == GestureRecognizerState.Ended)
            {
                panVelocity = new Vector2(gesture.VelocityX, gesture.VelocityY);
                panVelocity.x = Mathf.Clamp(panVelocity.x, -MaxSpeed, MaxSpeed);
                panVelocity.y = Mathf.Clamp(panVelocity.y, -MaxSpeed, MaxSpeed);
            }
        }
    }
}