using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace RehtseStudio.FreeLookCamera3rdPersonCharacterController.Scripts
{
    public class UITouchPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private Vector2 _playerTouchVectorOutput;
        private bool _isPlayerTouchingPanel;
        private Touch _myTouch;
        private int _touchID;

        private void FixedUpdate()
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    _myTouch = Input.GetTouch(i);
                    if (_isPlayerTouchingPanel)
                    {
                        if (_myTouch.fingerId == _touchID)
                        {
                            if (_myTouch.phase != TouchPhase.Moved)
                                OutputVectorValue(Vector2.zero);
                        }
                    }
                }
            }
        }

        private void OutputVectorValue(Vector2 outputValue)
        {
            _playerTouchVectorOutput = outputValue;
        }

        public Vector2 PlayerJoystickOutputVector()
        {
            return _playerTouchVectorOutput;
        }

        public void OnPointerUp(PointerEventData _onPointerUpData)
        {
            OutputVectorValue(Vector2.zero);
            _isPlayerTouchingPanel = false;
        }

        public void OnPointerDown(PointerEventData _onPointerDownData)
        {
            OnDrag(_onPointerDownData);
            _touchID = _myTouch.fingerId;
            _isPlayerTouchingPanel = true;
        }

        public void OnDrag(PointerEventData _onDragData)
        {
            OutputVectorValue(new Vector2(_onDragData.delta.normalized.x, _onDragData.delta.normalized.y));
        }
    }
}

