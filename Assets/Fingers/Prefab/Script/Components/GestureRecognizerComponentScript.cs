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
    /// Update event for gesture recognizers
    /// </summary>
    [System.Serializable]
    public class GestureRecognizerComponentStateUpdatedEvent : UnityEngine.Events.UnityEvent<DigitalRubyShared.GestureRecognizer> { }

    /// <summary>
    /// Gesture recognizer component event
    /// </summary>
    [System.Serializable]
    public class GestureRecognizerComponentEvent : UnityEngine.Events.UnityEvent { }

    /// <summary>
    /// Gesture recognizer component event with Vector2
    /// </summary>
    [System.Serializable]
    public class GestureRecognizerComponentEventVector2 : UnityEngine.Events.UnityEvent<Vector2> { }

    /// <summary>
    /// Base class for component gesture scripts
    /// </summary>
    public abstract class GestureRecognizerComponentScriptBase : MonoBehaviour
    {
        /// <summary>
        /// Different types of object modes for component gestures
        /// </summary>
        public enum GestureObjectMode
        {
            /// <summary>
            /// Gesture must execute on the game object
            /// </summary>
            RequireIntersectWithGameObject,

            /// <summary>
            /// Raycast will determine which object gets affected
            /// </summary>
            AllowOnAnyGameObjectViaRaycast
        }

        /// <summary>
        /// Base gesture recognizer
        /// </summary>
        public DigitalRubyShared.GestureRecognizer GestureBase { get; protected set; }
    }

    /// <summary>
    /// Base class for component gesture scripts with a type of gesture
    /// </summary>
    /// <typeparam name="T">Type of gesture</typeparam>
    public abstract class GestureRecognizerComponentScript<T> : GestureRecognizerComponentScriptBase where T : DigitalRubyShared.GestureRecognizer, new()
    {
        /// <summary>Gesture state updated callback</summary>
        [Header("Gesture properties")]
        [Tooltip("Gesture state updated callback")]
        public GestureRecognizerComponentStateUpdatedEvent GestureStateUpdated;

        /// <summary>The game object the gesture must execute over, null to allow the gesture to execute anywhere.</summary>
        [Tooltip("The game object the gesture must execute over, null to allow the gesture to execute anywhere.")]
        public GameObject GestureView;

        /// <summary>The minimum number of touches to track. This gesture will not start unless this many touches are tracked. Default is usually 1 or 2. Not all gestures will honor values higher than 1.</summary>
        [Tooltip("The minimum number of touches to track. This gesture will not start unless this many touches are tracked. Default is usually 1 or 2. Not all gestures will honor values higher than 1.")]
        [Range(1, 10)]
        public int MinimumNumberOfTouchesToTrack = 1;

        /// <summary>The maximum number of touches to track. This gesture will never track more touches than this. Default is usually 1 or 2. Not all gestures will honor values higher than 1.</summary>
        [Tooltip("The maximum number of touches to track. This gesture will never track more touches than this. Default is usually 1 or 2. Not all gestures will honor values higher than 1.")]
        [Range(1, 10)]
        public int MaximumNumberOfTouchesToTrack = 1;

        /// <summary>Gesture components to allow simultaneous execution with. By default, gestures cannot execute together.</summary>
        [Tooltip("Gesture components to allow simultaneous execution with. By default, gestures cannot execute together.")]
        public List<GestureRecognizerComponentScriptBase> AllowSimultaneousExecutionWith;

        /// <summary>Whether to allow the gesture to execute simultaneously with all other gestures.</summary>
        [Tooltip("Whether to allow the gesture to execute simultaneously with all other gestures.")]
        public bool AllowSimultaneousExecutionWithAllGestures;

        /// <summary>The gesture recognizers that should be required to fail</summary>
        [Tooltip("The gesture recognizers that should be required to fail")]
        public List<GestureRecognizerComponentScriptBase> RequireGestureRecognizersToFail;

        /// <summary>Whether tracked touches are cleared when the gesture ends or fails, default is false. By setting to true, you allow the gesture to possibly execute again with a different touch even if the original touch it failed on is still on-going. This is a special case, so be sure to watch for problems if you set this to true, as leaving it false ensures the most correct behavior, especially with lots of gestures at once.</summary>
        [Tooltip("Whether tracked touches are cleared when the gesture ends or fails, default is false. By setting to true, you allow the gesture to " +
            "possibly execute again with a different touch even if the original touch it failed on is still on-going. This is a special case, " +
            "so be sure to watch for problems if you set this to true, as leaving it false ensures the most correct behavior, especially " +
            "with lots of gestures at once.")]
        public bool ClearTrackedTouchesOnEndOrFail;

        /// <summary>A mask to restrict this gesture to</summary>
        [Tooltip("A mask to restrict this gesture to")]
        public Collider2D Mask;

        private T gesture;
        /// <summary>
        /// Get the gesture for this component
        /// </summary>
        public T Gesture
        {
            get
            {
                if (gesture != null)
                {
                    return gesture;
                }
                GestureBase = gesture = new T();
                return gesture;
            }
        }

        /// <summary>
        /// Gesture callback
        /// </summary>
        /// <param name="gesture">Gesture</param>
        protected virtual void GestureStateUpdatedCallback(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (GestureStateUpdated != null && (gesture.State == GestureRecognizerState.Began || gesture.State == GestureRecognizerState.Executing || gesture.State == GestureRecognizerState.Ended))
            {
                GestureStateUpdated.Invoke(gesture);
            }
        }

        /// <summary>
        /// Awake
        /// </summary>
        protected virtual void Awake()
        {

        }

        /// <summary>
        /// Update
        /// </summary>
        protected virtual void Update()
        {

        }

        /// <summary>
        /// LateUpdate
        /// </summary>
        protected virtual void LateUpdate()
        {

        }

        /// <summary>
        /// OnEnable
        /// </summary>
        protected virtual void OnEnable()
        {
            if (gesture == null)
            {
                Gesture.StateUpdated += GestureStateUpdatedCallback;
                Gesture.PlatformSpecificView = GestureView;
                Gesture.MinimumNumberOfTouchesToTrack = MinimumNumberOfTouchesToTrack;
                Gesture.MaximumNumberOfTouchesToTrack = MaximumNumberOfTouchesToTrack;
                Gesture.ClearTrackedTouchesOnEndOrFail = ClearTrackedTouchesOnEndOrFail;
                if (AllowSimultaneousExecutionWithAllGestures)
                {
                    Gesture.AllowSimultaneousExecutionWithAllGestures();
                }
                else if (AllowSimultaneousExecutionWith != null)
                {
                    foreach (GestureRecognizerComponentScriptBase gesture in AllowSimultaneousExecutionWith)
                    {
                        Gesture.AllowSimultaneousExecution(gesture.GestureBase);
                    }
                }
                foreach (GestureRecognizerComponentScriptBase gesture in RequireGestureRecognizersToFail)
                {
                    Gesture.AddRequiredGestureRecognizerToFail(gesture.GestureBase);
                }
            }
            FingersScript.Instance.AddGesture(Gesture);
            FingersScript.Instance.AddMask(Mask, Gesture);
        }

        /// <summary>
        /// OnDisable
        /// </summary>
        protected virtual void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(Gesture);
                FingersScript.Instance.RemoveMask(Mask, Gesture);
            }
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected virtual void OnDestroy()
        {
            // OnDisable is called right before OnDestroy
        }
    }
}
