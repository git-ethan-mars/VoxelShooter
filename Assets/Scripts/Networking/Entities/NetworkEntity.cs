using System.Collections.Generic;
using System.Linq;
using GamePlay.Entities;
using Mirror;

namespace Networking.Entities
{
	public abstract class NetworkEntity : NetworkBehaviour, IEntity
	{
		private static readonly HashSet<NetworkEntity> Container = new();

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