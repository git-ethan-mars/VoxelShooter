using GamePlay.Data;
using GamePlay.Factory;
using GamePlay.MapFeatures;
using UnityEngine;

namespace GamePlay.Entities
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

		public void Visit(Vector3 center, ExplosionData explosionData)
		{
			var damage = CalculateLinearDamage(center, explosionData.radius, explosionData.damage);
			character.Damage(damage);
		}

		private int CalculateLinearDamage(Vector3 explosionCenter, int radius, int damage)
		{
			return (int) ((1 - Vector3.Distance(character.transform.position, explosionCenter) / radius) * damage);
		}
	}
}