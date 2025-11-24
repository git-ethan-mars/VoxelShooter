using Cysharp.Threading.Tasks;
using UnityEngine;
namespace GamePlay
{
	public interface ISpawnPointService
	{
		void CreateSpawnPoints();
		UniTask<Vector3> GetSpawnPointAsync();
	}
}