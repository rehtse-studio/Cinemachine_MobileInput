﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RehtseStudio.FreeLookCamera3rdPersonCharacterController.Scripts
{
    public class PlayerControllerWithFreeLookCamera : MonoBehaviour
    {
        [Header("Player Inputs")]
        private Vector2 _inputs;
        private PlayerInput _inputActions;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _runAction;

        [Header("Animation Section")]
        private Animator _animator;
        private int _animSpeedId;
        private int _animRunId;
        private float _animSpeed;

        private Rigidbody _rigidBody;
        private Vector3 _movement;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _playerSpeed;
        private float _standardSpeed = 2;
        private float _runSpeed = 5;

        [Header("Camera Reference")]
        [SerializeField] private GameObject _cinemachineTargetObject;
        private Camera _mainCamera;
        
        private void Start()
        {
            _rigidBody = GetComponent<Rigidbody>();

            _inputActions = GetComponent<PlayerInput>();
            _moveAction = _inputActions.actions["Move"];
            _lookAction = _inputActions.actions["Look"];
            _runAction = _inputActions.actions["Run"];

            _animator = GetComponent<Animator>();
            _animSpeedId = Animator.StringToHash("Speed");
            _animRunId = Animator.StringToHash("isPlayerRunning");

            _mainCamera = Camera.main;

        }

        private void FixedUpdate()
        {
            Movement();
        }

        #region MovementSection
        private void Movement()
        {
            _inputs = _moveAction.ReadValue<Vector2>();
            _playerSpeed = IsPlayerRunning() ? _runSpeed : _standardSpeed;
            _animSpeed = Mathf.Abs(_movement.x) + Mathf.Abs(_movement.y);

            if(_inputs != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(_inputs.x, _inputs.y) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, 0.12f);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

                _movement = targetDirection * _playerSpeed + new Vector3(0, _rigidBody.velocity.y, 0) ;

                Animations(IsPlayerRunning());
            }
            else
            {
                _playerSpeed = 0.0f;
                _movement = new Vector3(0, _rigidBody.velocity.y, 0);
                Animations(false);
            }
            _rigidBody.velocity = _movement;
            _animator.SetFloat(_animSpeedId, _animSpeed);
        }

        private bool IsPlayerRunning()
        {
            bool isPlayerRunning = _runAction.ReadValue<float>() != 0 ? true : false;
            return isPlayerRunning;
        }
        #endregion

        #region AnimationSection
        private void Animations(bool isRunning)
        {
            _animator.SetBool(_animRunId, isRunning);
        }
        #endregion

    }

}
