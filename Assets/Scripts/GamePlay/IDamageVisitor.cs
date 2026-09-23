using Data;
using UnityEngine;

namespace GamePlay
{
	public interface IDamageVisitor
	{
		void Visit(RangeWeapon rangeWeapon, RaycastHit hit);
		void Visit(MeleeWeapon meleeWeapon, RaycastHit hit);
		void Visit(Explosive explosive, ExplosionData explosionData);
		void Visit(FallingDamage fallingDamage, int damage);
	}
}
