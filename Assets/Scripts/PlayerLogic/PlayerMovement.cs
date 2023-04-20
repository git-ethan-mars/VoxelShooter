using Data;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SyncVar] private float _speed;
        [SyncVar] private float _jumpMultiplier;
        private CharacterController _characterController;
        private IInputService _inputService;
        private Vector3 _movementDirection;
        private float _jumpSpeed;
        private const float Gravity = -30f;
        
        public void Construct(PlayerCharacteristic characteristic)
        {
            _speed = characteristic.speed;
            _jumpMultiplier = characteristic.jumpMultiplier;
        }
        
        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
        }

        public override void OnStartLocalPlayer()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            var axis = _inputService.Axis.normalized;
            var playerTransform = transform;
            _movementDirection = (axis.x * playerTransform.forward + axis.y * playerTransform.right).normalized;
            if (_characterController.isGrounded)
            {
                _jumpSpeed = 0;
                if (_inputService.IsJumpButtonDown())
                {
                    _jumpSpeed = _jumpMultiplier;
                }
            }
            _jumpSpeed += Gravity * Time.deltaTime;
            Vector3 direction = new Vector3(_movementDirection.x * _speed * Time.deltaTime, _jumpSpeed * Time.deltaTime,
                _movementDirection.z * Time.deltaTime * _speed);
            _characterController.Move(direction);
        }
    }
}