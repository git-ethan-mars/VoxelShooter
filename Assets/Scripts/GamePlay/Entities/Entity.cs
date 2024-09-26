using System.Collections.Generic;
using System.Linq;
using Entities;
using UnityEngine;

namespace GamePlay.Entities
{
	public abstract class Entity : MonoBehaviour, IEntity
	{
		private static readonly HashSet<Entity> Container = new();

		public static IEnumerable<T> GetEntitiesByType<T>()
		{
			return Container.OfType<T>();
		}
		
		protected virtual void Awake()
		{
			Container.Add(this);
		}

		protected virtual void OnDestroy()
		{
			Container.Remove(this);
		}
	}
}