using System;
using Infrastructure.Services.Input;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerMovement : NetworkBehaviour
    {
        private const float JumpHeightOffset = 0.5f;
        private const float AccelerationTime = 1f;
        private const float GravityScale = 3;

        private static readonly Vector3 HorizontalMask = new(1, 0, 1);

        [SerializeField]
        private new Rigidbody rigidbody;

        [SerializeField]
        private Transform bodyOrientation;

        [SerializeField]
        private CapsuleCollider hitBox;

        private IInputService _inputService;

        private float _speed;
        private float _jumpHeight;

        private Vector3 _desiredDirection;
        private bool _jumpRequested;
        private bool _shouldResetHorizontalVelocity;

        public void Construct(IInputService inputService, float speed, float jumpHeight)
        {
            _inputService = inputService;
            _speed = speed;
            _jumpHeight = jumpHeight + JumpHeightOffset;
        }

        private void Update()
        {
            var axis = _inputService.Axis;
            var horizontalDirection = (axis.x * bodyOrientation.forward + axis.y * bodyOrientation.right).normalized;
            if (Vector3.Dot(_desiredDirection, horizontalDirection) <= 0)
            {
                _shouldResetHorizontalVelocity = true;
            }

            _desiredDirection = horizontalDirection;

            if (_inputService.IsJumpButtonDown())
            {
                _jumpRequested = true;
            }
        }

        private void FixedUpdate()
        {
            if (IsGrounded())
            {
                if (_jumpRequested)
                {
                    rigidbody.AddForce(Mathf.Sqrt(-2 * GravityScale * Physics.gravity.y * _jumpHeight) * Vector3.up,
                        ForceMode.Impulse);
                }
            }
            else
            {
                rigidbody.AddForce(GravityScale * Physics.gravity);
            }

            rigidbody.AddForce(-GetHorizontalVelocity() / Time.deltaTime);

            if (!_shouldResetHorizontalVelocity)
            {
                rigidbody.AddForce(_desiredDirection * GetHorizontalVelocity().magnitude / Time.deltaTime);
                rigidbody.AddForce(
                    Math.Min((_speed - GetHorizontalVelocity().magnitude) / Time.deltaTime,
                        _speed / AccelerationTime) * _desiredDirection);
            }

            _jumpRequested = false;
            _shouldResetHorizontalVelocity = false;
        }

        private bool IsGrounded()
        {
            var isGrounded = Physics.CheckBox(rigidbody.position + hitBox.height / 2 * Vector3.down,
                new Vector3(hitBox.radius / 2, Constants.Epsilon, hitBox.radius / 2),
                Quaternion.identity, Constants.buildMask);
            return isGrounded;
        }

        private Vector3 GetHorizontalVelocity()
        {
            return Vector3.Scale(HorizontalMask, rigidbody.velocity);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(rigidbody.position + hitBox.height / 2 * Vector3.down,
                new Vector3(hitBox.radius, 2 * Constants.Epsilon, hitBox.radius));
        }
    }
}