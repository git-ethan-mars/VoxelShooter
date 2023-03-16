using System;
using Core;
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
        private Vector3 _movementForce;
        private void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.freezeRotation = true;
        }

        private void Update()
        {
            ReadInput();
        }

        private void ReadInput()
        {
            if (!isLocalPlayer) return;
            var verticalInput = 0f;
            if (Input.GetKey(KeyCode.W))
                verticalInput += 1;
            if (Input.GetKey(KeyCode.S))
                verticalInput -= 1;
            var horizontalInput = 0f;
            if (Input.GetKey(KeyCode.D))
            {
                horizontalInput += 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                horizontalInput -= 1;
            }

            var playerTransform = transform;
            _movementForce = (verticalInput * playerTransform.forward + horizontalInput * playerTransform.right).normalized;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isJumpButtonPressed = true;
            }
        }

        private void FixedUpdate()
        {
            if (Math.Abs(_previousFallingSpeed - Rigidbody.velocity.y) < 1e-4)
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
            Rigidbody.AddForce(_movementForce * speed * 10f);
        }

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