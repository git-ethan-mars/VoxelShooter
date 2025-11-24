using Data;
using GamePlay.Core;
using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;
namespace GamePlay
{
	public class Character : Entity, IDamageaeble, IDamageVisitor
	{
		private const float MoveSpeedThreshold = 1e-3f;
		
		[SerializeField] private CharacterMovement movement;
		[SerializeField] private CharacterAudio characterAudio;
		[SerializeField] private CharacterVisual visual;
		[SerializeField] private Bounds localBounds;

		[SyncVar] private GameClass _gameClass;
		[SyncVar] private string _nickName;

		private IStaticDataService _staticData;
		private CharacterProvider _characterProvider;
		private EntityContainerService _entityContainer;
		private IParticleFactory _particleFactory;
		private MapProvider _mapProvider;

		[Inject]
		private void Construct(IStaticDataService staticData, CharacterProvider characterProvider,
			EntityContainerService entityContainer, IParticleFactory particleFactory, MapProvider mapProvider)
		{
			_staticData = staticData;
			_characterProvider = characterProvider;
			_entityContainer = entityContainer;
			_particleFactory = particleFactory;
			_mapProvider = mapProvider;
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
			name = $"{_nickName} [CLASS: {_gameClass}]";

			HealthSystem.Health.Pairwise()
				.Where(pair => pair.Previous > pair.Current)
				.Subscribe(_ => characterAudio.PlayHurtSound())
				.AddTo(this);
		}

		public override void OnStartLocalPlayer()
		{
			visual.TurnOffBodyRender();
			visual.TurnOffNickName();

			_characterProvider.Character.Value = this;
		}

		private void Update()
		{
			if (movement.GetHorizontalVelocity().magnitude > MoveSpeedThreshold && movement.IsGrounded())
			{
				var groundVoxel = movement.GetGroundVoxel();
				
				if (groundVoxel.Position.y == 0 && groundVoxel.Data.Color.Equals(_mapProvider.Map.MapConfigure.WaterColor))
				{
					characterAudio.EnableFootStepInWaterSound();
					characterAudio.DisableStepSound();
				}
				else
				{
					characterAudio.EnableStepSound();	
					characterAudio.DisableFootStepInWaterSound();
				}
			}
			else
			{
				characterAudio.DisableFootStepInWaterSound();
				characterAudio.DisableStepSound();
			}
		}

		public override void OnStopLocalPlayer()
		{
			base.OnStopLocalPlayer();

			_characterProvider.Character.Value = null;
		}

		[Server]
		public void Heal(int heal)
		{
			HealthSystem.Increase(heal);
		}

		[Server]
		public void Damage(int damage)
		{
			HealthSystem.Decrease(damage);
		}

		[Server]
		public void Visit(RangeWeapon rangeWeapon, RaycastHit hit)
		{
			Damage(rangeWeapon.Configure.Damage);
			ParticleSystem blood = _particleFactory.CreateBlood(hit.point, Quaternion.LookRotation(hit.normal));
		}

		[Server]
		public void Visit(MeleeWeapon meleeWeapon, bool isStrongHit, RaycastHit hit)
		{
			Damage(meleeWeapon.Configure.DamageToPlayer);
		}

		[Server]
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

		public override Bounds Bounds => new Bounds(transform.position + localBounds.center, localBounds.size);
	}
}