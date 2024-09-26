using Entities;
using GamePlay.Entities;
using UnityEngine;

namespace GamePlay.Destruction
{
	public class Explosion : MonoBehaviour
	{
		[SerializeField]
		private int radius;

		[SerializeField]
		private int damage;

		public int Radius => radius;

		public int Damage => damage;
		
		public void Explode()
		{
			foreach (var visitor in Entity.GetEntitiesByType<IDamageVisitor>())
			{
				visitor.Visit(this);
			}
			
			_mapDamageVisitor.Visit(this);
		}
	}
}