using GamePlay.Data;
using UnityEngine;

namespace GamePlay.MapFeatures
{
	public interface IDamageVisitor
	{
		void Visit(RangeWeaponData rangeWeapon, RaycastHit hit);
		void Visit(MeleeWeaponData meleeWeapon, bool isStrongHit, RaycastHit hit);
		void Visit(Vector3 center, ExplosionData explosionData);
	}
}