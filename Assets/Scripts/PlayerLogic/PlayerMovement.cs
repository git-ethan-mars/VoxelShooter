using Infrastructure.Services;
using Infrastructure.Services.Input;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerMovement : NetworkBehaviour
    {
        private float _speed;
        private float _jumpMultiplier;
        private CharacterController _characterController;
        private IInputService _inputService;
        private Vector3 _movementDirection;
        private float _jumpSpeed;
        private const float Gravity = -30f;
        private float _speedModifier = 1f;
        private const float NormalSpeed = 1f;
        private const float SneakingSpeed = 0.7f;
        private const float SquattingSpeed = 0.4f;
        private const float SquattingSize = 0.5f;
        private float _stayingHeight;
        private float _squattingHeight;
        private Vector3 _stayingColliderCenter;
        private Vector3 _squattingColliderCenter;

        public void Construct(float speed, float jumpMultiplier)
        {
            _speed = speed;
            _jumpMultiplier = jumpMultiplier;
        }

        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
        }

        public void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _stayingHeight = _characterController.height;
            _squattingHeight = _stayingHeight - SquattingSize;
            _stayingColliderCenter = _characterController.center;
            _squattingColliderCenter = new Vector3(_stayingColliderCenter.x,
                _stayingColliderCenter.y - SquattingSize / 2, _stayingColliderCenter.z);
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            HandleSneaking();
            HandleSquatting();
            
            var axis = _inputService.Axis;
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
                _characterController.height = _squattingHeight;
                _characterController.center = _squattingColliderCenter;
            }

            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                _speedModifier = NormalSpeed;
                _characterController.height = _stayingHeight;
                _characterController.center = _stayingColliderCenter;
            }
        }
    }
}