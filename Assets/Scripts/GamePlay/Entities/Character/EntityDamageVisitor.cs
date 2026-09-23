using System;
using Data;
using Data.Mappers;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace GamePlay
{
	public class EntityDamageVisitor : NetworkBehaviour, IDamageVisitor
	{
		[SerializeField] private HealthSystem healthSystem;

		private IParticleFactory _particleFactory;
		private KillList _killList;

		[Inject]
		private void Construct(IParticleFactory particleFactory, KillList killList)
		{
			_particleFactory = particleFactory;
			_killList = killList;
		}

		[Server]
		public void Visit(RangeWeapon rangeWeapon, RaycastHit hit)
		{
			healthSystem.Decrease(rangeWeapon.Configure.Damage);

			if (healthSystem.Health.CurrentValue == 0)
			{
				if (!rangeWeapon.OwnerId.HasValue)
				{
					throw new ArgumentNullException(nameof(rangeWeapon.OwnerId));
				}

				var killReason = KillReasonsMapper.KillReasonByItemType(rangeWeapon.Type);
				var targetId = new PlayerId(connectionToClient.connectionId);
				var killData = new KillData(rangeWeapon.OwnerId.Value, targetId, killReason);
				_killList.AddKill(killData);
			}

			_particleFactory.CreateBlood(hit.point, Quaternion.LookRotation(hit.normal));
		}

		[Server]
		public void Visit(MeleeWeapon meleeWeapon, RaycastHit hit)
		{
			healthSystem.Decrease(meleeWeapon.Configure.DamageToPlayer);

			if (healthSystem.Health.CurrentValue == 0)
			{
				if (!meleeWeapon.OwnerId.HasValue)
				{
					throw new ArgumentNullException(nameof(meleeWeapon.OwnerId));
				}

				var killReason = KillReasonsMapper.KillReasonByItemType(meleeWeapon.Type);
				var targetId = new PlayerId(connectionToClient.connectionId);
				var killData = new KillData(meleeWeapon.OwnerId.Value, targetId, killReason);
				_killList.AddKill(killData);
			}
		}

		[Server]
		public void Visit(Explosive explosive, ExplosionData explosionData)
		{
			if (Vector3.Distance(explosive.transform.position, transform.position) >= explosionData.radius)
			{
				return;
			}

			var damage = (int)((1 - Vector3.Distance(transform.position, explosive.transform.position) / explosionData.radius)
			                   * explosionData.damage);
			healthSystem.Decrease(damage);

			if (healthSystem.Health.CurrentValue == 0)
			{
				if (!explosive.OwnerId.HasValue)
				{
					throw new ArgumentNullException(nameof(explosive.OwnerId));
				}

				var killReason = KillReasonsMapper.KillReasonByExplosiveType(explosive.Type);
				var targetId = new PlayerId(connectionToClient.connectionId);
				var killData = new KillData(explosive.OwnerId.Value, targetId, killReason);
				_killList.AddKill(killData);
			}
		}

		public void Visit(FallingDamage fallingDamage, int damage)
		{
			healthSystem.Decrease(damage);

			if (healthSystem.Health.CurrentValue == 0)
			{
				var sourceId = new PlayerId(connectionToClient.connectionId);
				var killData = new KillData(sourceId, sourceId, KillReason.FallingDamage);
				_killList.AddKill(killData);
			}
		}
	}
}
