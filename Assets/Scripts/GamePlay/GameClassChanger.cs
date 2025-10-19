using System;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay.Core;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class GameClassChanger
	{
		private readonly IPlayerService _playerService;
		private readonly ISpawnPointService _spawnPointService;
		private readonly IEntityFactory _entityFactory;
		private readonly IStaticDataService _staticData;
		private readonly IItemFactory _itemFactory;
		private readonly VoxelShooterNetworkManager _networkManager;

		private WorldSettings _worldSettings;

		public GameClassChanger(IPlayerService playerService, ISpawnPointService spawnPointService, IEntityFactory entityFactory, 
			IStaticDataService staticData, IItemFactory itemFactory, VoxelShooterNetworkManager networkManager)
		{
			_playerService = playerService;
			_spawnPointService = spawnPointService;
			_entityFactory = entityFactory;
			_staticData = staticData;
			_itemFactory = itemFactory;
			_networkManager = networkManager;
		}

		public void Initialize(WorldSettings worldSettings)
		{
			_worldSettings = worldSettings;
			_networkManager.MessageReceived
				.OfMessageType<ChangeClassRequest>()
				.Subscribe(directedMessage => ChangeClass(directedMessage.Connection, directedMessage.Message.GameClass))
				.AddTo(_networkManager);
		}

		private void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
		{
			if (!_playerService.TryGetPlayerData(connection.connectionId, out PlayerData playerData))
			{
				throw new ArgumentOutOfRangeException(nameof(connection.connectionId));
			}

			if (playerData.GameClass == chosenClass)
			{
				return;
			}

			if (playerData.GameClass == GameClass.None)
			{
				playerData.GameClass = chosenClass;
				Character character = SpawnCharacter(playerData);
				NetworkServer.AddPlayerForConnection(connection, character.gameObject);
				character.Inventory.Initialize(_itemFactory.CreateItems(chosenClass));
				playerData.IsAlive = true;
			}
			else if (playerData.IsAlive)
			{
				connection.identity.GetComponent<Character>().HealthSystem.Decrease(int.MaxValue);
				playerData.GameClass = chosenClass;
			}
		}

		private Character SpawnCharacter(PlayerData playerData)
		{
			GameClass chosenClass = playerData.GameClass;
			string nickName = playerData.NickName;
			Vector3 position = _spawnPointService.GetSpawnPoint();
			Character character = _entityFactory.CreateCharacter(position, chosenClass);
			
			Characteristics characteristics = _staticData.GetCharacteristics(chosenClass);
			character.Initialize(chosenClass, nickName);
			character.HealthSystem.Initialize(characteristics.MaxHealth);
			
			character.HealthSystem.Health
				.Where(healthValue => healthValue == 0)
				.Subscribe(_ => OnCharacterDied(character))
				.AddTo(character);
			
			return character;
		}

		private async void OnCharacterDied(Character oldCharacter)
		{
			NetworkConnectionToClient connection = oldCharacter.netIdentity.connectionToClient;

			if (!_playerService.TryGetPlayerData(connection.connectionId, out PlayerData playerData))
			{
				return;
			}

			playerData.IsAlive = false;
			Spectator spectator = _entityFactory.CreateSpectator(oldCharacter.transform.position);
			NetworkServer.ReplacePlayerForConnection(connection, spectator.gameObject, ReplacePlayerOptions.Destroy);
			Tombstone tombStone = _entityFactory.CreateTombstone(oldCharacter.transform.position);
			await tombStone.ExplodeWithDelay(_worldSettings.SpawnTime - TimeSpan.FromSeconds(1)).SuppressCancellationThrow();
			await UniTask.Delay(TimeSpan.FromSeconds(1));

			if (connection.isReady)
			{
				Character character = SpawnCharacter(playerData);
				NetworkServer.ReplacePlayerForConnection(connection, character.gameObject, ReplacePlayerOptions.Destroy);
				character.Inventory.Initialize(_itemFactory.CreateItems(playerData.GameClass));
				playerData.IsAlive = true;
			}
		}
	}
}