using System;
using Mirror;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
	public class CharacterMovement : NetworkBehaviour
	{
		private const float GravityScale = 3;
		private const float Epsilon = 0.1f;
		private const float AccelerationTime = 0.5f;
		private const float SprintMultiplier = 1.75f;

		[SerializeField] private Character character;
		[SerializeField] private Rigidbody rigidBody;
		[SerializeField] private CapsuleCollider hitBox;

		private static readonly Vector3 HorizontalMask = new Vector3(1, 0, 1);

		private IInputService _inputService;
		private MapProvider _mapProvider;

		private Vector3 _desiredMovementDirection;
		private float _jumpHeight;
		private bool _jumpRequested;
		private float _speed;
		private bool _resetHorizontalVelocity;

		private float _leftBoarderAngle;
		private float _rightBoarderAngle;
		[field: SerializeField] public Transform ForwardDirectionObject { get; private set; }

		public MovementState State { get; private set; }

		[Inject]
		private void Construct(IInputService inputService, MapProvider mapProvider)
		{
			_inputService = inputService;
			_mapProvider = mapProvider;
		}

		public Vector3 GetHorizontalVelocity()
		{
			return Vector3.Scale(HorizontalMask, rigidBody.linearVelocity);
		}

		private void Update()
		{
			if (character.Characteristics == null || !isLocalPlayer)
			{
				return;
			}

			Move(_inputService.Axis, character.Characteristics.Speed, _inputService.IsSprintButtonHold());

			if (_inputService.IsJumpButtonDown())
			{
				Jump(character.Characteristics.JumpHeight);
			}
		}

		private void FixedUpdate()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			Tick();
		}

		private bool CanClimb()
		{
			Vector3 slidingPosition = transform.position + Sign(_desiredMovementDirection) * hitBox.radius
			                                             + _desiredMovementDirection * (GetHorizontalVelocity().magnitude * Time.fixedDeltaTime)
			                                             + Vector3.up;
			Vector3Ushort slidingVoxelPosition = Vector3Ushort.FloorToUshort(slidingPosition - Vector3.up / 2);
			var slidingVoxel = new Voxel(slidingVoxelPosition, _mapProvider.Map.CurrentValue.GetVoxelByGlobalPosition(slidingVoxelPosition));
			return slidingVoxel.Data.IsSolid() && !Physics.CheckCapsule(slidingPosition + Vector3.up * (hitBox.radius + Epsilon),
				slidingPosition + Vector3.up * (hitBox.height - hitBox.radius - Epsilon), hitBox.radius, LayerMasks.BuildMask);
		}

		private bool IsGrounded()
		{
			bool isGrounded = Physics.CheckBox(rigidBody.position,
				new Vector3(hitBox.radius / 2, Epsilon, hitBox.radius / 2),
				Quaternion.identity, LayerMasks.BuildMask);
			return isGrounded;
		}

		private void Move(Vector2 direction, float speed, bool sprint)
		{
			Vector3 forward = Vector3.ProjectOnPlane(ForwardDirectionObject.position - transform.position, Vector3.up);
			Vector3 right = Quaternion.Euler(0f, 90, 0f) * forward;
			Vector3 horizontalDirection = (direction.x * forward + direction.y * right).normalized;

			if (Vector3.Dot(_desiredMovementDirection, horizontalDirection) <= 0)
			{
				_resetHorizontalVelocity = true;
			}

			_desiredMovementDirection = horizontalDirection;
			_speed = speed;

			if (sprint)
			{
				_speed *= SprintMultiplier;
			}
		}

		private void Jump(float jumpHeight)
		{
			_jumpRequested = true;
			_jumpHeight = jumpHeight;
		}

		private void Tick()
		{
			if (IsGrounded())
			{
				if (Mathf.Round(rigidBody.position.y) <= 1)
				{
					State = MovementState.OnWater;
				}
				else
				{
					State = MovementState.OnGround;
				}
			}
			else
			{
				State = MovementState.InAir;
			}

			if (State == MovementState.OnGround || State == MovementState.OnWater)
			{
				if (_jumpRequested)
				{
					rigidBody.AddForce(Mathf.Sqrt(-2 * GravityScale * Physics.gravity.y * _jumpHeight) * Vector3.up, ForceMode.Impulse);
				}

				if (CanClimb())
				{
					rigidBody.position += Vector3.up + hitBox.radius * _desiredMovementDirection;
					rigidBody.linearVelocity =
						new Vector3(rigidBody.linearVelocity.x / 2, rigidBody.linearVelocity.y, rigidBody.linearVelocity.z / 2);
				}
			}

			if (State == MovementState.InAir)
			{
				rigidBody.AddForce(GravityScale * Physics.gravity);
			}

			Vector3 horizontalVelocity = Vector3.zero;

			if (!_resetHorizontalVelocity)
			{
				float acceleration = _speed / AccelerationTime;
				horizontalVelocity =
					Mathf.Min(GetHorizontalVelocity().magnitude + acceleration * Time.fixedDeltaTime, _speed) * _desiredMovementDirection;
			}

			rigidBody.linearVelocity = horizontalVelocity + rigidBody.linearVelocity.y * Vector3.up;

			_jumpRequested = false;
			_resetHorizontalVelocity = false;
		}

		private Vector3 Sign(Vector3 vector)
		{
			return new Vector3(MathF.Sign(vector.x), MathF.Sign(vector.y), MathF.Sign(vector.z));
		}
	}

	public enum MovementState
	{
		InAir,
		OnGround,
		OnWater
	}
}
