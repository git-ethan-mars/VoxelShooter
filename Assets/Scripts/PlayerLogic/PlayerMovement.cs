using System;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerMovement
    {
        private const float AccelerationTime = 0.3f;
        private const float GravityScale = 3;

        private static readonly Vector3 HorizontalMask = new(1, 0, 1);

        private float _speed;
        private float _jumpHeight;

        private readonly Rigidbody _rigidbody;
        private readonly Transform _bodyOrientation;
        private readonly CapsuleCollider _hitBox;

        private Vector3 _desiredDirection;
        private bool _jumpRequested;
        private bool _shouldResetHorizontalVelocity;

        public PlayerMovement(CapsuleCollider hitBox, Rigidbody rigidbody, Transform bodyOrientation)
        {
            _hitBox = hitBox;
            _rigidbody = rigidbody;
            _bodyOrientation = bodyOrientation;
        }

        public void Move(Vector2 direction, float speed)
        {
            var horizontalDirection = (direction.x * _bodyOrientation.forward + direction.y * _bodyOrientation.right)
                .normalized;
            if (Vector3.Dot(_desiredDirection, horizontalDirection) <= 0)
            {
                _shouldResetHorizontalVelocity = true;
            }

            _desiredDirection = horizontalDirection;
            _speed = speed;
        }

        public void Jump(float jumpHeight)
        {
            _jumpRequested = true;
            _jumpHeight = jumpHeight;
        }

        public Vector3 GetHorizontalVelocity()
        {
            return Vector3.Scale(HorizontalMask, _rigidbody.velocity);
        }

        public void FixedUpdate()
        {
            if (IsGrounded())
            {
                if (_jumpRequested)
                {
                    _rigidbody.AddForce(Mathf.Sqrt(-2 * GravityScale * Physics.gravity.y * _jumpHeight) * Vector3.up,
                        ForceMode.Impulse);
                }
            }
            else
            {
                _rigidbody.AddForce(GravityScale * Physics.gravity);
            }

            _rigidbody.AddForce(-GetHorizontalVelocity() / Time.deltaTime);

            if (!_shouldResetHorizontalVelocity)
            {
                _rigidbody.AddForce(_desiredDirection * GetHorizontalVelocity().magnitude / Time.deltaTime);
                _rigidbody.AddForce(
                    Math.Min((_speed - GetHorizontalVelocity().magnitude) / Time.deltaTime,
                        _speed / AccelerationTime) * _desiredDirection);
            }

            _jumpRequested = false;
            _shouldResetHorizontalVelocity = false;
        }

        public bool IsGrounded()
        {
            var isGrounded = Physics.CheckBox(_rigidbody.position + _hitBox.height / 2 * Vector3.down + _hitBox.center,
                new Vector3(_hitBox.radius / 2, Constants.Epsilon, _hitBox.radius / 2),
                Quaternion.identity, Constants.buildMask);
            return isGrounded;
        }
    }
}