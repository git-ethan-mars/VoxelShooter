using Data;
using UnityEngine;
namespace GamePlay.MapFeatures
{
	public interface IDamageVisitor
	{
		void Visit(RangeWeapon rangeWeapon, RaycastHit hit);
		void Visit(MeleeWeapon meleeWeapon, bool isStrongHit, RaycastHit hit);
		void Visit(ExplosionData explosionData, Vector3 center);
	}
}