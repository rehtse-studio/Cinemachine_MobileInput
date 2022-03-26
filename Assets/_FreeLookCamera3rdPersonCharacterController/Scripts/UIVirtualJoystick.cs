using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace RehtseStudio.FreeLookCamera3rdPersonCharacterController.Scripts
{
    public class UIVirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private Vector2 _playerJoystickVectorOutput;

        [Header("Joysticks Images RectTransform Reference")]
        [SerializeField] RectTransform _joystickContainerRectTransform;
        [SerializeField] RectTransform _joystickHandleRectTransform;
        [SerializeField] GameObject _joystickHandleObject;

        [Header("Joystick Settings")]
        [SerializeField] float _joystickMovementRange = 50f;
        float _magnitudeMultiplier = 1f;

        [Header("Position Reference For The Joystick")]
        private Vector2 _touchPosition;
        private Vector2 _clampedPosition;
        private Vector2 _outputPosition;

        private void Start()
        {
            SetupJoystickHandle();
            _joystickHandleObject.SetActive(false);
        }
        private void SetupJoystickHandle()
        {
            if (_joystickHandleRectTransform)
            {
                UpdateJoystickHandleRectTransformPosition(Vector2.zero);
            }
        }
        public void OnPointerUp(PointerEventData _onPointerUpEventData)
        {
            OutputVectorValue(Vector2.zero);

            _joystickHandleObject.SetActive(false);

            if (_joystickHandleRectTransform)
            {
                UpdateJoystickHandleRectTransformPosition(Vector2.zero);
            }
        }

        private void OutputVectorValue(Vector2 outputValue)
        {
            _playerJoystickVectorOutput = outputValue;
        }

        public Vector2 PlayerJoystickOutputVector()
        {
            return _playerJoystickVectorOutput;
        }

        public void OnPointerDown(PointerEventData _onPointerDownEventData)
        {
            OnDrag(_onPointerDownEventData);
            _joystickHandleObject.SetActive(true);
        }

        public void OnDrag(PointerEventData _onDragEvenData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_joystickContainerRectTransform, _onDragEvenData.position, _onDragEvenData.pressEventCamera, out _touchPosition);
            _touchPosition = ApplySizeDelta(_touchPosition);
            _clampedPosition = ClampValuesToMagnitude(_touchPosition);
            _outputPosition = _touchPosition;

            OutputVectorValue(_outputPosition * _magnitudeMultiplier);

            if (_joystickHandleRectTransform)
            {
                UpdateJoystickHandleRectTransformPosition(_clampedPosition * _joystickMovementRange);
            }
        }

        Vector2 ApplySizeDelta(Vector2 position)
        {
            float x = (position.x / _joystickContainerRectTransform.sizeDelta.x) * 5.5f;
            float y = (position.y / _joystickContainerRectTransform.sizeDelta.y) * 5.5f;
            return new Vector2(x, y);
        }

        Vector2 ClampValuesToMagnitude(Vector2 position)
        {
            return Vector2.ClampMagnitude(position, 1);
        }

        private void UpdateJoystickHandleRectTransformPosition(Vector2 newPosition)
        {
            _joystickHandleRectTransform.anchoredPosition = newPosition;
        }
    }
}

