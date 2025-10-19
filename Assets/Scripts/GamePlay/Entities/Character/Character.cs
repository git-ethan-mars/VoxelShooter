using Data;
using GamePlay.Core;
using GamePlay.MapFeatures;
using Mirror;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class Character : NetworkBehaviour, IDamageaeble, IEntity, IDamageVisitor
	{
		[SerializeField] private CharacterMovement movement;
		[SerializeField] private CharacterVisual visual;
		[SerializeField] private Bounds localBounds;
		[SerializeField] private AudioSource continuousAudio;
		[SerializeField] private AudioSource stepAudio;
		[SerializeField] private AudioData stepAudioData;

		[SyncVar] private GameClass _gameClass;
		[SyncVar] private string _nickName;

		private IInputService _inputService;
		private IStaticDataService _staticData;
		private CharacterProvider _characterProvider;
		private EntityContainerService _entityContainer;
		private IParticleFactory _particleFactory;

		[Inject]
		private void Construct(IInputService inputService, IStaticDataService staticData, CharacterProvider characterProvider,
			EntityContainerService entityContainer, IParticleFactory particleFactory)
		{
			_inputService = inputService;
			_staticData = staticData;
			_characterProvider = characterProvider;
			_entityContainer = entityContainer;
			_particleFactory = particleFactory;
		}

		private void Start()
		{
			_entityContainer.Add(this);
		}

		private void OnDestroy()
		{
			_entityContainer.Remove(this);
		}

		[field: SerializeField] public HealthSystem HealthSystem { get; private set; }
		[field: SerializeField] public Inventory Inventory { get; private set; }
		public Characteristics Characteristics { get; private set; }
		public Vector3 ForwardVector => movement.ForwardVector;

		[Server]
		public void Initialize(GameClass gameClass, string nickName)
		{
			_gameClass = gameClass;
			_nickName = nickName;
		}

		public override void OnStartClient()
		{
			Characteristics = _staticData.GetCharacteristics(_gameClass);
			visual.SetNickName(_nickName);
			name = $"{_nickName} [ID: ..., CLASS: {_gameClass}]";
		}

		public override void OnStartLocalPlayer()
		{
			movement.Initialize();

			visual.TurnOffBodyRender();
			visual.TurnOffNickName();

			_characterProvider.Character.Value = this;
		}

		private void Update()
		{
			if (!isLocalPlayer || Characteristics == null)
			{
				return;
			}

			movement.Move(_inputService.Axis, Characteristics.Speed);

			if (_inputService.IsJumpButtonDown())
			{
				movement.Jump(Characteristics.JumpHeight);
			}

			movement.Rotate(_inputService.MouseAxis);
		}

		private void FixedUpdate()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			movement.Tick();
		}

		public override void OnStopLocalPlayer()
		{
			base.OnStopLocalPlayer();

			_characterProvider.Character.Value = null;
		}

		public void Heal(int heal)
		{
			HealthSystem.Increase(heal);
		}

		public void Damage(int damage)
		{
			HealthSystem.Decrease(damage);
		}

		public void Visit(RangeWeapon rangeWeapon, RaycastHit hit)
		{
			Damage(rangeWeapon.Configure.Damage);
			ParticleSystem blood = _particleFactory.CreateBlood(hit.point, Quaternion.LookRotation(hit.normal));
		}

		public void Visit(MeleeWeapon meleeWeapon, bool isStrongHit, RaycastHit hit)
		{
			Damage(meleeWeapon.Configure.DamageToPlayer);
		}

		public void Visit(ExplosionData explosionData, Vector3 center)
		{
			if (Vector3.Distance(center, transform.position) >= explosionData.radius)
			{
				return;
			}

			int damage = CalculateLinearDamage(center, explosionData.radius, explosionData.damage);

			if (damage == 0)
			{
				return;
			}

			Damage(damage);
		}

		private int CalculateLinearDamage(Vector3 explosionCenter, int radius, int damage)
		{
			return (int)((1 - Vector3.Distance(transform.position, explosionCenter) / radius) * damage);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		}

		public Bounds Bounds => new Bounds(transform.position + localBounds.center, localBounds.size);
	}
}