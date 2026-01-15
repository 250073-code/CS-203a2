using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        // Input System Variables
        private PlayerInputAction _inputDrone;
        private Vector2 _moveInput;
        private float _thrustInput;
        private float _rotationInput;
        [SerializeField]
        private float _tiltAmount = 30f;
        [SerializeField]
        private float _tiltSpeed = 5f;
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightMode;

        // Actions
        private void Awake()
        {
            _inputDrone = new PlayerInputAction();

            _inputDrone.DroneControl.Control.performed += ctx =>
            {
                _moveInput = ctx.ReadValue<Vector2>();
            };
            _inputDrone.DroneControl.Control.canceled += ctx =>
            {
                _moveInput = Vector2.zero;
            };

            _inputDrone.DroneControl.Thrust.performed += ctx =>
            {
                _thrustInput = ctx.ReadValue<float>();
            };
            _inputDrone.DroneControl.Thrust.canceled += ctx =>
            {
                _thrustInput = 0;
            };

            _inputDrone.DroneControl.Rotation.performed += ctx =>
            {
                _rotationInput = ctx.ReadValue<float>();
            };
            _inputDrone.DroneControl.Rotation.canceled += ctx =>
            {
                _rotationInput = 0;
            };
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
            // Enable the Input System
            _inputDrone.DroneControl.Enable();
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);            
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    _inFlightMode = false;
                    onExitFlightMode?.Invoke();
                    ExitFlightMode();
                }
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            transform.Rotate(transform.up, _rotationInput * _speed / 2);

            //Old Code

            /*if (Input.GetKey(KeyCode.LeftArrow))
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }*/
        }

        private void CalculateMovementFixedUpdate()
        {
            _rigidbody.AddForce(_thrustInput * transform.up * _speed, ForceMode.Acceleration);

            //Old Code
            
            /*if (Input.GetKey(KeyCode.Space))
            {
                _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.V))
            {
                _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            }*/
        }

        private void CalculateTilt()
        {
            float tiltX = _moveInput.y * _tiltAmount;
            float tiltZ = -_moveInput.x * _tiltAmount;

            Quaternion targetRotation = Quaternion.Euler(tiltX, transform.localRotation.eulerAngles.y, tiltZ);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * _tiltSpeed);

            //Old Code

            /*if (Input.GetKey(KeyCode.A)) 
                transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
            else if (Input.GetKey(KeyCode.D))
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            else if (Input.GetKey(KeyCode.W))
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (Input.GetKey(KeyCode.S))
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            else 
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);*/
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
            // Disable the Input System
            _inputDrone.DroneControl.Disable();
        }
    }
}
