using System;
using Core;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Mirror;
using UnityEngine;

namespace GamePlay
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private float jumpMultiplayer;
        private Rigidbody Rigidbody { get; set; }
        private bool _isGrounded;
        private bool _isFell;
        private bool _isJumpButtonPressed;
        private float _previousFallingSpeed;
        private IInputService _inputService;
        private Vector3 _movementDirection;

        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
        }

        private void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.freezeRotation = true;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            ReadInput();
        }

        private void ReadInput()
        {
            var axis = _inputService.Axis.normalized;
            var playerTransform = transform;
            _movementDirection = (axis.x * playerTransform.forward + axis.y * playerTransform.right).normalized;
            if (_inputService.IsJumpButtonDown())
            {
                _isJumpButtonPressed = true;
            }
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;
            if (IsPlayerInAir)
            {
                _isFell = true;
                if (_isJumpButtonPressed && _isGrounded)
                {
                    Rigidbody.AddForce(Vector3.up * jumpMultiplayer, ForceMode.VelocityChange);
                }
            }
            else
            {
                _isFell = false;
            }

            _isJumpButtonPressed = false;
            _previousFallingSpeed = Rigidbody.velocity.y;
            Rigidbody.velocity = _movementDirection * speed + new Vector3(0, Rigidbody.velocity.y, 0);
        }

        private bool IsPlayerInAir => Math.Abs(_previousFallingSpeed - Rigidbody.velocity.y) < Constants.Epsilon;

        private void OnCollisionStay(Collision collision)
        {
            if (collision.collider.gameObject.GetComponent<ChunkRenderer>() && _isFell)
            {
                _isGrounded = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.gameObject.GetComponent<ChunkRenderer>())
            {
                _isGrounded = false;
            }
        }
    }
}