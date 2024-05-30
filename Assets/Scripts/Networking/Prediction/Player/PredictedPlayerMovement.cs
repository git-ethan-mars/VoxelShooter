using System;
using Infrastructure.Services.Input;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Prediction.Player
{
    [RequireComponent(typeof(NetworkedPlayerMessenger))]
    public class PredictedPlayerMovement : Prediction<PlayerInput, PlayerState>
    {
        [SerializeField]
        private Rigidbody rigidBody;

        [SerializeField]
        private CapsuleCollider hitBox;

        [SerializeField]
        private Transform bodyOrientation;

        private IInputService _inputService;

        private const float AccelerationTime = 0.3f;
        private const float GravityScale = 3;
        private static readonly Vector3 HorizontalMask = new(1, 0, 1);

        private Vector3 _desiredDirection;
        private float _speed;
        private float _jumpHeight;
        private bool _isJumpButtonPressed;

        private Scene _mainScene;
        private Scene _idleScene;

        PhysicsScene _physicsScene;

        public void Construct(IInputService inputService, float speed, float jumpHeight)
        {
            _inputService = inputService;
            _speed = speed;
            _jumpHeight = jumpHeight;
        }

        private void Start()
        {
            _mainScene = gameObject.scene;
            GetOrCreateIdleScene();
            MoveToScene(_idleScene);
            Physics.autoSimulation = false;
        }

        protected override void Update()
        {
            base.Update();
            if (isLocalPlayer && _inputService.IsJumpButtonDown())
            {
                _isJumpButtonPressed = true;
            }
        }

        protected override PlayerInput GetInput(float deltaTime, uint currentTick)
        {
            var input = new PlayerInput(_inputService.Axis, bodyOrientation.forward, bodyOrientation.right,
                _isJumpButtonPressed, deltaTime, currentTick);
            _isJumpButtonPressed = false;
            return input;
        }

        protected override PlayerState RecordState(uint lastProcessedInputTick)
        {
            return new PlayerState(transform.position, rigidBody.velocity, lastProcessedInputTick);
        }

        protected override void SetState(PlayerState state)
        {
            transform.position = state.Position;
            rigidBody.velocity = state.Velocity;
        }

        protected override void ProcessInput(PlayerInput input)
        {
            MoveToScene(_mainScene);
            var horizontalDirection = input.Direction.x * input.Forward + input.Direction.y * input.Right;
            var shouldResetHorizontalVelocity = Vector3.Dot(_desiredDirection, horizontalDirection) <= 0;
            _desiredDirection = horizontalDirection;
            if (IsGrounded())
            {
                if (input.JumpButtonPressed)
                {
                    rigidBody.AddForce(Mathf.Sqrt(-2 * GravityScale * Physics.gravity.y * _jumpHeight) * Vector3.up,
                        ForceMode.Impulse);
                }
            }
            else
            {
                rigidBody.AddForce(GravityScale * Physics.gravity);
            }

            rigidBody.AddForce(-GetHorizontalVelocity() / NetworkServer.sendInterval);

            if (!shouldResetHorizontalVelocity)
            {
                rigidBody.AddForce(_desiredDirection * GetHorizontalVelocity().magnitude / NetworkServer.sendInterval);
                rigidBody.AddForce(
                    Math.Min((_speed - GetHorizontalVelocity().magnitude) / NetworkServer.sendInterval,
                        _speed / AccelerationTime) * _desiredDirection);
            }

            _physicsScene.Simulate(NetworkServer.sendInterval);
            MoveToScene(_idleScene);
        }

        public bool IsGrounded()
        {
            var isGrounded = Physics.CheckBox(rigidBody.position + hitBox.height / 2 * Vector3.down,
                new Vector3(hitBox.radius / 2, Constants.Epsilon, hitBox.radius / 2),
                Quaternion.identity, Constants.buildMask);
            return isGrounded;
        }

        public Vector3 GetHorizontalVelocity()
        {
            return Vector3.Scale(HorizontalMask, rigidBody.velocity);
        }

        private void GetOrCreateIdleScene()
        {
            _idleScene = SceneManager.GetSceneByName("Idle");

            if (!_idleScene.IsValid())
                CreateIdleScene();
        }

        private void CreateIdleScene()
        {
            var parameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
            _idleScene = SceneManager.CreateScene("Idle", parameters);
        }

        private void MoveToScene(Scene scene)
        {
            SceneManager.MoveGameObjectToScene(gameObject, scene);
        }
    }
}