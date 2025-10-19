using UnityEngine;
namespace GamePlay
{
	public interface ISpawnPointService
	{
		void CreateSpawnPoints();
		Vector3 GetSpawnPoint();
	}
}