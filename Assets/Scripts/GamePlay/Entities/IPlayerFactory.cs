using Common;
using Entities.PlayerLogic;
using PlayerLogic;
using PlayerLogic.Spectator;
using UnityEngine;

namespace Entities
{
	public interface IPlayerFactory : IService
	{
		Character CreateCharacter(Vector3 position);
		SpectatorPlayer CreateSpectatorPlayer(Vector3 position);
	}
}