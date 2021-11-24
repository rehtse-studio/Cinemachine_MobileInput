using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// Main menu script for example projects
    /// </summary>
    public class FingersExampleMainMenuScript : MonoBehaviour
    {
        /// <summary>
        /// The scene to load when play is clicked
        /// </summary>
        [Tooltip("The scene to load when play is clicked")]
        public string PlaySceneName;

        /// <summary>
        /// The scene to load when options is clicked
        /// </summary>
        [Tooltip("The scene to load when options is clicked")]
        public string OptionsSceneName;

        private void OnEnable()
        {
            // do not modify gestures when a new scene loads, it is up to each scene to add and remove the gestures in onenable and ondisable script methods
            FingersScript.Instance.LevelUnloadOption = FingersScript.GestureLevelUnloadOption.Nothing;
        }

        /// <summary>
        /// Play button click event
        /// </summary>
        public void PlayButtonClicked()
        {
            Debug.Log("Play clicked");
            FingersExampleSceneTransitionScript.PushScene(PlaySceneName);
        }

        /// <summary>
        /// Options button click event
        /// </summary>
        public void OptionsButtonClicked()
        {
            Debug.Log("Options clicked");
            FingersExampleSceneTransitionScript.PushScene(OptionsSceneName);
        }

        /// <summary>
        /// Quit button click event
        /// </summary>
        public void QuitButtonClicked()
        {
            Debug.Log("Quit clicked");
            Application.Quit();
        }
    }
}