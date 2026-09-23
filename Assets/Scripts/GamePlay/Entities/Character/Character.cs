using Data;
using Mirror;
using Reflex.Attributes;
using Services;
using UnityEngine;

namespace GamePlay
{
	public class Character : Entity
	{
		[SerializeField] private CharacterMovement movement;
		[SerializeField] private Bounds localBounds;

		[SyncVar] private GameClass _gameClass;
		[SyncVar] private string _nickName;

		private IStaticDataService _staticData;
		private CharacterProvider _characterProvider;
		private EntityContainer _entityContainer;

		[field: SerializeField] public HealthSystem HealthSystem { get; private set; }
		[field: SerializeField] public Inventory Inventory { get; private set; }
		public Characteristics Characteristics { get; private set; }
		public Vector3 ForwardDirection => Vector3.ProjectOnPlane(movement.ForwardDirectionObject.position - transform.position, Vector3.up);
		public string NickName => _nickName;

		public override Bounds Bounds => new Bounds(transform.position + localBounds.center, localBounds.size);

		[Inject]
		private void Construct(IStaticDataService staticData, CharacterProvider characterProvider,
			EntityContainer entityContainer)
		{
			_staticData = staticData;
			_characterProvider = characterProvider;
			EntityContainer = entityContainer;
		}

		public override void OnStartClient()
		{
			base.OnStartClient();

			Characteristics = _staticData.GetCharacteristics(_gameClass);
			name = $"{Id} [CLASS: {_gameClass}]";
		}

		public override void OnStartLocalPlayer()
		{
			_characterProvider.Character.Value = this;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		}

		public override void OnStopLocalPlayer()
		{
			_characterProvider.Character.Value = null;
		}

		[Server]
		public void Initialize(GameClass gameClass, string nickName)
		{
			_gameClass = gameClass;
			_nickName = nickName;
		}
	}
}
