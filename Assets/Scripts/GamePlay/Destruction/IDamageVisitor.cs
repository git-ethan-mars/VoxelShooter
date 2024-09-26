using Common.StaticData;
using UnityEngine;

namespace GamePlay.Destruction
{
	public interface IDamageVisitor
	{
		void Visit(RangeWeaponData rangeWeapon, RaycastHit hit);
		void Visit(MeleeWeaponData meleeWeapon, bool isStrongHit, RaycastHit hit);
		void Visit(Explosion explosion);
	}
}