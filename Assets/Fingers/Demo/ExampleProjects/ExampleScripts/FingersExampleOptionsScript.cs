using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    public class FingersExampleOptionsScript : MonoBehaviour
    {
        public void BackButtonClicked()
        {
            Debug.Log("Options back clicked");
            FingersExampleSceneTransitionScript.PopScene();
        }
    }
}
