using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DigitalRubyShared
{
    /// <summary>
    /// Manage scene transitions for example projects
    /// </summary>
    public static class FingersExampleSceneTransitionScript
    {
        /// <summary>
        /// Animation duration for transitions
        /// </summary>
        public static float AnimationDuration = 0.5f;

        private static RenderTexture transitionTexture1;
        private static RenderTexture transitionTexture2;
        private static Canvas transitionCanvas;
        private static UnityEngine.UI.RawImage transitionImage;
        private static FingersExampleCoRoutineScript coRoutineScript;

        private static void ShowTransitionImage()
        {
            if (transitionTexture1 == null)
            {
                transitionTexture1 = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            }
            if (transitionTexture2 == null)
            {
                transitionTexture2 = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            }
            if (transitionCanvas == null)
            {
                GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("FingersExampleTransitionCanvas"));
                GameObject.DontDestroyOnLoad(obj);
                transitionCanvas = obj.GetComponent<Canvas>();
                transitionImage = transitionCanvas.transform.Find("FingersExampleTransitionImage").GetComponent<RawImage>();
                coRoutineScript = obj.GetComponent<FingersExampleCoRoutineScript>();
            }
            transitionImage.material.SetTexture("_MainTex1", transitionTexture1);
            transitionImage.material.SetTexture("_MainTex2", transitionTexture2);
            transitionCanvas.gameObject.SetActive(true);
        }

        private static void HideTransitionImage(Camera currentCamera, Camera newCamera)
        {
            // upon animation completion
            transitionCanvas.gameObject.SetActive(false);
            transitionImage.material.SetTexture("_MainTex1", null);
            transitionImage.material.SetTexture("_MainTex2", null);
            if (currentCamera != null)
            {
                currentCamera.targetTexture = null;
            }
            newCamera.targetTexture = null;
            GameObject.DestroyImmediate(transitionTexture1);
            GameObject.DestroyImmediate(transitionTexture2);
        }

        private static IEnumerator TransitionCoRoutine(Camera currentCamera, Camera newCamera, Scene currentScene, Scene newScene, bool unload)
        {
            newCamera.enabled = true;
            float elapsed = 0.0f;
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                transitionImage.material.SetFloat("_Fade", Mathf.Min(1.0f, elapsed / AnimationDuration));
                yield return null;
            }
            if (unload)
            {
                SceneManager.UnloadSceneAsync(currentScene).completed += (_op) =>
                {
                    HideTransitionImage(currentCamera, newCamera);
                };
            }
            else
            {
                currentCamera.enabled = false;
                HideTransitionImage(currentCamera, newCamera);
                foreach (GameObject obj in currentScene.GetRootGameObjects())
                {
                    obj.SetActive(false);
                }
            }
            yield break;
        }

        private static void PerformTransition(Scene currentScene, Scene newScene, bool unload)
        {
            ShowTransitionImage();
            Camera currentCamera = GetCameraFromScene(currentScene);
            Camera newCamera = GetCameraFromScene(newScene);
            currentCamera.targetTexture = transitionTexture1;
            newCamera.targetTexture = transitionTexture2;
            Debug.LogFormat("Transition from {0} to {1} with unload {2}", currentScene.name, newScene.name, unload);
            coRoutineScript.StartCoroutine(TransitionCoRoutine(currentCamera, newCamera, currentScene, newScene, unload));
        }

        private static Camera GetCameraFromScene(Scene scene)
        {
            if (!scene.isLoaded)
            {
                return null;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            Camera currentCamera = null;
            foreach (GameObject root in roots)
            {
                root.SetActive(true);
                if (currentCamera == null)
                {
                    currentCamera = root.GetComponentInChildren<Camera>();
                }
            }
            if (currentCamera == null)
            {
                Debug.LogError("Missing camera in scene " + scene.name);
            }

            return currentCamera;
        }

        /// <summary>
        /// Push a new scene
        /// </summary>
        /// <param name="name">Scene name</param>
        /// <param name="animated">True for animated transition, false for instant</param>
        public static void PushScene(string name, bool animated = true)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            AsyncOperation op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            if (animated)
            {
                op.completed += (_op) =>
                {
                    Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                    PerformTransition(currentScene, newScene, false);
                };
            }            
        }

        /// <summary>
        /// Pop the last pushed scene
        /// </summary>
        /// <param name="animated">True for animated transition, false for instant</param>
        public static void PopScene(bool animated = true)
        {
            Scene currentScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            if (animated)
            {
                Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 2);
                PerformTransition(currentScene, newScene, true);
            }
            else
            {
                SceneManager.UnloadSceneAsync(currentScene);
            }
        }
    }
}