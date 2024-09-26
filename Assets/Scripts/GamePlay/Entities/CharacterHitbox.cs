using Common.Factory;
using Common.StaticData;
using Entities.PlayerLogic;
using GamePlay.Destruction;
using UnityEngine;

namespace Entities
{
	[RequireComponent(typeof(Character))]
	public class CharacterHitBox : MonoBehaviour, IDamageVisitor
	{
		[SerializeField]
		private Character character;

		private IParticleFactory _particleFactory;

		public void Construct(IParticleFactory particleFactory)
		{
			_particleFactory = particleFactory;
		}

		public void Visit(RangeWeaponData rangeWeapon, RaycastHit hit)
		{
			character.Damage(rangeWeapon.Damage);
			var blood = _particleFactory.CreateBlood(hit.point, Quaternion.LookRotation(hit.normal));
		}

		public void Visit(MeleeWeaponData meleeWeapon, bool isStrongHit, RaycastHit hit)
		{
			character.Damage(meleeWeapon.DamageToPlayer);
		}

		public void Visit(Explosion explosion)
		{
			var damage = CalculateLinearDamage(explosion.transform.position, explosion.Radius, explosion.Damage);
			character.Damage(damage);
		}

		private int CalculateLinearDamage(Vector3 explosionCenter, int radius, int damage)
		{
			return (int) ((1 - Vector3.Distance(character.transform.position, explosionCenter) / radius) * damage);
		}
	}
}