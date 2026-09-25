using System;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using R3;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace GamePlay
{
	// A training target: takes damage like a player, shows it and restores its health after dying.
	public class Dummy : Entity, IDamageVisitor
	{
		private const float RestoreDelay = 2.0f;
		private const float DamageShowTime = 1.2f;
		private const float DamageRiseHeight = 0.6f;

		[SerializeField] private Bounds localBounds;
		[SerializeField] private HealthSystem healthSystem;
		[SerializeField] private Canvas canvas;
		[SerializeField] private TextMeshProUGUI healthText;
		[SerializeField] private TextMeshProUGUI damageText;

		private IParticleFactory _particleFactory;
		private CameraProvider _cameraProvider;

		[SyncVar] private int _maxHealth;

		private Vector3 _damageTextPosition;
		private float _damageShownTime = float.NegativeInfinity;

		public override Bounds Bounds => new Bounds(transform.position + localBounds.center, localBounds.size);

		[Inject]
		private void Construct(EntityContainer entityContainer, IParticleFactory particleFactory, CameraProvider cameraProvider)
		{
			EntityContainer = entityContainer;
			_particleFactory = particleFactory;
			_cameraProvider = cameraProvider;
		}

		[Server]
		public void Initialize(int maxHealth)
		{
			_maxHealth = maxHealth;
			healthSystem.Initialize(maxHealth);
		}

		public override void OnStartClient()
		{
			base.OnStartClient();

			_damageTextPosition = damageText.rectTransform.anchoredPosition;
			damageText.SetText(string.Empty);
			healthSystem.Health.Subscribe(ShowHealth).AddTo(this);
		}

		private void LateUpdate()
		{
			if (isClient && _cameraProvider.MainCamera != null)
			{
				canvas.transform.rotation = _cameraProvider.MainCamera.transform.rotation;
			}

			float progress = (Time.time - _damageShownTime) / DamageShowTime;

			if (progress <= 1.0f)
			{
				damageText.alpha = 1.0f - progress * progress;
				damageText.rectTransform.anchoredPosition = _damageTextPosition +
					Vector3.up * (progress * DamageRiseHeight / canvas.transform.lossyScale.y);
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		}

		[Server]
		public void Visit(RangeWeapon rangeWeapon, RaycastHit hit)
		{
			if (TakeDamage(rangeWeapon.Configure.Damage))
			{
				_particleFactory.CreateBlood(hit.point, Quaternion.LookRotation(hit.normal));
			}
		}

		[Server]
		public void Visit(MeleeWeapon meleeWeapon, RaycastHit hit)
		{
			TakeDamage(meleeWeapon.Configure.DamageToPlayer);
		}

		[Server]
		public void Visit(Explosive explosive, ExplosionData explosionData)
		{
			TakeDamage(explosive.GetDamageAt(transform.position, explosionData));
		}

		public void Visit(FallingDamage fallingDamage, int damage)
		{
		}

		private bool TakeDamage(int damage)
		{
			if (damage <= 0 || healthSystem.Health.CurrentValue == 0)
			{
				return false;
			}

			healthSystem.Decrease(damage);
			RpcShowDamage(damage);

			if (healthSystem.Health.CurrentValue == 0)
			{
				RestoreHealthAsync().Forget();
			}

			return true;
		}

		private async UniTaskVoid RestoreHealthAsync()
		{
			bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(RestoreDelay), cancellationToken: destroyCancellationToken)
				.SuppressCancellationThrow();

			if (!isCancelled)
			{
				healthSystem.Initialize(_maxHealth);
			}
		}

		[ClientRpc]
		private void RpcShowDamage(int damage)
		{
			damageText.SetText($"-{damage}");
			_damageShownTime = Time.time;
		}

		private void ShowHealth(int health)
		{
			healthText.SetText(health > 0 ? $"DUMMY\n{health}/{_maxHealth}" : "DUMMY\nDEAD");
		}
	}
}
