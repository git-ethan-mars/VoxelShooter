using System;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField]
        private CapsuleCollider hitBox;

        [SerializeField]
        private Rigidbody rigidBody;

        private const float AccelerationTime = 0.3f;
        private const float GravityScale = 3;

        private static readonly Vector3 HorizontalMask = new(1, 0, 1);

        private float _speed;
        private float _jumpHeight;

        private Vector3 _desiredDirection;
        private bool _jumpRequested;
        private bool _shouldResetHorizontalVelocity;

        public void Construct(float speed, float jumpHeight)
        {
            _speed = speed;
            _jumpHeight = jumpHeight;
        }

        [Command]
        public void CmdMove(Vector2 direction, Vector3 forward, Vector3 right)
        {
            var horizontalDirection = (direction.x * forward + direction.y * right)
                .normalized;
            if (Vector3.Dot(_desiredDirection, horizontalDirection) <= 0)
            {
                _shouldResetHorizontalVelocity = true;
            }

            _desiredDirection = horizontalDirection;
        }

        [Command]
        public void CmdJump()
        {
            _jumpRequested = true;
        }

        public Vector3 GetHorizontalVelocity()
        {
            return Vector3.Scale(HorizontalMask, rigidBody.velocity);
        }

        public void FixedUpdate()
        {
            if (!isServer)
            {
                return;
            }

            if (IsGrounded())
            {
                if (_jumpRequested)
                {
                    rigidBody.AddForce(Mathf.Sqrt(-2 * GravityScale * Physics.gravity.y * _jumpHeight) * Vector3.up,
                        ForceMode.Impulse);
                }
            }
            else
            {
                rigidBody.AddForce(GravityScale * Physics.gravity);
            }

            rigidBody.AddForce(-GetHorizontalVelocity() / Time.deltaTime);

            if (!_shouldResetHorizontalVelocity)
            {
                rigidBody.AddForce(_desiredDirection * GetHorizontalVelocity().magnitude / Time.deltaTime);
                rigidBody.AddForce(
                    Math.Min((_speed - GetHorizontalVelocity().magnitude) / Time.deltaTime,
                        _speed / AccelerationTime) * _desiredDirection);
            }

            _jumpRequested = false;
            _shouldResetHorizontalVelocity = false;
        }

        public bool IsGrounded()
        {
            var isGrounded = Physics.CheckBox(rigidBody.position + hitBox.height / 2 * Vector3.down,
                new Vector3(hitBox.radius / 2, Constants.Epsilon, hitBox.radius / 2),
                Quaternion.identity, Constants.buildMask);
            return isGrounded;
        }
    }
}