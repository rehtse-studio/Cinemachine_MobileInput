//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DigitalRubyShared
{
    /// <summary>
    /// The main Unity fingers script - only one of these should exist. FingersScript.Instance gives you easy access to add and remove gestures.
    /// </summary>
    public class FingersScript : MonoBehaviour
    {
        /// <summary>
        /// Gesture mask
        /// </summary>
        private class GestureMask
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="collider">Collider</param>
            /// <param name="gestures">Gestures</param>
            public GestureMask(Collider2D collider, params GestureRecognizer[] gestures)
            {
                Collider = collider;
                Gestures = new List<GestureRecognizer>(gestures);
            }

            /// <summary>
            /// Collider
            /// </summary>
            public Collider2D Collider { get; private set; }

            /// <summary>
            /// List of gesture recognizers
            /// </summary>
            public List<GestureRecognizer> Gestures { get; private set; }
        }

        /// <summary>Whether to enable Unity input multi-touch automatically.</summary>
        [Header("Mouse and Touch Settings")]
        [Tooltip("Whether to enable Unity input multi-touch automatically.")]
        public bool EnableMultiTouch = true;

        /// <summary>True to treat the mouse as a finger, false otherwise. Left, middle and right mouse buttons can be used as individual fingers and will all have the same location.</summary>
        [Tooltip("True to treat the mouse as a finger, false otherwise. Left, middle and right mouse buttons can be used as individual fingers and will all have the same location.")]
        public bool TreatMousePointerAsFinger = true;

        /// <summary>True to treat the mouse wheel as two fingers for rotation and scaling, false otherwise.</summary>
        [Tooltip("True to treat the mouse wheel as two fingers for rotation and scaling, false otherwise.")]
        public bool TreatMouseWheelAsFingers = true;

        /// <summary>Whether to treat touches as mouse pointer? This needs to be set before the script Awake method is called.</summary>
        [Tooltip("Whether to treat touches as mouse pointer? This needs to be set before the script Awake method is called.")]
        public bool SimulateMouseWithTouches;

        /// <summary>Whether to process Unity touch events. If false, you will need to implement the VirtualTouch* methods to implement touch handling.</summary>
        [Tooltip("Whether to process Unity touch events. If false, you will need to implement the VirtualTouch* methods to implement touch handling.")]
        public bool ProcessUnityTouches = true;

        /// <summary>Whether the control key is required for mouse zoom. If true, control pluse mouse wheel zooms. If false, mouse wheel alone will zoom.</summary>
        [Tooltip("Whether the control key is required for mouse zoom. If true, control pluse mouse wheel zooms. If false, mouse wheel alone will zoom.")]
        public bool RequireControlKeyForMouseZoom = true;

        /// <summary>The distance (in units, default is inches) for simulated fingers to start at for a mouse zoom or rotate.</summary>
        [Tooltip("The distance (in units, default is inches) for simulated fingers to start at for a mouse zoom or rotate.")]
        [Range(0.1f, 10.0f)]
        public float MouseDistanceInUnitsForScaleAndRotate = 2.0f;

        /// <summary>Mouse wheel delta multiplier.</summary>
        [Tooltip("Mouse wheel delta multiplier.")]
        [Range(0.0001f, 1.0f)]
        public float MouseWheelDeltaMultiplier = 0.025f;

        /// <summary>Mouse wheel snap back option. X is distance to snap back at in units, Y is distance to snap back to in units.</summary>
        [Tooltip("Mouse wheel snap back option. X is distance to snap back at in units, Y is distance to snap back to in units.")]
        public Vector2 MouseWheelZoomOutSnapBack;

        /// <summary>Objects that should pass gestures through. By default, some UI components block gestures, such as Panel, Button, Dropdown, etc. See the SetupDefaultPassThroughComponents method for the full list of defaults.</summary>
        [Header("Other Settings")]
        [Tooltip("Objects that should pass gestures through. By default, some UI components block gestures, such as Panel, Button, Dropdown, etc. See the SetupDefaultPassThroughComponents method for " +
            "the full list of defaults.")]
        public List<GameObject> PassThroughObjects;

        /// <summary>Whether to auto-add required components like physics raycasters, event system, etc. if they are missing.</summary>
        [Tooltip("Whether to auto-add required components like physics raycasters, event system, etc. if they are missing.")]
        public bool AutoAddRequiredComponents = true;

        /// <summary>Whether to show touches using the TouchCircles array. Make sure to turn this off before releasing your game or app.</summary>
        [Tooltip("Whether to show touches using the TouchCircles array. Make sure to turn this off before releasing your game or app.")]
        public bool ShowTouches;

        /// <summary>If ShowTouches is true, this array is used to show the touches. The FingersScriptPrefab sets this up as 10 circles.</summary>
        [Tooltip("If ShowTouches is true, this array is used to show the touches. The FingersScriptPrefab sets this up as 10 circles.")]
        public GameObject[] TouchCircles;
        private GameObject[] origTouchCircles;

        /// <summary>The default DPI to use if the DPI cannot be determined</summary>
        [Tooltip("The default DPI to use if the DPI cannot be determined")]
        public int DefaultDPI = 200;

        /// <summary>Allows resetting state (keeps the gestures, just resets them) or clearing all gestures when a level is unloaded.</summary>
        [Tooltip("Allows resetting state (keeps the gestures, just resets them) or clearing all gestures when a level is unloaded.")]
        public GestureLevelUnloadOption LevelUnloadOption = GestureLevelUnloadOption.ClearAllGestures;

        /// <summary>Whether to use fixed update instead of update. Using fixed update can result in slightly more responsibe input, depending on project settings -> time -> fixed update rate.</summary>
        [Tooltip("Whether to use fixed update instead of update. Using fixed update can result in slightly more responsibe input, depending on project settings -> time -> fixed update rate.")]
        public bool UseFixedUpdate;

        /// <summary>
        /// Whether to record touches. Recorded touches are put into TestTouches where they can be copied and pasted once you exit play mode. Use copy component and paste component values.
        /// </summary>
        [Header("Testing")]
        [Tooltip("Whether to record touches. Recorded touches are put into TestTouches where they can be copied and pasted once you exit play mode. Use copy component and paste component values.")]
        public bool RecordTouches;

        /// <summary>
        /// Fake touches, for testing and automation
        /// </summary>
        [Tooltip("Fake touches, for testing and automation.")]
        public List<FakeTouch> TestTouches = new List<FakeTouch>();
        private float accumulatedTime;
        private Dictionary<int, GestureTouch> fakeTouchesInProgress = new Dictionary<int, GestureTouch>();

        /// <summary>Mask that will restrict gestures to the collider area(s). The colliders MUST be in a canvas. Leave empty for no mask. Leave gesture recognizer list empty for all gestures.</summary>
        private readonly List<GestureMask> masks = new List<GestureMask>();

        /// <summary>
        /// Gesture level unload option
        /// </summary>
        public enum GestureLevelUnloadOption
        {
            /// <summary>
            /// Do nothing
            /// </summary>
            Nothing,

            /// <summary>
            /// Reset all gesture states on level load
            /// </summary>
            ResetGestureState,

            /// <summary>
            /// Clear out all gestures on level load
            /// </summary>
            ClearAllGestures
        }

        /// <summary>
        /// Id for mouse pointer 0
        /// </summary>
        public const int MousePointerId0 = int.MaxValue - 2;

        /// <summary>
        /// Id for mouse pointer 1
        /// </summary>
        public const int MousePointerId1 = int.MaxValue - 3;

        /// <summary>
        /// Id for mouse pointer 2
        /// </summary>
        public const int MousePointerId2 = int.MaxValue - 4;

        /// <summary>
        /// First id for virtual rotate/pinch gesture using mouse
        /// </summary>
        public const int MouseRotatePinchId0 = int.MaxValue - 5;

        /// <summary>
        /// Second id for virtual rotate/pinch gesture using mouse
        /// </summary>
        public const int MouseRotatePinchId1 = int.MaxValue - 6;

        private readonly List<DigitalRubyShared.GestureRecognizer> gestures = new List<DigitalRubyShared.GestureRecognizer>();
        private readonly List<DigitalRubyShared.GestureRecognizer> gesturesTemp = new List<DigitalRubyShared.GestureRecognizer>();
        private readonly List<GestureTouch> touchesBegan = new List<GestureTouch>();
        private readonly List<GestureTouch> touchesMoved = new List<GestureTouch>();
        private readonly List<GestureTouch> touchesEnded = new List<GestureTouch>();
        private readonly Dictionary<int, List<GameObject>> gameObjectsForTouch = new Dictionary<int, List<GameObject>>();
        private readonly List<RaycastResult> captureRaycastResults = new List<RaycastResult>();
        private readonly List<GestureTouch> filteredTouches = new List<GestureTouch>();
        private readonly List<GestureTouch> touches = new List<GestureTouch>();
        private readonly List<GameObject> availableShowTouches = new List<GameObject>();
        private readonly Dictionary<int, ShownTouch> shownTouches = new Dictionary<int, ShownTouch>();
        private readonly List<int> shownTouchesToRemove = new List<int>();
        private readonly Dictionary<int, Vector2> previousTouchPositions = new Dictionary<int, Vector2>();
        private readonly List<Component> components = new List<Component>();
        private readonly HashSet<System.Type> componentTypesToDenyPassThrough = new HashSet<System.Type>();
        private readonly HashSet<System.Type> componentTypesToIgnorePassThrough = new HashSet<System.Type>();
        private readonly Collider2D[] hackResults = new Collider2D[128];
        private readonly List<GestureTouch> previousTouches = new List<GestureTouch>();
        private readonly List<GestureTouch> currentTouches = new List<GestureTouch>();
        private readonly HashSet<GestureTouch> tempTouches = new HashSet<GestureTouch>();

        private float rotateAngle;
        private float pinchScale = 1.0f;
        private GestureTouch rotatePinch1;
        private GestureTouch rotatePinch2;
        private System.DateTime lastMouseWheelTime;
        private Vector2 mousePosition;
        private int touchCount;

#if UNITY_INPUT_SYSTEM_V2

        private static readonly Dictionary<UnityEngine.KeyCode, UnityEngine.InputSystem.Key> keyCodeMappings = new Dictionary<KeyCode, UnityEngine.InputSystem.Key>();

#endif

        private static FingersScript singleton;

        private enum CaptureResult
        {
            /// <summary>
            /// Force the gesture to pass through
            /// </summary>
            ForcePassThrough,

            /// <summary>
            /// Force the gesture to be denied unless the platform specific view matches
            /// </summary>
            ForceDenyPassThrough,

            /// <summary>
            /// Do not force or deny the pass through
            /// </summary>
            Default,

            /// <summary>
            /// Pretend this object doesn't exist
            /// </summary>
            Ignore
        }

        private struct ShownTouch
        {
            public GameObject GameObject { get; set; }
            public float Timestamp { get; set; }
        }

        static FingersScript()
        {

#if UNITY_INPUT_SYSTEM_V2

            keyCodeMappings.Add(UnityEngine.KeyCode.None, UnityEngine.InputSystem.Key.None);
            keyCodeMappings.Add(UnityEngine.KeyCode.Backspace, UnityEngine.InputSystem.Key.Backspace);
            keyCodeMappings.Add(UnityEngine.KeyCode.Tab, UnityEngine.InputSystem.Key.Tab);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Clear, UnityEngine.InputSystem.Key.Clear);
            keyCodeMappings.Add(UnityEngine.KeyCode.Return, UnityEngine.InputSystem.Key.Enter);
            keyCodeMappings.Add(UnityEngine.KeyCode.Pause, UnityEngine.InputSystem.Key.Pause);
            keyCodeMappings.Add(UnityEngine.KeyCode.Escape, UnityEngine.InputSystem.Key.Escape);
            keyCodeMappings.Add(UnityEngine.KeyCode.Space, UnityEngine.InputSystem.Key.Space);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Exclaim, UnityEngine.InputSystem.Key.Exclaim);
            //keyCodeMappings.Add(UnityEngine.KeyCode.DoubleQuote, UnityEngine.InputSystem.Key.DoubleQuote);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Hash, UnityEngine.InputSystem.Key.Hash);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Dollar, UnityEngine.InputSystem.Key.Dollar);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Percent, UnityEngine.InputSystem.Key.Percent);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Ampersand, UnityEngine.InputSystem.Key.Ampersand);
            keyCodeMappings.Add(UnityEngine.KeyCode.Quote, UnityEngine.InputSystem.Key.Quote);
            //keyCodeMappings.Add(UnityEngine.KeyCode.LeftParen, UnityEngine.InputSystem.Key.LeftParen);
            //keyCodeMappings.Add(UnityEngine.KeyCode.RightParen, UnityEngine.InputSystem.Key.RightParen);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Asterisk, UnityEngine.InputSystem.Key.Asterisk);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Plus, UnityEngine.InputSystem.Key.Plus);
            keyCodeMappings.Add(UnityEngine.KeyCode.Comma, UnityEngine.InputSystem.Key.Comma);
            keyCodeMappings.Add(UnityEngine.KeyCode.Minus, UnityEngine.InputSystem.Key.Minus);
            keyCodeMappings.Add(UnityEngine.KeyCode.Period, UnityEngine.InputSystem.Key.Period);
            keyCodeMappings.Add(UnityEngine.KeyCode.Slash, UnityEngine.InputSystem.Key.Slash);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha0, UnityEngine.InputSystem.Key.Digit0);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha1, UnityEngine.InputSystem.Key.Digit1);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha2, UnityEngine.InputSystem.Key.Digit2);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha3, UnityEngine.InputSystem.Key.Digit3);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha4, UnityEngine.InputSystem.Key.Digit4);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha5, UnityEngine.InputSystem.Key.Digit5);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha6, UnityEngine.InputSystem.Key.Digit6);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha7, UnityEngine.InputSystem.Key.Digit7);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha8, UnityEngine.InputSystem.Key.Digit8);
            keyCodeMappings.Add(UnityEngine.KeyCode.Alpha9, UnityEngine.InputSystem.Key.Digit9);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Colon, UnityEngine.InputSystem.Key.Colon);
            keyCodeMappings.Add(UnityEngine.KeyCode.Semicolon, UnityEngine.InputSystem.Key.Semicolon);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Less, UnityEngine.InputSystem.Key.Less);
            keyCodeMappings.Add(UnityEngine.KeyCode.Equals, UnityEngine.InputSystem.Key.Equals);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Greater, UnityEngine.InputSystem.Key.Greater);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Question, UnityEngine.InputSystem.Key.Question);
            //keyCodeMappings.Add(UnityEngine.KeyCode.At, UnityEngine.InputSystem.Key.At);
            keyCodeMappings.Add(UnityEngine.KeyCode.LeftBracket, UnityEngine.InputSystem.Key.LeftBracket);
            keyCodeMappings.Add(UnityEngine.KeyCode.Backslash, UnityEngine.InputSystem.Key.Backslash);
            keyCodeMappings.Add(UnityEngine.KeyCode.RightBracket, UnityEngine.InputSystem.Key.RightBracket);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Caret, UnityEngine.InputSystem.Key.Caret);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Underscore, UnityEngine.InputSystem.Key.Underscore);
            //keyCodeMappings.Add(UnityEngine.KeyCode.BackQuote, UnityEngine.InputSystem.Key.BackQuote);
            keyCodeMappings.Add(UnityEngine.KeyCode.A, UnityEngine.InputSystem.Key.A);
            keyCodeMappings.Add(UnityEngine.KeyCode.B, UnityEngine.InputSystem.Key.B);
            keyCodeMappings.Add(UnityEngine.KeyCode.C, UnityEngine.InputSystem.Key.C);
            keyCodeMappings.Add(UnityEngine.KeyCode.D, UnityEngine.InputSystem.Key.D);
            keyCodeMappings.Add(UnityEngine.KeyCode.E, UnityEngine.InputSystem.Key.E);
            keyCodeMappings.Add(UnityEngine.KeyCode.F, UnityEngine.InputSystem.Key.F);
            keyCodeMappings.Add(UnityEngine.KeyCode.G, UnityEngine.InputSystem.Key.G);
            keyCodeMappings.Add(UnityEngine.KeyCode.H, UnityEngine.InputSystem.Key.H);
            keyCodeMappings.Add(UnityEngine.KeyCode.I, UnityEngine.InputSystem.Key.I);
            keyCodeMappings.Add(UnityEngine.KeyCode.J, UnityEngine.InputSystem.Key.J);
            keyCodeMappings.Add(UnityEngine.KeyCode.K, UnityEngine.InputSystem.Key.K);
            keyCodeMappings.Add(UnityEngine.KeyCode.L, UnityEngine.InputSystem.Key.L);
            keyCodeMappings.Add(UnityEngine.KeyCode.M, UnityEngine.InputSystem.Key.M);
            keyCodeMappings.Add(UnityEngine.KeyCode.N, UnityEngine.InputSystem.Key.N);
            keyCodeMappings.Add(UnityEngine.KeyCode.O, UnityEngine.InputSystem.Key.O);
            keyCodeMappings.Add(UnityEngine.KeyCode.P, UnityEngine.InputSystem.Key.P);
            keyCodeMappings.Add(UnityEngine.KeyCode.Q, UnityEngine.InputSystem.Key.Q);
            keyCodeMappings.Add(UnityEngine.KeyCode.R, UnityEngine.InputSystem.Key.R);
            keyCodeMappings.Add(UnityEngine.KeyCode.S, UnityEngine.InputSystem.Key.S);
            keyCodeMappings.Add(UnityEngine.KeyCode.T, UnityEngine.InputSystem.Key.T);
            keyCodeMappings.Add(UnityEngine.KeyCode.U, UnityEngine.InputSystem.Key.U);
            keyCodeMappings.Add(UnityEngine.KeyCode.V, UnityEngine.InputSystem.Key.V);
            keyCodeMappings.Add(UnityEngine.KeyCode.W, UnityEngine.InputSystem.Key.W);
            keyCodeMappings.Add(UnityEngine.KeyCode.X, UnityEngine.InputSystem.Key.X);
            keyCodeMappings.Add(UnityEngine.KeyCode.Y, UnityEngine.InputSystem.Key.Y);
            keyCodeMappings.Add(UnityEngine.KeyCode.Z, UnityEngine.InputSystem.Key.Z);
            //keyCodeMappings.Add(UnityEngine.KeyCode.LeftCurlyBracket, UnityEngine.InputSystem.Key.LeftCurlyBracket);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Pipe, UnityEngine.InputSystem.Key.Pipe);
            //keyCodeMappings.Add(UnityEngine.KeyCode.RightCurlyBracket, UnityEngine.InputSystem.Key.RightCurlyBracket);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Tilde, UnityEngine.InputSystem.Key.Tilde);
            keyCodeMappings.Add(UnityEngine.KeyCode.Delete, UnityEngine.InputSystem.Key.Delete);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad0, UnityEngine.InputSystem.Key.Numpad0);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad1, UnityEngine.InputSystem.Key.Numpad1);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad2, UnityEngine.InputSystem.Key.Numpad2);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad3, UnityEngine.InputSystem.Key.Numpad3);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad4, UnityEngine.InputSystem.Key.Numpad4);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad5, UnityEngine.InputSystem.Key.Numpad5);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad6, UnityEngine.InputSystem.Key.Numpad6);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad7, UnityEngine.InputSystem.Key.Numpad7);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad8, UnityEngine.InputSystem.Key.Numpad8);
            keyCodeMappings.Add(UnityEngine.KeyCode.Keypad9, UnityEngine.InputSystem.Key.Numpad9);
            //keyCodeMappings.Add(UnityEngine.KeyCode.KeypadPeriod, UnityEngine.InputSystem.Key.KeypadPeriod);
            //keyCodeMappings.Add(UnityEngine.KeyCode.KeypadDivide, UnityEngine.InputSystem.Key.KeypadDivide);
            //keyCodeMappings.Add(UnityEngine.KeyCode.KeypadMultiply, UnityEngine.InputSystem.Key.KeypadMultiply);
            //keyCodeMappings.Add(UnityEngine.KeyCode.KeypadMinus, UnityEngine.InputSystem.Key.KeypadMinus);
            //keyCodeMappings.Add(UnityEngine.KeyCode.KeypadPlus, UnityEngine.InputSystem.Key.KeypadPlus);
            //keyCodeMappings.Add(UnityEngine.KeyCode.KeypadEnter, UnityEngine.InputSystem.Key.KeypadEnter);
            //keyCodeMappings.Add(UnityEngine.KeyCode.KeypadEquals, UnityEngine.InputSystem.Key.KeypadEquals);
            keyCodeMappings.Add(UnityEngine.KeyCode.UpArrow, UnityEngine.InputSystem.Key.UpArrow);
            keyCodeMappings.Add(UnityEngine.KeyCode.DownArrow, UnityEngine.InputSystem.Key.DownArrow);
            keyCodeMappings.Add(UnityEngine.KeyCode.RightArrow, UnityEngine.InputSystem.Key.RightArrow);
            keyCodeMappings.Add(UnityEngine.KeyCode.LeftArrow, UnityEngine.InputSystem.Key.LeftArrow);
            keyCodeMappings.Add(UnityEngine.KeyCode.Insert, UnityEngine.InputSystem.Key.Insert);
            keyCodeMappings.Add(UnityEngine.KeyCode.Home, UnityEngine.InputSystem.Key.Home);
            keyCodeMappings.Add(UnityEngine.KeyCode.End, UnityEngine.InputSystem.Key.End);
            keyCodeMappings.Add(UnityEngine.KeyCode.PageUp, UnityEngine.InputSystem.Key.PageUp);
            keyCodeMappings.Add(UnityEngine.KeyCode.PageDown, UnityEngine.InputSystem.Key.PageDown);
            keyCodeMappings.Add(UnityEngine.KeyCode.F1, UnityEngine.InputSystem.Key.F1);
            keyCodeMappings.Add(UnityEngine.KeyCode.F2, UnityEngine.InputSystem.Key.F2);
            keyCodeMappings.Add(UnityEngine.KeyCode.F3, UnityEngine.InputSystem.Key.F3);
            keyCodeMappings.Add(UnityEngine.KeyCode.F4, UnityEngine.InputSystem.Key.F4);
            keyCodeMappings.Add(UnityEngine.KeyCode.F5, UnityEngine.InputSystem.Key.F5);
            keyCodeMappings.Add(UnityEngine.KeyCode.F6, UnityEngine.InputSystem.Key.F6);
            keyCodeMappings.Add(UnityEngine.KeyCode.F7, UnityEngine.InputSystem.Key.F7);
            keyCodeMappings.Add(UnityEngine.KeyCode.F8, UnityEngine.InputSystem.Key.F8);
            keyCodeMappings.Add(UnityEngine.KeyCode.F9, UnityEngine.InputSystem.Key.F9);
            keyCodeMappings.Add(UnityEngine.KeyCode.F10, UnityEngine.InputSystem.Key.F10);
            keyCodeMappings.Add(UnityEngine.KeyCode.F11, UnityEngine.InputSystem.Key.F11);
            keyCodeMappings.Add(UnityEngine.KeyCode.F12, UnityEngine.InputSystem.Key.F12);
            //keyCodeMappings.Add(UnityEngine.KeyCode.F13, UnityEngine.InputSystem.Key.F13);
            //keyCodeMappings.Add(UnityEngine.KeyCode.F14, UnityEngine.InputSystem.Key.F14);
            //keyCodeMappings.Add(UnityEngine.KeyCode.F15, UnityEngine.InputSystem.Key.F15);
            keyCodeMappings.Add(UnityEngine.KeyCode.Numlock, UnityEngine.InputSystem.Key.NumLock);
            keyCodeMappings.Add(UnityEngine.KeyCode.CapsLock, UnityEngine.InputSystem.Key.CapsLock);
            keyCodeMappings.Add(UnityEngine.KeyCode.ScrollLock, UnityEngine.InputSystem.Key.ScrollLock);
            keyCodeMappings.Add(UnityEngine.KeyCode.RightShift, UnityEngine.InputSystem.Key.RightShift);
            keyCodeMappings.Add(UnityEngine.KeyCode.LeftShift, UnityEngine.InputSystem.Key.LeftShift);
            keyCodeMappings.Add(UnityEngine.KeyCode.RightControl, UnityEngine.InputSystem.Key.RightCtrl);
            keyCodeMappings.Add(UnityEngine.KeyCode.LeftControl, UnityEngine.InputSystem.Key.LeftCtrl);
            keyCodeMappings.Add(UnityEngine.KeyCode.RightAlt, UnityEngine.InputSystem.Key.RightAlt);
            keyCodeMappings.Add(UnityEngine.KeyCode.LeftAlt, UnityEngine.InputSystem.Key.LeftAlt);
            keyCodeMappings.Add(UnityEngine.KeyCode.RightCommand, UnityEngine.InputSystem.Key.RightCommand);
            //keyCodeMappings.Add(UnityEngine.KeyCode.RightApple, UnityEngine.InputSystem.Key.RightApple);
            keyCodeMappings.Add(UnityEngine.KeyCode.LeftCommand, UnityEngine.InputSystem.Key.LeftCommand);
            //keyCodeMappings.Add(UnityEngine.KeyCode.LeftApple, UnityEngine.InputSystem.Key.LeftApple);
            keyCodeMappings.Add(UnityEngine.KeyCode.LeftWindows, UnityEngine.InputSystem.Key.LeftWindows);
            keyCodeMappings.Add(UnityEngine.KeyCode.RightWindows, UnityEngine.InputSystem.Key.RightWindows);
            keyCodeMappings.Add(UnityEngine.KeyCode.AltGr, UnityEngine.InputSystem.Key.AltGr);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Help, UnityEngine.InputSystem.Key.Help);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Print, UnityEngine.InputSystem.Key.Print);
            //keyCodeMappings.Add(UnityEngine.KeyCode.SysReq, UnityEngine.InputSystem.Key.SysReq);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Break, UnityEngine.InputSystem.Key.Break);
            //keyCodeMappings.Add(UnityEngine.KeyCode.Menu, UnityEngine.InputSystem.Key.Menu);

#endif

        }

        private IEnumerator MainThreadCallback(float delay, System.Action action)
        {
            if (action != null)
            {
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                yield return null;
                while ((float)timer.Elapsed.TotalSeconds < delay)
                {
                    yield return null;
                }
                action();
            }
        }

        private CaptureResult ShouldCaptureGesture(GameObject obj, out bool platformSpecificView)
        {
            platformSpecificView = false;

            if (obj == null)
            {
                return CaptureResult.Default;
            }

            // if we have a capture gesture handler, perform a check to see if the user has custom pass through logic
            else if (CaptureGestureHandler != null)
            {
                bool? tmp = CaptureGestureHandler(obj);
                if (tmp != null)
                {
                    // user has decided on pass through, stop the loop
                    return (tmp.Value ? CaptureResult.ForceDenyPassThrough : CaptureResult.ForcePassThrough);
                }
            }

            // check pass through objects, these always pass the gesture through
            if (PassThroughObjects.Contains(obj))
            {
                // allow the gesture to pass through, do not capture it and stop the loop
                return CaptureResult.ForcePassThrough;
            }
            else
            {
                // if any gesture has a platform specific view that matches the object, use default behavior
                foreach (DigitalRubyShared.GestureRecognizer gesture in gestures)
                {
                    if (object.ReferenceEquals(gesture.PlatformSpecificView, obj))
                    {
                        platformSpecificView = true;
                        return CaptureResult.Default;
                    }
                }
            }

            obj.GetComponents<Component>(components);

            try
            {
                System.Type type;
                foreach (Component c in components)
                {
                    if (c != null)
                    {
                        type = c.GetType();
                        if (componentTypesToDenyPassThrough.Contains(type))
                        {
                            return CaptureResult.ForceDenyPassThrough;
                        }
                        else if (componentTypesToIgnorePassThrough.Contains(type))
                        {
                            return CaptureResult.Ignore;
                        }
                    }
                }
            }
            finally
            {
                components.Clear();
            }

            // default is for input UI elements (elements that normally block touches) to not pass through
            return CaptureResult.Default;
        }

        private void PopulateGameObjectsForTouch(int pointerId, float x, float y)
        {
            // Find a game object for a touch id
            if (EventSystem.current == null)
            {
                return;
            }

            List<GameObject> list;
            if (gameObjectsForTouch.TryGetValue(pointerId, out list))
            {
                list.Clear();
            }
            else
            {
                list = new List<GameObject>();
                gameObjectsForTouch[pointerId] = list;
            }

            captureRaycastResults.Clear();
            PointerEventData p = new PointerEventData(EventSystem.current);
            p.Reset();
            p.position = new Vector2(x, y);
            p.clickCount = 1;
            EventSystem.current.RaycastAll(p, captureRaycastResults);

            // HACK: Unity doesn't get collider2d on UI element, get those now
            int hackCount = Physics2D.OverlapPointNonAlloc(p.position, hackResults);
            for (int i = 0; i < hackCount; i++)
            {
                RaycastResult result = new RaycastResult { gameObject = hackResults[i].gameObject };
                if (captureRaycastResults.FindIndex((cmp) =>
                {
                    return cmp.gameObject == result.gameObject;
                }) < 0)
                {
                    captureRaycastResults.Add(result);
                }
            }
            System.Array.Clear(hackResults, 0, hackCount);

            if (captureRaycastResults.Count == 0)
            {
                captureRaycastResults.Add(new RaycastResult());
            }

            // determine what game object, if any should capture the gesture
            bool forcePassThrough = false;
            bool platformSpecificView;
            foreach (RaycastResult r in captureRaycastResults)
            {
                switch (ShouldCaptureGesture(r.gameObject, out platformSpecificView))
                {
                    case CaptureResult.ForcePassThrough:
                        forcePassThrough = true;
                        break;

                    case CaptureResult.ForceDenyPassThrough:
                        // if forcing pass through, prefer that over this behavior
                        if (!forcePassThrough)
                        {
                            // unless a platform specific view matches, deny the gesture
                            // this specific object stops any further pass through checks underneath
                            list.Add(r.gameObject);
                            return;
                        } break;

                    case CaptureResult.Default:
                        // if forcing pass through, only add if matches a platform specific view
                        if (forcePassThrough)
                        {
                            bool matchesPlatformSpecificView = false;
                            foreach (DigitalRubyShared.GestureRecognizer g in gestures)
                            {
                                if (object.ReferenceEquals(g.PlatformSpecificView, r.gameObject))
                                {
                                    matchesPlatformSpecificView = platformSpecificView = true;
                                    break;
                                }
                            }
                            if (matchesPlatformSpecificView)
                            {
                                list.Add(r.gameObject);
                            } // else ignore
                        }
                        else
                        {
                            list.Add(r.gameObject);
                        }
                        break;

                    default:
                        break;
                }

                // once we have a platform specific view match, we are done
                if (platformSpecificView)
                {
                    break;
                }
            }
        }

#if UNITY_INPUT_SYSTEM_V2

        private GestureTouch GestureTouchFromTouch(UnityEngine.InputSystem.Controls.TouchControl t,
            UnityEngine.InputSystem.TouchPhase unityPhase)
        {
            // convert Unity touch to Gesture touch
            Vector2 prev;
            int id = t.touchId.ReadValue();
            int deviceId = t.device.deviceId;
            float x = t.position.x.ReadValue();
            float y = t.position.y.ReadValue();
            float p = t.pressure.ReadValue();
            if (!previousTouchPositions.TryGetValue(id, out prev))
            {
                prev.x = x;
                prev.y = y;
            }
            DigitalRubyShared.TouchPhase phase;
            switch (unityPhase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    phase = TouchPhase.Began;
                    break;

                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    phase = TouchPhase.Cancelled;
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                    phase = TouchPhase.Ended;
                    break;

                case UnityEngine.InputSystem.TouchPhase.Moved:
                    phase = TouchPhase.Moved;
                    break;

                case UnityEngine.InputSystem.TouchPhase.Stationary:
                    phase = TouchPhase.Stationary;
                    break;

                default:
                    phase = TouchPhase.Unknown;
                    break;
            }
            GestureTouch touch = new GestureTouch(id, x, y, prev.x, prev.y, p, t, phase, deviceId);
            prev.x = x;
            prev.y = y;
            previousTouchPositions[id] = prev;
            return touch;
        }

#else

        private GestureTouch GestureTouchFromTouch(ref UnityEngine.Touch t)
        {
            // convert Unity touch to Gesture touch
            Vector2 prev;
            if (!previousTouchPositions.TryGetValue(t.fingerId, out prev))
            {
                prev.x = t.position.x;
                prev.y = t.position.y;
            }
            TouchPhase phase;
            switch (t.phase)
            {
                case UnityEngine.TouchPhase.Began:
                    phase = TouchPhase.Began;
                    break;

                case UnityEngine.TouchPhase.Canceled:
                    phase = TouchPhase.Cancelled;
                    break;

                case UnityEngine.TouchPhase.Ended:
                    phase = TouchPhase.Ended;
                    break;

                case UnityEngine.TouchPhase.Moved:
                    phase = TouchPhase.Moved;
                    break;

                case UnityEngine.TouchPhase.Stationary:
                    phase = TouchPhase.Stationary;
                    break;

                default:
                    phase = TouchPhase.Unknown;
                    break;
            }
            GestureTouch touch = new GestureTouch(t.fingerId, t.position.x, t.position.y, prev.x, prev.y, t.pressure, t, phase);
            prev.x = t.position.x;
            prev.y = t.position.y;
            previousTouchPositions[t.fingerId] = prev;
            return touch;
        }

#endif

        private void FingersBeginTouch(ref GestureTouch g)
        {
            if (!previousTouches.Contains(g))
            {
                previousTouches.Add(g);
            }
            touchesBegan.Add(g);
            previousTouchPositions[g.Id] = new Vector2(g.X, g.Y);
        }

        private void FingersContinueTouch(ref GestureTouch g)
        {
            touchesMoved.Add(g);
            previousTouchPositions[g.Id] = new Vector2(g.X, g.Y);
        }

        private void FingersEndTouch(ref GestureTouch g, bool lost = false)
        {
            if (!lost)
            {
                touchesEnded.Add(g);
            }
            previousTouchPositions.Remove(g.Id);
            previousTouches.Remove(g);
        }

        private void FingersProcessTouch(ref GestureTouch g)
        {

#if UNITY_EDITOR

            if (RecordTouches)
            {
                TestTouches.Add(new FakeTouch
                {
                    Id = g.Id,
                    Phase = g.TouchPhase,
                    ScreenPosition = new Vector2(g.X, g.Y),
                    Time = Time.timeSinceLevelLoad
                });
            }

#endif

            currentTouches.Add(g);

            // do our own touch up / down tracking, the user can reset touch state so that touches can begin again without a finger being lifted
            if (g.TouchPhase == TouchPhase.Moved || g.TouchPhase == TouchPhase.Stationary)
            {
                FingersContinueTouch(ref g);
            }
            else if (g.TouchPhase == TouchPhase.Began)
            {
                FingersBeginTouch(ref g);
            }
            else
            {
                FingersEndTouch(ref g);
            }

            // string d = string.Format ("Touch: {0} {1}", t.position, t.phase);
            // Debug.Log (d);
        }

#if UNITY_INPUT_SYSTEM_V2

        private UnityEngine.InputSystem.Controls.ButtonControl GetMouseButton(int index)
        {
            UnityEngine.InputSystem.Controls.ButtonControl mouseButton;
            switch (index)
            {
                case 0: mouseButton = UnityEngine.InputSystem.Mouse.current.leftButton; break;
                case 1: mouseButton = UnityEngine.InputSystem.Mouse.current.middleButton; break;
                case 2: mouseButton = UnityEngine.InputSystem.Mouse.current.rightButton; break;
                default: throw new System.ArgumentException("Invalid mouse button index " + index);
            }
            return mouseButton;
        }

#endif


        private void AddMouseTouch(int index, int pointerId)
        {
            TouchPhase phase;

#if UNITY_INPUT_SYSTEM_V2

            float x = UnityEngine.InputSystem.Mouse.current.position.x.ReadValue();
            float y = UnityEngine.InputSystem.Mouse.current.position.y.ReadValue();
            mousePosition = new Vector2(x, y);
            UnityEngine.InputSystem.Controls.ButtonControl mouseButton = GetMouseButton(index);
            if (mouseButton.wasPressedThisFrame)
            {
                phase = TouchPhase.Began;
            }
            else if (mouseButton.isPressed)
            {
                phase = TouchPhase.Moved;
            }
            else if (mouseButton.wasReleasedThisFrame)
            {
                phase = TouchPhase.Ended;
            }

#else

            float x = UnityEngine.Input.mousePosition.x;
            float y = UnityEngine.Input.mousePosition.y;
            mousePosition = new Vector2(x, y);

            if (UnityEngine.Input.GetMouseButtonDown(index))
            {
                phase = TouchPhase.Began;
            }
            else if (UnityEngine.Input.GetMouseButtonUp(index))
            {
                phase = TouchPhase.Ended;
            }
            else if (UnityEngine.Input.GetMouseButton(index))
            {
                phase = TouchPhase.Moved;
            }

#endif

            else
            {
                return;
            }

            Vector2 prev;
            if (!previousTouchPositions.TryGetValue(pointerId, out prev))
            {
                prev.x = x;
                prev.y = y;
            }
            GestureTouch g = new GestureTouch(pointerId, x, y, prev.x, prev.y, 1.0f, index, phase);
            FingersProcessTouch(ref g);
            prev.x = x;
            prev.y = y;
            previousTouchPositions[pointerId] = prev;
        }

        private void ProcessTouches()
        {
            touchCount = 0;

#if UNITY_INPUT_SYSTEM_V2

            foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
            {
                if (device.enabled &&
                    device is UnityEngine.InputSystem.Touchscreen touchScreen)
                {
                    foreach (var touch in touchScreen.touches)
                    {
                        var inputPhase = touch.phase.ReadValue();
                        if (inputPhase != UnityEngine.InputSystem.TouchPhase.None)
                        {
                            GestureTouch gestureTouch = GestureTouchFromTouch(touch, inputPhase);
                            touchCount++;
                            FingersProcessTouch(ref gestureTouch);
                        }
                    }
                }
            }

#else

            // process each touch in the Unity list of touches
            touchCount = UnityEngine.Input.touchCount;
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                UnityEngine.Touch t = UnityEngine.Input.GetTouch(i);
                GestureTouch g = GestureTouchFromTouch(ref t);
                FingersProcessTouch(ref g);
            }

#endif

        }

        private void ProcessVirtualTouches()
        {
            if (!RecordTouches && TestTouches != null && TestTouches.Count != 0)
            {
                for (int i = 0; i < TestTouches.Count; i++)
                {
                    FakeTouch fake = TestTouches[i];
                    if (accumulatedTime >= fake.Time)
                    {
                        TestTouches.RemoveAt(i--);
                        GestureTouch inProgress;
                        float prevX, prevY;
                        if (fakeTouchesInProgress.TryGetValue(fake.Id, out inProgress))
                        {
                            prevX = inProgress.ScreenX;
                            prevY = inProgress.ScreenY;
                        }
                        else
                        {
                            prevX = fake.ScreenPosition.x;
                            prevY = fake.ScreenPosition.y;
                            if (fake.ScreenPositionIsPercentage)
                            {
                                prevX *= Screen.width;
                                prevY *= Screen.height;
                            }
                        }
                        float screenX = fake.ScreenPosition.x;
                        float screenY = fake.ScreenPosition.y;
                        if (fake.ScreenPositionIsPercentage)
                        {
                            screenX *= Screen.width;
                            screenY *= Screen.height;
                        }
                        GestureTouch g = new GestureTouch(fake.Id, screenX, screenY, prevX, prevY, 1.0f, null, fake.Phase);
                        if (g.TouchPhase == TouchPhase.Cancelled || g.TouchPhase == TouchPhase.Ended)
                        {
                            FingersProcessTouch(ref g);
                            fakeTouchesInProgress.Remove(g.Id);
                        }
                        else
                        {
                            fakeTouchesInProgress[g.Id] = g;
                        }
                    }
                }
                foreach (KeyValuePair<int, GestureTouch> f in fakeTouchesInProgress.ToArray())
                {
                    GestureTouch g = f.Value;
                    FingersProcessTouch(ref g);
                }
            }
            if (VirtualTouchCountHandler != null && VirtualTouchObjectHandler != null)
            {
                int count = VirtualTouchCountHandler();
                for (int i = 0; i < count; i++)
                {
                    GestureTouch g = VirtualTouchObjectHandler(i);
                    FingersProcessTouch(ref g);
                }
                if (VirtualTouchUpdateHandler == null)
                {
                    Debug.LogError("Please implement VirtualTouchUpdateHandler and remove any ended gestures in that callback");
                }
                else
                {
                    VirtualTouchUpdateHandler();
                }
            }
        }

        private void RotateAroundPoint(ref float rotX, ref float rotY, float anchorX, float anchorY, float angleRadians)
        {
            // rotate around a point in 2D space
            float cosTheta = Mathf.Cos(angleRadians);
            float sinTheta = Mathf.Sin(angleRadians);
            float x = rotX - anchorX;
            float y = rotY - anchorY;
            rotX = ((cosTheta * x) - (sinTheta * y)) + anchorX;
            rotY = ((sinTheta * x) + (cosTheta * y)) + anchorY;
        }

        private bool HasMouse()
        {

#if UNITY_INPUT_SYSTEM_V2

            if (UnityEngine.InputSystem.Mouse.current == null || !TreatMousePointerAsFinger)
            {
                return false;
            }

#else

            // if not using the mouse, bail
            if (!UnityEngine.Input.mousePresent || !TreatMousePointerAsFinger)
            {
                return false;
            }

#endif

            return true;
        }

        private bool HasKeyboard()
        {

#if UNITY_INPUT_SYSTEM_V2

            if (UnityEngine.InputSystem.Keyboard.current == null)
            {
                return false;
            }

#endif

            return true;
        }

        private void ProcessMouseButtons()
        {
            if (!HasMouse())
            {
                return;
            }

            // add touches based on each mouse button
            AddMouseTouch(0, MousePointerId0);
            AddMouseTouch(1, MousePointerId1);
            AddMouseTouch(2, MousePointerId2);
        }

        private void ProcessMouseWheel()
        {
            if (!TreatMouseWheelAsFingers || !HasMouse() || !HasKeyboard())
            {
                return;
            }

            // the mouse wheel will act as a rotate and pinch / zoom

#if UNITY_INPUT_SYSTEM_V2

            var mouse = UnityEngine.InputSystem.Mouse.current;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            Vector2 delta = mouse.scroll.ReadValue() * 0.01f; // new input system is different from old input system by ~10x

#else

            Vector2 delta = UnityEngine.Input.mouseScrollDelta * 10.0f;

#endif

            float scrollDelta = (delta.y == 0.0f ? delta.x : delta.y) * MouseWheelDeltaMultiplier;
            float threshold = DeviceInfo.UnitsToPixels(MouseDistanceInUnitsForScaleAndRotate * 0.5f);

            // add type 1 = moved, 2 = begin, 3 = ended, 4 = none
            int addType1 = 4;
            int addType2 = 4;

            // left or right control initial down means begin
            if (!RequireControlKeyForMouseZoom)
            {
                if (delta == Vector2.zero)
                {
                    if (lastMouseWheelTime != System.DateTime.MinValue)
                    {
                        if ((System.DateTime.UtcNow - lastMouseWheelTime).TotalSeconds < 1.0f)
                        {
                            // continue zooming
                            pinchScale = Mathf.Max(0.35f, pinchScale + scrollDelta);
                            addType1 = 1;
                        }
                        else
                        {
                            // stop zooming
                            lastMouseWheelTime = System.DateTime.MinValue;
                            addType1 = 3;
                        }
                    }
                }
                else if (lastMouseWheelTime == System.DateTime.MinValue)
                {
                    // start zooming
                    addType1 = 2;
                    lastMouseWheelTime = System.DateTime.UtcNow;
                }
                else
                {
                    // continue zooming
                    pinchScale = Mathf.Max(0.35f, pinchScale + scrollDelta);
                    addType1 = 1;
                    lastMouseWheelTime = System.DateTime.UtcNow;
                }
            }

#if UNITY_INPUT_SYSTEM_V2

            else if (keyboard.leftCtrlKey.wasPressedThisFrame || keyboard.rightCtrlKey.wasPressedThisFrame)
            {
                // initial start of scale
                addType1 = 2;
            }
            // left or right control still down means move
            else if (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
            {
                pinchScale = Mathf.Max(0.35f, pinchScale + scrollDelta);
                addType1 = 1;
            }
            // left or right control initial up means end
            else if (keyboard.leftCtrlKey.wasReleasedThisFrame || keyboard.rightCtrlKey.wasReleasedThisFrame)
            {
                addType1 = 3;
            }

            // left or right shift initial down means begin
            if (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
            {
                addType2 = 2;
            }
            // left or right shift still down means move
            else if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
            {
                rotateAngle += scrollDelta;
                addType2 = 1;
            }
            // left or right shift initial up means end
            else if (keyboard.leftShiftKey.wasReleasedThisFrame || keyboard.rightShiftKey.wasReleasedThisFrame)
            {
                addType2 = 3;
            }

#else

            else if (UnityEngine.Input.GetKeyDown(KeyCode.LeftControl) || UnityEngine.Input.GetKeyDown(KeyCode.RightControl))
            {
                // initial start of scale
                addType1 = 2;
            }
            // left or right control still down means move
            else if (UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl))
            {
                pinchScale = Mathf.Max(0.35f, pinchScale + scrollDelta);
                addType1 = 1;
            }
            // left or right control initial up means end
            else if (UnityEngine.Input.GetKeyUp(KeyCode.LeftControl) || UnityEngine.Input.GetKeyUp(KeyCode.RightControl))
            {
                addType1 = 3;
            }

            // left or right shift initial down means begin
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift) || UnityEngine.Input.GetKeyDown(KeyCode.RightShift))
            {
                addType2 = 2;
            }
            // left or right shift still down means move
            else if (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))
            {
                rotateAngle += scrollDelta;
                addType2 = 1;
            }
            // left or right shift initial up means end
            else if (UnityEngine.Input.GetKeyUp(KeyCode.LeftShift) || UnityEngine.Input.GetKeyUp(KeyCode.RightShift))
            {
                addType2 = 3;
            }

#endif

            // use the minimum add type so that moves are preferred over begins and begins are preferred over ends
            int addType = Mathf.Min(addType1, addType2);

            // no begins, moves or ends, set defaults and end
            if (addType == 4)
            {
                pinchScale = 1.0f;
                rotateAngle = 0.0f;
                return;
            }

            // calculate rotation

#if UNITY_INPUT_SYSTEM_V2

            float x = mouse.position.ReadValue().x;
            float y = mouse.position.ReadValue().y;

#else

            float x = UnityEngine.Input.mousePosition.x;
            float y = UnityEngine.Input.mousePosition.y;

#endif

            float xRot1 = x - threshold;
            float yRot1 = y;
            float xRot2 = x + threshold;
            float yRot2 = y;
            float distance = threshold * pinchScale;
            xRot1 = x - distance;
            yRot1 = y;
            xRot2 = x + distance;
            yRot2 = y;
            RotateAroundPoint(ref xRot1, ref yRot1, x, y, rotateAngle);
            RotateAroundPoint(ref xRot2, ref yRot2, x, y, rotateAngle);

#if DEBUG

            if (scrollDelta != 0.0f)
            {
                //Debug.LogFormat("Mouse delta: {0}", scrollDelta);
            }

#endif

            // calculate rotation and zoom based on mouse values
            if (addType == 1)
            {
                // moved
                rotatePinch1 = new GestureTouch(MouseRotatePinchId0, xRot1, yRot1, rotatePinch1.X, rotatePinch1.Y, 1.0f, addType, TouchPhase.Moved);
                rotatePinch2 = new GestureTouch(MouseRotatePinchId1, xRot2, yRot2, rotatePinch2.X, rotatePinch2.Y, 1.0f, addType, TouchPhase.Moved);
            }
            else if (addType == 2)
            {
                // begin
                rotatePinch1 = new GestureTouch(MouseRotatePinchId0, xRot1, yRot1, xRot1, yRot1, 1.0f, addType, TouchPhase.Began);
                rotatePinch2 = new GestureTouch(MouseRotatePinchId1, xRot2, yRot2, xRot2, yRot2, 1.0f, addType, TouchPhase.Began);
            }
            else if (addType == 3)
            {
                // end
                rotatePinch1 = new GestureTouch(MouseRotatePinchId0, xRot1, yRot1, xRot1, yRot1, 1.0f, addType, TouchPhase.Ended);
                rotatePinch2 = new GestureTouch(MouseRotatePinchId1, xRot2, yRot2, xRot2, yRot2, 1.0f, addType, TouchPhase.Ended);
            }

            if (addType >= 1 && addType <= 3)
            {
                // apply snap back option if needed
                if (MouseWheelZoomOutSnapBack.x > 0.0f && MouseWheelZoomOutSnapBack.y > MouseWheelZoomOutSnapBack.x)
                {
                    Vector2 v1 = new Vector2(rotatePinch1.X, rotatePinch1.Y);
                    Vector2 v2 = new Vector2(rotatePinch2.X, rotatePinch2.Y);
                    float dist = Vector2.Distance(v1, v2);
                    dist = DeviceInfo.PixelsToUnits(dist);
                    if (dist < MouseWheelZoomOutSnapBack.x)
                    {
                        Vector2 dir = (v2 - v1).normalized;
                        dist = DeviceInfo.UnitsToPixels(MouseWheelZoomOutSnapBack.y);
                        dir *= dist;
                        v1 -= dir;
                        v2 += dir;
                        rotatePinch1 = new GestureTouch(rotatePinch1.Id, v1.x, v1.y, v1.x, v1.y,
                            rotatePinch1.Pressure, rotatePinch1.PlatformSpecificTouch, rotatePinch1.TouchPhase, rotatePinch1.DeviceId);
                        rotatePinch2 = new GestureTouch(rotatePinch2.Id, v2.x, v2.y, v2.x, v2.y,
                            rotatePinch2.Pressure, rotatePinch2.PlatformSpecificTouch, rotatePinch2.TouchPhase, rotatePinch2.DeviceId);
                        pinchScale = 1.0f;
                        rotateAngle = 0.0f;
                    }
                }

                FingersProcessTouch(ref rotatePinch1);
                FingersProcessTouch(ref rotatePinch2);
            }
        }

        private void ProcessLostTouches()
        {
            // handle lost touches due to Unity bugs, Unity can not send touch end states properly
            //  and it appears that even the id's of touches can change in WebGL
            foreach (GestureTouch t in previousTouches)
            {
                if (!currentTouches.Contains(t))
                {
                    tempTouches.Add(t);
                }
            }
            foreach (DigitalRubyShared.GestureRecognizer g in gestures)
            {
                bool reset = false;
                foreach (GestureTouch t in g.CurrentTrackedTouches)
                {
                    if (!currentTouches.Contains(t))
                    {
                        tempTouches.Add(t);
                        reset = true;
                    }
                }
                if (reset)
                {
                    g.Reset();
                }
            }
            foreach (GestureTouch t in tempTouches)
            {
                // only end touch here, as end touch removes from previousTouches list
                GestureTouch tmp = t;
                FingersEndTouch(ref tmp, true);
                previousTouches.Remove(tmp);
            }

            tempTouches.Clear();
        }

        private bool GameObjectMatchesPlatformSpecificView(List<GameObject> list, DigitalRubyShared.GestureRecognizer r)
        {
            GameObject platformSpecificView = r.PlatformSpecificView as GameObject;

            if ((platformSpecificView == null && EventSystem.current == null) ||
                // HACK: If the platform specific view is a Canvas, always match
                (platformSpecificView != null && platformSpecificView.GetComponent<Canvas>() != null))
            {
                return true;
            }
            else if (list.Count == 0)
            {
                return (platformSpecificView == null);
            }
            foreach (GameObject obj in list)
            {
                if (obj == platformSpecificView)
                {
                    return true;
                }
                else
                {
                    // if we have a collider and no platform specific view, count as a match
                    bool hasCollider = (obj != null && (obj.GetComponent<Collider2D>() != null || obj.GetComponent<Collider>() != null));
                    if (hasCollider && platformSpecificView == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private ICollection<GestureTouch> FilterTouchesBegan(List<GestureTouch> touches, DigitalRubyShared.GestureRecognizer r)
        {
            // in order to begin, touches must match the platform specific view
            List<GameObject> gameObjects;
            filteredTouches.Clear();
            foreach (GestureTouch t in touches)
            {
                if (!gameObjectsForTouch.TryGetValue(t.Id, out gameObjects) || GameObjectMatchesPlatformSpecificView(gameObjects, r))
                {
                    // make sure gesture is not masked out
                    if (masks.Count != 0)
                    {
                        bool hit = false;
                        bool needsMask = false;
                        foreach (GestureMask mask in masks)
                        {
                            Collider2D collider = mask.Collider;
                            if (collider != null && collider.enabled && collider.gameObject.activeInHierarchy)
                            {
                                Canvas canvas = collider.GetComponentInParent<Canvas>();
                                if (canvas != null)
                                {
                                    if (mask.Gestures == null || mask.Gestures.Count == 0 || mask.Gestures.Contains(r))
                                    {
                                        needsMask = true;
                                        Vector2 canvasSpacePoint = FingersUtility.ScreenToCanvasPoint(canvas, new Vector2(t.ScreenX, t.ScreenY));
                                        if (collider.OverlapPoint(canvasSpacePoint))
                                        {
                                            hit = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (!hit && needsMask)
                        {
                            // do not add to the gesture
                            continue;
                        }
                    }
                    filteredTouches.Add(t);
                }
            }
            return filteredTouches;
        }

        private void CleanupPassThroughObjects()
        {
            if (PassThroughObjects == null)
            {
                PassThroughObjects = new List<GameObject>();
            }
            for (int i = PassThroughObjects.Count - 1; i >= 0; i--)
            {
                if (PassThroughObjects[i] == null)
                {
                    PassThroughObjects.RemoveAt(i);
                }
            }
        }

        private void SetupDefaultPassThroughComponents()
        {
            componentTypesToDenyPassThrough.Add(typeof(Scrollbar));
            componentTypesToDenyPassThrough.Add(typeof(Button));
            componentTypesToDenyPassThrough.Add(typeof(Dropdown));
            componentTypesToDenyPassThrough.Add(typeof(Toggle));
            componentTypesToDenyPassThrough.Add(typeof(Slider));
            componentTypesToDenyPassThrough.Add(typeof(InputField));

#if UNITY_2019_1_OR_NEWER

            componentTypesToDenyPassThrough.Add(typeof(UnityEngine.UIElements.ScrollView));

#endif

            componentTypesToIgnorePassThrough.Add(typeof(Text));
        }

        private void SceneManagerSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {

#if UNITY_EDITOR

            if (scene.name.IndexOf("Preview Scene", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return;
            }

#endif

            switch (LevelUnloadOption)
            {
                case GestureLevelUnloadOption.ResetGestureState:
                    ResetState(false);
                    break;

                case GestureLevelUnloadOption.ClearAllGestures:
                    ResetState(true);
                    break;
            }
        }

        private void Awake()
        {
            if (singleton != null && singleton != this)
            {

#if UNITY_EDITOR

                Debug.LogError("Multiple fingers scripts found, there should only be one. When you access FingersScript.Instance, the prefab will be loaded automatically from the resources folder if it isn't already in the scene. If you are dragging the prefab into your scenes, then every scene needs to have the prefab dragged in, otherwise no scene should have the prefab dragged in and it will load automatically when you call FingersScript.Instance.");

#endif

                DestroyImmediate(gameObject, true);
                return;
            }
            singleton = this;
            origTouchCircles = TouchCircles;

            // setup DPI, using a default value if it cannot be determined
            DeviceInfo.PixelsPerInch = Screen.dpi;
            if (DeviceInfo.PixelsPerInch > 0)
            {
                DeviceInfo.UnitMultiplier = DeviceInfo.PixelsPerInch;
                Debug.Log("Detected DPI of " + DeviceInfo.PixelsPerInch);
            }
            else
            {
                // pick a sensible dpi since we don't know the actual DPI
                DeviceInfo.UnitMultiplier = DeviceInfo.PixelsPerInch = DefaultDPI;

#if UNITY_2019_4_OR_NEWER

                if (!UnityEngine.Application.isBatchMode)

#else

                if (UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)

#endif

                    Debug.LogError("Unable to determine DPI, using default DPI of " + DefaultDPI);
            }

            // set the main thread callback so gestures can callback after a delay
            DigitalRubyShared.GestureRecognizer.MainThreadCallback = (float delay, System.Action callback) =>
            {
                StartCoroutine(MainThreadCallback(delay, callback));
            };

            ResetState(false);
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += SceneManagerSceneUnloaded;

#if UNITY_INPUT_SYSTEM_V2

            // unable to set multi-touch and simulate mouse with touches as of 1.0.2

#else

            if (!UnityEngine.Input.multiTouchEnabled && EnableMultiTouch)
            {
                UnityEngine.Input.multiTouchEnabled = true;
            }
            UnityEngine.Input.simulateMouseWithTouches = SimulateMouseWithTouches;

#endif

            SetupDefaultPassThroughComponents();
        }

        private void OnEnable()
        {
            if (!HasMouse())
            {
                TreatMousePointerAsFinger = false;
            }

            // add event system if needed
            if (EventSystem.current == null && AutoAddRequiredComponents && FindObjectOfType<EventSystem>() == null)
            {
                gameObject.AddComponent<EventSystem>().hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void OnDisable()
        {
            ResetState(false);
        }

        private void Update()
        {
            if (!UseFixedUpdate)
            {
                accumulatedTime += Time.deltaTime;
                InternalUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (UseFixedUpdate)
            {
                accumulatedTime += Time.fixedDeltaTime;
                InternalUpdate();
            }
        }

        private void InternalUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (AutoAddRequiredComponents)
            {
                // add ray casters
                foreach (Camera camera in Camera.allCameras)
                {
                    if (camera.cameraType == CameraType.Game)
                    {
                        if (camera.GetComponent<PhysicsRaycaster>() == null)
                        {
                            camera.gameObject.AddComponent<PhysicsRaycaster>().hideFlags = HideFlags.HideAndDontSave;
                        }
                        if (camera.GetComponent<Physics2DRaycaster>() == null)
                        {
                            camera.gameObject.AddComponent<Physics2DRaycaster>().hideFlags = HideFlags.HideAndDontSave;
                        }
                    }
                }
            }

            // turn on the canvas to see touches - don't do this unless you are debugging as it can mess up other canvases
            if (gameObject.transform.childCount > 0 && gameObject.transform.GetChild(0).GetComponent<Canvas>() != null)
            {
                gameObject.transform.GetChild(0).gameObject.SetActive(ShowTouches);
            }

            // cleanup pass through objects
            CleanupPassThroughObjects();

            // clear out all touches for each phase
            currentTouches.Clear();
            touchesBegan.Clear();
            touchesMoved.Clear();
            touchesEnded.Clear();

            // process touches if asked for
            if (ProcessUnityTouches)
            {
                ProcessTouches();
            }

            // process injected touches
            ProcessVirtualTouches();

            // process mouse
            ProcessMouseButtons();
            ProcessMouseWheel();

            // hack around Unity bugs
            ProcessLostTouches();

            // Debug.LogFormat("B: {0}, M: {1}, E: {2}", touchesBegan.Count, touchesMoved.Count, touchesEnded.Count);

            // keep track of game objects and touches
            foreach (GestureTouch t in touchesBegan)
            {
                PopulateGameObjectsForTouch(t.Id, t.X, t.Y);
            }

            // for each gesture, process the touches
            // copy to temp list in case gestures are added during the callbacks
            gesturesTemp.AddRange(gestures);
            gesturesTemp.Sort();
            foreach (DigitalRubyShared.GestureRecognizer gesture in gesturesTemp)
            {
                gesture.ProcessTouchesBegan(FilterTouchesBegan(touchesBegan, gesture));
                gesture.ProcessTouchesMoved(touchesMoved);
                gesture.ProcessTouchesEnded(touchesEnded);
            }
            gesturesTemp.Clear();

            // remove any game objects that are no longer being touched
            foreach (GestureTouch t in touchesEnded)
            {
                gameObjectsForTouch.Remove(t.Id);
            }

            // clear touches
            touches.Clear();

            // add all the touches
            touches.AddRange(touchesBegan);
            touches.AddRange(touchesMoved);
            touches.AddRange(touchesEnded);
        }

        private void LateUpdate()
        {
            if (ShowTouches && TouchCircles != null && TouchCircles.Length != 0)
            {
                float timestamp = Time.time;
                foreach (GestureTouch t in Touches)
                {
                    // find the existing touch, else create a new entry
                    ShownTouch obj;
                    if (shownTouches.TryGetValue(t.Id, out obj))
                    {
                        // update the timestamp
                        obj.Timestamp = timestamp;
                        shownTouches[t.Id] = obj;
                    }
                    else if (availableShowTouches.Count != 0)
                    {
                        // create a new entry
                        int index = availableShowTouches.Count - 1;
                        shownTouches[t.Id] = obj = new ShownTouch { GameObject = availableShowTouches[index], Timestamp = timestamp };
                        availableShowTouches.RemoveAt(index);

                        // activate the game object
                        obj.GameObject.SetActive(true);
                    }
                    else
                    {
                        // move along, no available touches to display
                        continue;
                    }
                    obj.GameObject.transform.position = new Vector3(t.X, t.Y);
                }

                // create list to remove, cannot remove during foreach
                foreach (var kv in shownTouches)
                {
                    if (kv.Value.Timestamp < timestamp)
                    {
                        // add id to remove, add to available list and de-activate the game object
                        shownTouchesToRemove.Add(kv.Key);
                        availableShowTouches.Add(kv.Value.GameObject);
                        kv.Value.GameObject.SetActive(false);
                    }
                }

                // remove any shown touches that no longer have a touch
                foreach (int id in shownTouchesToRemove)
                {
                    shownTouches.Remove(id);
                }
                shownTouchesToRemove.Clear();
            }
        }

        private void OnDestroy()
        {
            if (singleton == this)
            {
                singleton = null;
            }
        }

        private static readonly List<RaycastResult> captureRaycastResultsTemp = new List<RaycastResult>();
        private static readonly System.Comparison<RaycastResult> raycastResultCompare = RaycastResultCompare;

        private static int RaycastResultCompare(RaycastResult r1, RaycastResult r2)
        {
            SpriteRenderer rend1 = r1.gameObject.GetComponent<SpriteRenderer>();
            if (rend1 != null)
            {
                SpriteRenderer rend2 = r2.gameObject.GetComponent<SpriteRenderer>();
                if (rend2 != null)
                {
                    int comp = rend2.sortingLayerID.CompareTo(rend1.sortingLayerID);
                    if (comp == 0)
                    {
                        comp = rend2.sortingOrder.CompareTo(rend1.sortingOrder);
                    }
                    return comp;
                }
            }
            return r2.gameObject.transform.GetSiblingIndex().CompareTo(r1.gameObject.transform.GetSiblingIndex());
        }

        /// <summary>
        /// Check if a gesture intersects an object
        /// </summary>
        /// <param name="r">Gesture recognizer</param>
        /// <param name="camera">Camera</param>
        /// <param name="obj">Object to check for intersection</param>
        /// <param name="mode">Gesture mode</param>
        /// <param name="comparer">Comparer or null for default</param>
        /// <returns>The intersected game object or null if none</returns>
        public static GameObject GestureIntersectsObject(DigitalRubyShared.GestureRecognizer r, Camera camera, GameObject obj,
            GestureRecognizerComponentScriptBase.GestureObjectMode mode, System.Comparison<RaycastResult> comparer = null)
        {
            if (EventSystem.current == null || camera == null)
            {
                return null;
            }
            captureRaycastResultsTemp.Clear();
            PointerEventData p = new PointerEventData(EventSystem.current);
            p.Reset();
            p.position = new Vector2(r.FocusX, r.FocusY);
            p.clickCount = 1;
            EventSystem.current.RaycastAll(p, captureRaycastResultsTemp);
            captureRaycastResultsTemp.Sort(comparer ?? raycastResultCompare);

            foreach (RaycastResult result in captureRaycastResultsTemp)
            {
                if (result.gameObject == obj)
                {
                    return result.gameObject;
                }
                else if (result.gameObject.GetComponent<Collider>() != null ||
                    result.gameObject.GetComponent<Collider2D>() != null ||
                    result.gameObject.GetComponent<FingersPanRotateScaleComponentScript>() != null)
                {
                    if (mode == GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
                    {
                        return result.gameObject;
                    }

                    // blocked by a collider or another gesture, bail
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// Get camera for a gesture using gesture start location
        /// </summary>
        /// <param name="r">Gesture recognizer</param>
        /// <param name="cameras">Cameras</param>
        /// <returns>Camera or null if none found</returns>
        public static Camera GetCameraForGesture(DigitalRubyShared.GestureRecognizer r, Camera[] cameras)
        {
            Camera camera = null;
            if (cameras != null && cameras.Length != 0)
            {
                Vector2 screenPoint = new Vector2(r.StartFocusX, r.StartFocusY);
                foreach (Camera cam in cameras)
                {
                    if (cam != null && cam.pixelRect.Contains(screenPoint))
                    {
                        camera = cam;
                        break;
                    }
                }
            }
            return camera;
        }

        /// <summary>
        /// Start or reset a gesture
        /// </summary>
        /// <param name="r">Gesture</param>
        /// <param name="bringToFront">Whether to bring obj to front</param>
        /// <param name="cameras">Allowed cameras to execute in</param>
        /// <param name="obj">Object to execute gesture on</param>
        /// <param name="spriteRenderer">Sprite renderer to bring to front</param>
        /// <param name="mode">Gesture mode</param>
        /// <param name="camera">Receies found camera or null if none</param>
        /// <returns></returns>
        public static GameObject StartOrResetGesture(DigitalRubyShared.GestureRecognizer r, bool bringToFront, Camera[] cameras, GameObject obj,
            SpriteRenderer spriteRenderer, GestureRecognizerComponentScriptBase.GestureObjectMode mode, out Camera camera)
        {
            GameObject result = null;
            camera = FingersScript.GetCameraForGesture(r, cameras);
            if (camera != null && r.State == GestureRecognizerState.Began)
            {
                if ((result = GestureIntersectsObject(r, camera, obj, mode)) != null)
                {
                    SpriteRenderer _spriteRenderer;
                    if (bringToFront && (_spriteRenderer = result.GetComponent<SpriteRenderer>()) != null)
                    {
                        _spriteRenderer.sortingOrder = 1000;
                    }
                }
                else
                {
                    r.Reset();
                }
            }
            return result;
        }

        /// <summary>
        /// Add a gesture to the fingers script. This gesture will give callbacks when it changes state.
        /// </summary>
        /// <param name="gesture">Gesture to add</param>
        /// <return>True if the gesture was added, false if the gesture was already added</return>
        public bool AddGesture(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture == null || gestures.Contains(gesture))
            {
                return false;
            }
            gestures.Add(gesture);
            return true;
        }

        /// <summary>
        /// Remove a gesture from the script and dispose of it. The gesture will no longer give callbacks.
        /// You will need to re-create the gesture.
        /// </summary>
        /// <param name="gesture">Gesture to remove</param>
        /// <returns>True if the gesture was removed, false if it was not in the script</returns>
        public bool RemoveGesture(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture != null)
            {
                gesture.Dispose();
                return gestures.Remove(gesture);
            }
            return false;
        }

        /// <summary>
        /// Add a mask
        /// </summary>
        /// <param name="collider">Mask</param>
        /// <param name="gesture">Gesture to mask, pass null to mask all gestures</param>
        /// <returns>True if added, false if already exists</returns>
        public bool AddMask(Collider2D collider, GestureRecognizer gesture)
        {
            if (collider == null)
            {
                return false;
            }

            foreach (GestureMask mask in masks)
            {
                if (mask.Collider == collider)
                {
                    if (!mask.Gestures.Contains(gesture))
                    {
                        mask.Gestures.Add(gesture);
                        return true;
                    }
                    return false;
                }
            }
            GestureMask newMask = new GestureMask(collider, gesture);
            masks.Add(newMask);
            return true;
        }

        /// <summary>
        /// Remove a mask
        /// </summary>
        /// <param name="collider">Mask to remove, null to remove all masks</param>
        /// <param name="gesture">Gesture to unmask, pass null to remove the mask completely</param>
        /// <returns>True if removed, false if did not exist</returns>
        public bool RemoveMask(Collider2D collider, GestureRecognizer gesture)
        {
            if (collider == null)
            {
                masks.Clear();
                return true;
            }

            foreach (GestureMask mask in masks.ToArray())
            {
                if (mask.Collider == collider)
                {
                    if (gesture == null)
                    {
                        masks.Remove(mask);
                        return true;
                    }
                    else if (mask.Gestures.Contains(gesture))
                    {
                        mask.Gestures.Remove(gesture);
                        if (mask.Gestures.Count == 0)
                        {
                            masks.Remove(mask);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Reset state - all touches and tracking are cleared
        /// </summary>
        /// <param name="clearGestures">True to clear out all gestures, false otherwise</param>
        public void ResetState(bool clearGestures)
        {
            for (int i = gestures.Count - 1; i >= 0; i--)
            {
                if (gestures[i] == null)
                {
                    gestures.RemoveAt(i);
                }
                else
                {
                    gestures[i].Reset();
                }
            }
            if (clearGestures)
            {
                gestures.Clear();
            }
            availableShowTouches.Clear();
            availableShowTouches.AddRange(TouchCircles);
            foreach (GameObject obj in TouchCircles)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
            foreach (GameObject obj in origTouchCircles)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
            shownTouches.Clear();
            currentTouches.Clear();
            previousTouches.Clear();
            touchesBegan.Clear();
            touchesMoved.Clear();
            touchesEnded.Clear();
            gameObjectsForTouch.Clear();
            captureRaycastResults.Clear();
            filteredTouches.Clear();
            touches.Clear();
            previousTouchPositions.Clear();
            rotateAngle = 0.0f;
            pinchScale = 1.0f;
            rotatePinch1 = new GestureTouch();
            rotatePinch2 = new GestureTouch();
            lastMouseWheelTime = System.DateTime.MinValue;

            if (PassThroughObjects != null)
            {
                // cleanup deleted pass through objects
                for (int i = PassThroughObjects.Count - 1; i >= 0; i--)
                {
                    if (PassThroughObjects[i] == null)
                    {
                        PassThroughObjects.RemoveAt(i);
                    }
                }
            }

            if (VirtualTouchResetHandler != null)
            {
                VirtualTouchResetHandler();
            }
        }

        /// <summary>
        /// Convert rect transform to screen space
        /// </summary>
        /// <param name="transform">Rect transform</param>
        /// <returns>Screen space rect</returns>
        public static Rect RectTransformToScreenSpace(RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            float x = transform.position.x - (size.x * 0.5f);
            float y = transform.position.y - (size.y * 0.5f);
            return new Rect(x, y, size.x, size.y);
        }

        /// <summary>
        /// Create a gesture touch object from a virtual touch object and updates the previous position internally
        /// </summary>
        /// <param name="touchId">Virtual touch/finger id</param>
        /// <param name="touchPosition">Virtual touch position</param>
        /// <param name="touchPhase">Virtual touch phase</param>
        /// <param name="touchPressure">Virtual touch pressure</param>
        /// <returns>GestureTouch</returns>
        public GestureTouch GestureTouchFromVirtualTouch(int touchId, Vector2 touchPosition, UnityEngine.TouchPhase touchPhase, float touchPressure = 1.0f)
        {
            // convert Unity touch to Gesture touch
            Vector2 prev;
            if (!previousTouchPositions.TryGetValue(touchId, out prev))
            {
                prev.x = touchPosition.x;
                prev.y = touchPosition.y;
            }
            DigitalRubyShared.TouchPhase mappedTouchPhase;
            switch (touchPhase)
            {
                case UnityEngine.TouchPhase.Began:
                    mappedTouchPhase = TouchPhase.Began;
                    break;

                case UnityEngine.TouchPhase.Canceled:
                    mappedTouchPhase = TouchPhase.Cancelled;
                    break;

                case UnityEngine.TouchPhase.Ended:
                    mappedTouchPhase = TouchPhase.Ended;
                    break;

                case UnityEngine.TouchPhase.Moved:
                    mappedTouchPhase = TouchPhase.Moved;
                    break;

                case UnityEngine.TouchPhase.Stationary:
                    mappedTouchPhase = TouchPhase.Stationary;
                    break;

                default:
                    mappedTouchPhase = TouchPhase.Unknown;
                    break;
            }
            GestureTouch touch = new GestureTouch(touchId, touchPosition.x, touchPosition.y, prev.x, prev.y, touchPressure, null, mappedTouchPhase);
            prev.x = touchPosition.x;
            prev.y = touchPosition.y;
            previousTouchPositions[touchId] = prev;
            return touch;
        }

        /// <summary>
        /// Get a touch from an index
        /// </summary>
        /// <param name="index">Touch index</param>
        /// <returns>Touch</returns>
        public GestureTouch GetTouch(int index)
        {

#if UNITY_INPUT_SYSTEM_V2

            int idx = 0;
            foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
            {
                if (device is UnityEngine.InputSystem.Touchscreen touchScreen)
                {
                    foreach (var touch in touchScreen.touches)
                    {
                        var phase = touch.phase.ReadValue();
                        if (phase != UnityEngine.InputSystem.TouchPhase.None)
                        {
                            if (idx == index)
                            {
                                GestureTouch gestureTouch = GestureTouchFromTouch(touch, phase);
                                return gestureTouch;
                            }
                            idx++;
                        }
                    }
                }
            }
            throw new System.ArgumentException("Index is out of range of active touches");

#else

            // process each touch in the Unity list of touches
            UnityEngine.Touch t = UnityEngine.Input.GetTouch(index);
            GestureTouch g = GestureTouchFromTouch(ref t);
            return g;

#endif

        }

        public bool IsKeyDownThisFrame(UnityEngine.KeyCode key)
        {

#if UNITY_INPUT_SYSTEM_V2

            return UnityEngine.InputSystem.Keyboard.current[keyCodeMappings[key]].wasPressedThisFrame;

#else

            return UnityEngine.Input.GetKeyDown(key);

#endif

        }

        public bool IsKeyDown(UnityEngine.KeyCode key)
        {

#if UNITY_INPUT_SYSTEM_V2

            return UnityEngine.InputSystem.Keyboard.current[keyCodeMappings[key]].isPressed;

#else

            return UnityEngine.Input.GetKey(key);

#endif

        }

        public bool IsKeyUpThisFrame(UnityEngine.KeyCode key)
        {

#if UNITY_INPUT_SYSTEM_V2

            return UnityEngine.InputSystem.Keyboard.current[keyCodeMappings[key]].wasReleasedThisFrame;

#else

            return UnityEngine.Input.GetKeyUp(key);

#endif

        }

        public bool IsMouseDownThisFrame(int index)
        {

#if UNITY_INPUT_SYSTEM_V2

            UnityEngine.InputSystem.Controls.ButtonControl mouseButton = GetMouseButton(index);
            return mouseButton.wasPressedThisFrame;

#else

            return UnityEngine.Input.GetMouseButtonDown(index);

#endif

        }

        public bool IsMouseDown(int index)
        {

#if UNITY_INPUT_SYSTEM_V2

            UnityEngine.InputSystem.Controls.ButtonControl mouseButton = GetMouseButton(index);
            return mouseButton.isPressed;

#else

            return UnityEngine.Input.GetMouseButton(index);

#endif

        }

        public bool IsMouseUpThisFrame(int index)
        {

#if UNITY_INPUT_SYSTEM_V2

            UnityEngine.InputSystem.Controls.ButtonControl mouseButton = GetMouseButton(index);
            return mouseButton.wasReleasedThisFrame;

#else

            return UnityEngine.Input.GetMouseButtonUp(index);

#endif

        }

        /// <summary>
        /// Gets a collection of the current touches
        /// </summary>
        public ICollection<GestureTouch> Touches { get { return touches; } }

        /// <summary>
        /// Optional handler to determine whether a game object will pass through or not.
        /// Null handler gets default gesture capture handling.
        /// Non-null handler that returns null gets default handling.
        /// Non-null handler that returns true captures the gesture.
        /// Non-null handler that returns false passes the gesture through.
        /// </summary>
        public System.Func<GameObject, bool?> CaptureGestureHandler { get; set; }

        /// <summary>
        /// Get a count of virtual touches, null for none. This callback is where you should update and populate the current set of gestures for your virtual touches.
        /// </summary>
        public System.Func<int> VirtualTouchCountHandler { get; set; }

        /// <summary>
        /// Get a virtual touch from a given index (0 to count - 1), null for none. Use GestureTouchFromVirtualTouch once a frame to create the GestureTouch object.
        /// </summary>
        public System.Func<int, GestureTouch> VirtualTouchObjectHandler { get; set; }

        /// <summary>
        /// This event executes after the all virtual touches have been processed by the fingers script.
        /// This is where you would remove any ended gestures from your virtual touch gesture lists or dictionaries.
        /// </summary>
        public System.Action VirtualTouchUpdateHandler { get; set; }

        /// <summary>
        /// This is called anytime fingers script is reset. All virtual touches should be cleared.
        /// </summary>
        public System.Action VirtualTouchResetHandler { get; set; }

        /// <summary>
        /// A set of component types that will stop the gesture from passing through. By default includes UI components like Button, Dropdown, etc.
        /// You can add additional component types if you like, but you should not remove items from this set or clear the set.
        /// </summary>
        public HashSet<System.Type> ComponentTypesToDenyPassThrough { get { return componentTypesToDenyPassThrough; } }

        /// <summary>
        /// A set of component types that will be ignored for purposes of pass through checking. By default includes the Text UI component.
        /// You can add additional component types if you like, but you should not remove items from this set or clear the set.
        /// </summary>
        public HashSet<System.Type> ComponentTypesToIgnorePassThrough { get { return componentTypesToIgnorePassThrough; } }

        /// <summary>
        /// Previous touch locations
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<GestureTouch> PreviousTouches { get { return previousTouches.AsReadOnly(); } }

        /// <summary>
        /// Current touch objects begin tracked
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<GestureTouch> CurrentTouches { get { return currentTouches.AsReadOnly(); } }

        /// <summary>
        /// All gestures added to the script
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<DigitalRubyShared.GestureRecognizer> Gestures { get { return gestures.AsReadOnly(); } }

        /// <summary>
        /// Shared static instance of fingers script that lives forever - the prefab MUST exist in a resources folder!
        /// Note that you can still add the prefab to your scene if you prefer to do it that way, in which case the
        /// singleton will be re-created each time a scene loads.
        /// </summary>
        public static FingersScript Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = GameObject.FindObjectOfType<FingersScript>();
                    if (singleton == null)
                    {
                        GameObject prefab = GameObject.Instantiate(Resources.Load("FingersScriptPrefab") as GameObject);
                        GameObject.DontDestroyOnLoad(prefab);
                        singleton = prefab.GetComponent<FingersScript>();
                    }
                }
                return singleton;
            }
        }

        /// <summary>
        /// Get the current touch count
        /// </summary>
        public int TouchCount { get { return touchCount; } }

        /// <summary>
        /// Determines if a mouse is present
        /// </summary>
        public bool MousePresent { get { return HasMouse(); } }

        /// <summary>
        /// Get the current mouse position
        /// </summary>
        public Vector2 MousePosition { get { return mousePosition; } }

        /// <summary>
        /// Check whether Instance is not null without it actually creating a prefab if needed
        /// Call this when removing the gestures in OnDisable to avoid creating a prefab in OnDestroy methods and causing lots of errors
        /// </summary>
        public static bool HasInstance { get { return singleton != null; } }
    }

    /// <summary>
    /// Fake touch class, for testing
    /// </summary>
    [System.Serializable]
    public class FakeTouch
    {
        /// <summary>
        /// Touch id
        /// </summary>
        [Tooltip("Touch id")]
        public int Id = 1;

        /// <summary>
        /// Screen position
        /// </summary>
        [Tooltip("Screen position")]
        public Vector2 ScreenPosition;

        [Tooltip("Whether the screen position is a percentage (0 to 1) or absolute pixel coordinate")]
        public bool ScreenPositionIsPercentage;

        /// <summary>
        /// Time in seconds
        /// </summary>
        [Tooltip("Time in seconds")]
        public float Time;

        /// <summary>
        /// Touch phase
        /// </summary>
        [Tooltip("Touch phase")]
        public TouchPhase Phase = TouchPhase.Moved;
    }
}
