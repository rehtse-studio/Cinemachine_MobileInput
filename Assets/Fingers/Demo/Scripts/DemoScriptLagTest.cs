using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    public class DemoScriptLagTest : MonoBehaviour
    {
        private Vector2 offset;

        private void CheckOffset(Vector2 pos)
        {
            offset = pos - (Vector2)transform.position;
        }

        private void Update()
        {
            if (FingersScript.Instance.MousePresent)
            {
                if (FingersScript.Instance.IsMouseDownThisFrame(0))
                {
                    CheckOffset(FingersScript.Instance.MousePosition);
                }
                else if (FingersScript.Instance.IsMouseDown(0))
                {
                    transform.position = FingersScript.Instance.MousePosition - offset;
                }
                else
                {
                    offset = Vector2.zero;
                }
            }
            else if (FingersScript.Instance.TouchCount > 0)
            {
                DigitalRubyShared.GestureTouch touch = FingersScript.Instance.GetTouch(0);
                Vector2 pos = new Vector2(touch.X, touch.Y);
                if (touch.TouchPhase == DigitalRubyShared.TouchPhase.Began)
                {
                    CheckOffset(pos);
                }
                transform.position = pos - offset;
            }
            else
            {
                offset = Vector2.zero;
            }
        }
    }
}
