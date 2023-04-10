using System;
using Infrastructure.Services.Input;
using Mirror;
using Rendering;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : NetworkBehaviour
    {
        private float _speed;
        private float _jumpMultiplier;
        private Rigidbody Rigidbody { get; set; }
        private bool _isGrounded;
        private bool _isFell;
        private bool _isJumpButtonPressed;
        private float _previousFallingSpeed;

        private IInputService _inputService;

        private Vector3 _movementDirection;


        public void Construct(IInputService inputService, float speed, float jumpMultiplier)
        {
            Debug.Log("Construct");
            _inputService = inputService;
            _speed = speed;
            _jumpMultiplier = jumpMultiplier;
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.freezeRotation = true;
        }

        private void Update()
        {
            Debug.Log("Update");
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
            Debug.Log("FixedUpdate");
            if (!isLocalPlayer) return;
            if (IsPlayerInAir)
            {
                _isFell = true;
                if (_isJumpButtonPressed && _isGrounded)
                {
                    Rigidbody.AddForce(Vector3.up * _jumpMultiplier, ForceMode.VelocityChange);
                }
            }
            else
            {
                _isFell = false;
            }

            _isJumpButtonPressed = false;
            _previousFallingSpeed = Rigidbody.velocity.y;
            Rigidbody.velocity = _movementDirection * _speed + new Vector3(0, Rigidbody.velocity.y, 0);
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