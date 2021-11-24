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
    /// Demo a drag and swipe away
    /// </summary>
    public class DemoScriptDragSwipe : MonoBehaviour
    {
        /// <summary>
        /// Speed (in units) that will cause the card to get swiped off the screen
        /// </summary>
        [Tooltip("Speed (in units) that will cause the card to get swiped off the screen")]
        [Range(25.0f, 100.0f)]
        public float SwipeAwaySpeed = 60.0f;

        /// <summary>
        /// Reduce swipe away velocity by this amount
        /// </summary>
        [Tooltip("Reduce swipe away velocity by this amount")]
        [Range(0.1f, 1.0f)]
        public float SwipeVelocityDampening = 0.5f;

        private LongPressGestureRecognizer longPress;
        private Transform draggingCard;
        private Vector3 dragOffset;
        private readonly List<Transform> swipedCards = new List<Transform>();

        private void Start()
        {
            // create a long press gesture to drag and swipe cards around
            longPress = new LongPressGestureRecognizer();
            longPress.StateUpdated += LongPress_StateUpdated;
            FingersScript.Instance.AddGesture(longPress);
            FingersScript.Instance.ShowTouches = true;
        }

        private void Update()
        {
            // ESC reloads the scene
            if (FingersScript.Instance.IsKeyDownThisFrame(KeyCode.Escape))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0, UnityEngine.SceneManagement.LoadSceneMode.Single);
                return;
            }

            // clean-up any swiped cards
            for (int i = swipedCards.Count - 1; i >= 0; i--)
            {
                Renderer r = swipedCards[i].GetComponent<Renderer>();
                if (!r.isVisible)
                {
                    GameObject.Destroy(r.gameObject);
                    swipedCards.RemoveAt(i);
                }                
            }
        }

        private void LongPress_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began)
            {
                // raycast out for the card - this let's us have multiple cards
                Vector3 gestureWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(gesture.FocusX, gesture.FocusY, 0.0f));
                Collider2D hit = Physics2D.OverlapPoint(gestureWorldPos);

                // see if we found a card
                if (hit != null && !swipedCards.Contains(hit.transform))
                {
                    // set the draggingCard variable to the card transform, this let's us move it around in the "Executing" state
                    Debug.Log("Found card, beginning drag...");
                    draggingCard = hit.GetComponent<Transform>();
                    draggingCard.localScale = Vector3.one * 1.2f;
                    draggingCard.transform.SetSiblingIndex(draggingCard.transform.parent.childCount - 1);
                    SpriteRenderer renderer = draggingCard.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        // put the picked up card on top of everything else
                        renderer.transform.SetSiblingIndex(renderer.transform.parent.childCount - 1);
                        for (int i = 0; i < renderer.transform.parent.childCount; i++)
                        {
                            SpriteRenderer other = renderer.transform.parent.GetChild(i).GetComponent<SpriteRenderer>();
                            if (other != null)
                            {
                                // make sure sort order and z position is set for the other cards to ensure they are in the right
                                // spot in the pile
                                other.sortingOrder = i;
                                Vector3 pos = other.transform.position;
                                pos.z = ((float)(renderer.transform.parent.childCount - i) * 0.01f);
                                other.transform.position = pos;
                            }
                        }
                    }
                }
                else
                {
                    // no card, reset the gesture and they must lift the touch and try again
                    Debug.Log("No card under gesture, resetting...");
                    gesture.Reset();
                }

                // apply an offset from the center of the card so it drags from wherever it was touched on the card
                dragOffset = draggingCard.position - Camera.main.ScreenToWorldPoint(new Vector3(gesture.FocusX, gesture.FocusY, 0.0f));
                dragOffset.z = 0.0f;
            }
            else if (gesture.State == GestureRecognizerState.Executing)
            {
                // if the gesture velocity is high enough, fling the card off screen
                float speed = longPress.Distance(gesture.VelocityX, gesture.VelocityY);
                if (speed >= SwipeAwaySpeed)
                {
                    // convert the screen units velocity to world velocity and apply to the card
                    draggingCard.localScale = Vector3.one;
                    Vector3 worldVelocityZero = Camera.main.ScreenToWorldPoint(Vector3.zero);
                    Vector3 worldVelocityGesture = Camera.main.ScreenToWorldPoint(new Vector3(gesture.VelocityX, gesture.VelocityY, 0.0f));
                    Vector3 worldVelocity = (worldVelocityGesture - worldVelocityZero) * 0.5f;
                    worldVelocity.z = 0.0f;
                    Rigidbody2D rb = draggingCard.GetComponent<Rigidbody2D>();
                    rb.velocity = worldVelocity;

                    // apply some random spin for fun
                    rb.angularVelocity = Random.Range(-1000.0f, 1000.0f);

                    // don't allow the card to be re-dragged while it flings away
                    swipedCards.Add(draggingCard);
                    draggingCard = null;

                    // reset gesture, the swipe away finishes the gesture
                    gesture.Reset();

                    Debug.LogFormat("Swiping card away at world velocity {0} (screen velocity units {1})", (Vector2)worldVelocity, new Vector2(gesture.VelocityX, gesture.VelocityY));
                }
                else
                {
                    // drag the card
                    Vector3 dragCurrent = Camera.main.ScreenToWorldPoint(new Vector3(gesture.FocusX, gesture.FocusY, 0.0f));
                    dragCurrent.z = draggingCard.transform.position.z;
                    draggingCard.position = dragCurrent + dragOffset;
                }
            }
            else
            {
                // if not begin or execute state, null out the dragging card
                if (draggingCard != null)
                {
                    draggingCard.localScale = Vector3.one;
                    SpriteRenderer renderer = draggingCard.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        //renderer.sortingOrder = savedSortOrder;
                    }
                }
                draggingCard = null;
            }
        }
    }
}
