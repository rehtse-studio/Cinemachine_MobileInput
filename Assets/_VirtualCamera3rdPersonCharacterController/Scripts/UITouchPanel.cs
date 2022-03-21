using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RehtseStudio.VirtualCamera3rdPersonCharacterController.Scripts
{
    public class UITouchPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private Vector2 _playerVectorOutput;
        private Touch _myTouch;
        private int _touchID;

        private void Update()
        {
            if(Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    _myTouch = Input.GetTouch(i);
                    if(_myTouch.fingerId == _touchID)
                    {
                        if (_myTouch.phase != TouchPhase.Moved)
                            OutputVectorValue(Vector2.zero);
                    }
                }
            }            
        }
        private void OutputVectorValue(Vector2 outputValue)
        {
            _playerVectorOutput = outputValue;
        }
        public Vector2 VectorOutput()
        {
            return _playerVectorOutput;
        }
       
        public void OnPointerUp(PointerEventData _onPointerUpData)
        {
            OutputVectorValue(Vector2.zero);
        }

        public void OnPointerDown(PointerEventData _onPointerDownData)
        {
            OnDrag(_onPointerDownData);
            _touchID = _myTouch.fingerId;
        }

        public void OnDrag(PointerEventData _onDragData)
        {
            OutputVectorValue(new Vector2(_onDragData.delta.normalized.x, _onDragData.delta.normalized.y));
        }
    }
}

