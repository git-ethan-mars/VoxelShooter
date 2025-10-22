using Mirror;
using UnityEngine;
namespace GamePlay
{
	public abstract class Entity : NetworkBehaviour
	{
		public abstract Bounds Bounds { get; }
	}
}