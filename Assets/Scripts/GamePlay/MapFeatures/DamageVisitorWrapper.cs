using Data;
using UnityEngine;
namespace GamePlay.MapFeatures
{
	public class DamageVisitorWrapper : MonoBehaviour, IDamageVisitor
	{
		private IDamageVisitor _visitor;

		public void Construct(IDamageVisitor visitor)
		{
			_visitor = visitor;
		}

		public void Visit(RangeWeapon rangeWeapon, RaycastHit hit)
		{
			_visitor.Visit(rangeWeapon, hit);
		}

		public void Visit(MeleeWeapon meleeWeapon, bool isStrongHit, RaycastHit hit)
		{
			_visitor.Visit(meleeWeapon, isStrongHit, hit);
		}

		public void Visit(ExplosionData explosionData, Vector3 center)
		{
			_visitor.Visit(explosionData, center);
		}
	}
}