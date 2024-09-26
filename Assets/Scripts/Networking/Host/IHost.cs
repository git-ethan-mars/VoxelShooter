using System;
using System.Threading;
using Common.StaticData;
using Entities;
using Entities.PlayerLogic;
using Mirror;
using Networking.Client;
using Networking.Host.Services;
using UnityEngine;

namespace Networking.Host
{
	public interface IHost
	{
		CancellationToken OnHostStopped { get; }
		IClient InnerClient { get; }
		event Action<Character> PlayerDied;
		MapUpdater MapUpdater { get; }
		bool AddPlayer(NetworkConnectionToClient connection, ulong id, string nickName);
		void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage);
		void Heal(NetworkConnectionToClient receiver, int totalHeal);
		bool TryGetPlayerData(NetworkConnectionToClient connection, out Player player);
		Player GetPlayerData(NetworkConnectionToClient connection);
		void SpawnEntity(Entity entity, NetworkConnectionToClient owner = null);
		void UnSpawnEntity(Entity entity);
		void Start();
		void Stop();
		void SpawnParticles(ParticleSystem particles);
		void SendAudio(AudioData audio, NetworkIdentity source);
		void StartContinuousAudio(AudioData audio, NetworkIdentity source);
		void StopContinuousSound(NetworkIdentity source);
		void StartMuzzleFlash(NetworkIdentity source);
		void StopMuzzleFlash(NetworkIdentity source);
		void SendMap(NetworkConnectionToClient connection);
	}
}