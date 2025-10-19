using Data;
using UnityEngine;
namespace GamePlay
{
	public interface IEntityFactory
	{
		SpawningTNT CreateSpawningTnt(Vector3 position, Quaternion rotation);
		Tombstone CreateTombstone(Vector3 position);
		Rocket CreateRocket(Vector3 position, Quaternion rotation);
		Drill CreateDrill(Vector3 position, Quaternion rotation);
		LootBox CreateLootBox(LootBoxType type, Vector3 boxPosition);
		SpawnPoint CreateSpawnPoint(SpawnPointData position, Transform parent);
		SpawningGrenade CreateSpawningGrenade(Vector3 position);
		Spectator CreateSpectator(Vector3 position);
		Character CreateCharacter(Vector3 position, GameClass chosenClass);
	}
}