using Data;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Mirror;
using Networking.Synchronization;
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
        private float _speedModifier = 1f;
        private const float NormalSpeed = 1f;
        private const float SneakingSpeed = 0.7f;
        private const float SquattingSpeed = 0.4f;
        private const float NormalHeight = 2.07f;
        private const float SquattingHeight = 1.57f;
        private readonly Vector3 _normalColliderCenter = new (0, 0.3f, 0);
        private readonly Vector3 _squattingColliderCenter = new (0, 0, 0);

        public void Construct(PlayerCharacteristic characteristic)
        {
            _speed = characteristic.speed;
            _jumpMultiplier = characteristic.jumpMultiplier;
        }
        
        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
        }

        public void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
           
            HandleSneaking();
            HandleSquatting();

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
            Vector3 direction = new Vector3(_movementDirection.x * _speed * _speedModifier * Time.deltaTime, 
                _jumpSpeed * Time.deltaTime,
                _movementDirection.z * Time.deltaTime * _speed * _speedModifier);
            _characterController.Move(direction);
        }

        private void HandleSneaking()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _speedModifier = SneakingSpeed;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                _speedModifier = NormalSpeed;
            }
        }
        
        private void HandleSquatting()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _speedModifier = SquattingSpeed;
                _characterController.height = SquattingHeight;
                _characterController.center = _squattingColliderCenter;
            }

            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                _speedModifier = NormalSpeed;
                _characterController.height = NormalHeight;
                _characterController.center = _normalColliderCenter;
            }
        }
    }
}