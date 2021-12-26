using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RehtseStudio.FreeLookCamera3rdPersonCharacterController.Scripts
{
    public class PlayerInputsWithFreeLookCamera : MonoBehaviour
    {
        [Header("Player Inputs")]
        private PlayerInput _inputActions;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _runAction;

        private void Start()
        {
            _inputActions = GetComponent<PlayerInput>();
            _moveAction = _inputActions.actions["Move"];
            _lookAction = _inputActions.actions["Look"];
            _runAction = _inputActions.actions["Run"];
        }

        public Vector2 OnMove()
        {
            return _moveAction.ReadValue<Vector2>();
        }

        public Vector2 OnLook()
        {
            return _lookAction.ReadValue<Vector2>();
        }

        public bool OnRun()
        {
            return _runAction.ReadValue<float>() != 0 ? true : false;
        }
    }
}

