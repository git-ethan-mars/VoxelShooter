using Data;
using Mirror;
using UnityEngine;
using VoxelMap.Data;
namespace GamePlay
{
	public interface IEntityFactory
	{
		SpawningTNT CreateSpawningTnt(Vector3 position, Quaternion rotation, NetworkConnectionToClient connection);
		Tombstone CreateTombstone(Vector3 position, NetworkConnectionToClient connection);
		Rocket CreateRocket(Vector3 position, Quaternion rotation, NetworkConnectionToClient connection);
		Drill CreateDrill(Vector3 position, Quaternion rotation, NetworkConnectionToClient connection);
		SpawningGrenade CreateSpawningGrenade(Vector3 position, NetworkConnectionToClient connection);
		LootBox CreateLootBox(LootBoxType type, Vector3 boxPosition);
		SpawnPoint CreateSpawnPoint(SpawnPointData position, Transform parent);
		Spectator CreateSpectator(Vector3 position);
		Character CreateCharacter(Vector3 position, GameClass chosenClass, string nickname);
	}
}