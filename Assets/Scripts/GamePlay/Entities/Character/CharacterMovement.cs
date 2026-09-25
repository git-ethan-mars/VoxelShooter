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

		private const float StepDuration = 0.12f;
		private const float StepMaxRiseTime = 0.25f;
		private const float StepHoldTime = 0.1f;
		private const float StepHeightTolerance = 0.005f;
		private const float StepClearance = 0.02f;
		private const float StepProbeMargin = 0.1f;
		private const float MinStepHeight = 0.05f;
		private const float MaxStepHeight = 1.05f;
		private const float StepPushDot = 0.5f;
		private const float StepSpaceInset = 0.05f;

		[SerializeField] private Character character;
		[SerializeField] private Rigidbody rigidBody;
		[SerializeField] private CapsuleCollider hitBox;

		private static readonly Vector3 HorizontalMask = new Vector3(1, 0, 1);
		private static readonly Collider[] OverlapBuffer = new Collider[16];

		private IInputService _inputService;
		private MapProvider _mapProvider;

		private Vector3 _desiredMovementDirection;
		private float _jumpHeight;
		private bool _jumpRequested;
		private float _speed;
		private bool _resetHorizontalVelocity;
		private int _groundedTicks;
		private bool _isSteppingUp;
		private float _stepTargetHeight;
		private float _stepElapsedTime;
		private float _stepHoldTimeLeft;
		private float _stepCarriedSpeed;

		[field: SerializeField] public Transform ForwardDirectionObject { get; private set; }

		public MovementState State { get; private set; }

		[Inject]
		private void Construct(IInputService inputService, MapProvider mapProvider)
		{
			_inputService = inputService;
			_mapProvider = mapProvider;
		}

		// Only the owner simulates its character; everywhere else NetworkTransform moves it,
		// so the body must not be pushed around by physics or interpolation.
		public override void OnStartServer()
		{
			SetSimulated(false);
		}

		public override void OnStartClient()
		{
			SetSimulated(false);
		}

		public override void OnStartLocalPlayer()
		{
			SetSimulated(true);
		}

		private void FixedUpdate()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			Tick();
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

		public Vector3 GetHorizontalVelocity()
		{
			return Vector3.Scale(HorizontalMask, rigidBody.linearVelocity);
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
			var forward = Vector3.ProjectOnPlane(ForwardDirectionObject.position - transform.position, Vector3.up);
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
			UpdateState();
			float verticalVelocity = rigidBody.linearVelocity.y;

			if (_isSteppingUp)
			{
				verticalVelocity = ContinueStepUp();
			}
			else if (State == MovementState.OnGround || State == MovementState.OnWater)
			{
				if (_jumpRequested)
				{
					rigidBody.AddForce(Mathf.Sqrt(-2 * GravityScale * Physics.gravity.y * _jumpHeight) * Vector3.up, ForceMode.Impulse);
				}
				else if (_groundedTicks > 1 && TryFindStep(out float stepHeight))
				{
					_isSteppingUp = true;
					_stepTargetHeight = stepHeight;
					_stepElapsedTime = 0.0f;
					_stepHoldTimeLeft = StepHoldTime;
					_stepCarriedSpeed = GetHorizontalVelocity().magnitude;
					verticalVelocity = ContinueStepUp();
				}
			}

			if (State == MovementState.InAir && !_isSteppingUp)
			{
				rigidBody.AddForce(GravityScale * Physics.gravity);
				verticalVelocity = rigidBody.linearVelocity.y;
			}

			Vector3 horizontalVelocity = Vector3.zero;

			if (!_resetHorizontalVelocity)
			{
				float acceleration = _speed / AccelerationTime;
				float currentSpeed = GetHorizontalVelocity().magnitude;

				// The step edge stops the body while it rises; keep the speed it had when it reached the step.
				if (_isSteppingUp)
				{
					currentSpeed = Mathf.Max(currentSpeed, _stepCarriedSpeed);
				}

				horizontalVelocity = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, _speed) * _desiredMovementDirection;
			}
			else
			{
				_stepCarriedSpeed = 0.0f;
			}

			rigidBody.linearVelocity = horizontalVelocity + verticalVelocity * Vector3.up;

			_jumpRequested = false;
			_resetHorizontalVelocity = false;
		}

		private void SetSimulated(bool isSimulated)
		{
			rigidBody.isKinematic = !isSimulated;
			rigidBody.interpolation = isSimulated ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
		}

		private void UpdateState()
		{
			if (IsGrounded())
			{
				State = Mathf.Round(rigidBody.position.y) <= 1 ? MovementState.OnWater : MovementState.OnGround;
				_groundedTicks++;
			}
			else
			{
				State = MovementState.InAir;
				_groundedTicks = 0;
			}
		}

		// Rises to the step top over StepDuration, then holds that height without gravity
		// until the body has moved over the step (or StepHoldTime runs out).
		private float ContinueStepUp()
		{
			float remainingHeight = _stepTargetHeight - rigidBody.position.y;

			if (remainingHeight > StepHeightTolerance && _stepElapsedTime < StepMaxRiseTime)
			{
				float remainingTime = Mathf.Max(StepDuration - _stepElapsedTime, Time.fixedDeltaTime);
				_stepElapsedTime += Time.fixedDeltaTime;
				return remainingHeight / remainingTime;
			}

			_stepHoldTimeLeft -= Time.fixedDeltaTime;

			if (State != MovementState.InAir || _stepHoldTimeLeft <= 0.0f)
			{
				_isSteppingUp = false;
			}

			return 0.0f;
		}

		private bool TryFindStep(out float stepHeight)
		{
			stepHeight = 0.0f;
			Vector3 direction = _desiredMovementDirection;

			if (direction.sqrMagnitude < 0.01f)
			{
				return false;
			}

			// Probe along the movement direction just past the collider edge.
			Vector3 feet = rigidBody.position;
			float probeDistance = hitBox.radius + StepProbeMargin + GetHorizontalVelocity().magnitude * Time.fixedDeltaTime;
			Vector3 probe = feet + direction * probeDistance;
			var stepVoxel = Vector3Int.FloorToInt(new Vector3(probe.x, feet.y + 0.5f, probe.z));

			if (!IsSolid(stepVoxel))
			{
				return false;
			}

			// Only step when the player is walking into the block, not brushing past it.
			Vector3 toStep = Vector3.Scale(stepVoxel + Map.WorldOffset - feet, HorizontalMask).normalized;

			if (Vector3.Dot(direction, toStep) < StepPushDot)
			{
				return false;
			}

			float rise = stepVoxel.y + 1 - feet.y;

			if (rise < MinStepHeight || rise > MaxStepHeight)
			{
				return false;
			}

			// The body needs room along the way up and where it ends up on the step.
			if (!IsBodySpaceFree(feet + Vector3.up * rise) ||
			    !IsBodySpaceFree(feet + Vector3.up * rise + direction * (hitBox.radius + StepProbeMargin)))
			{
				return false;
			}

			// A little above the step top, so the capsule does not catch the edge when it moves over it.
			stepHeight = stepVoxel.y + 1 + StepClearance;
			return true;
		}

		private bool IsBodySpaceFree(Vector3 feetPosition)
		{
			float radius = hitBox.radius - StepSpaceInset;
			Vector3 bottom = feetPosition + Vector3.up * (hitBox.radius + StepSpaceInset);
			Vector3 top = feetPosition + Vector3.up * (hitBox.height - hitBox.radius - StepSpaceInset);
			int count = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, OverlapBuffer, LayerMasks.MovementBlockMask,
				QueryTriggerInteraction.Ignore);

			for (int i = 0; i < count; i++)
			{
				if (!OverlapBuffer[i].transform.IsChildOf(transform))
				{
					return false;
				}
			}

			return true;
		}

		private bool IsSolid(Vector3Int position)
		{
			Map map = _mapProvider.Map.CurrentValue;
			return map.IsInsideMap(position) && map.GetVoxelByGlobalPosition((ushort)position.x, (ushort)position.y, (ushort)position.z).IsSolid();
		}
	}
}
