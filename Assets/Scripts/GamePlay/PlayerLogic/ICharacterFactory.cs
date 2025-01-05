using Common;
using UnityEngine;

namespace GamePlay
{
	public interface ICharacterFactory : IService
	{
		Character CreateCharacter(Vector3 position);
		Spectator CreateSpectator(Vector3 position);
	}
}