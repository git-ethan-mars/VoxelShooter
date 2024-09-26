using UnityEngine;

namespace Networking.Messages.Requests
{
	public struct GrenadeSpawnRequest : IMirrorRequest
	{
		public readonly Ray Ray;
		public readonly float ThrowForce;

		public GrenadeSpawnRequest(Ray ray, float throwForce)
		{
			ThrowForce = throwForce;
			Ray = ray;
		}
	}
}