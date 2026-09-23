using Mirror;
using UnityEngine;
using EntityId = Data.EntityId;

namespace GamePlay
{
	public abstract class Entity : NetworkBehaviour
	{
		public abstract Bounds Bounds { get; }
		public EntityId Id => new EntityId(netIdentity.netId);
		
		protected EntityContainer EntityContainer;

		public override void OnStartClient()
		{
			base.OnStartClient();
			EntityContainer.Add(Id, this);
		}

		public override void OnStopClient()
		{
			EntityContainer.Remove(Id);
		}
	}
}