using System.Collections.Generic;
using System.Threading;
using Common;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using Entities;
using Entities.PlayerLogic;
using UnityEngine;

namespace Networking.Host.Services
{
	public class FallDamage
	{
		private CancellationTokenSource _cts;
		
		private readonly IHost _host;
		private readonly Dictionary<Character, float> _lastSpeedByCharacter = new();
		private readonly FallDamageData _fallDamageData;

		public FallDamage(IHost host, IStaticDataService staticData)
		{
			_host = host;
			_fallDamageData = staticData.GetFallDamageConfiguration();
		}

		public void Start()
		{
			_host.PlayerDied += OnPlayerDied;
			_cts = new CancellationTokenSource();
			CheckFallDamageAsync(_cts.Token).Forget();
		}

		public void Stop()
		{
			_host.PlayerDied -= OnPlayerDied;
			_cts.Cancel();
			_cts.Dispose();
		}

		private async UniTaskVoid CheckFallDamageAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				foreach (var player in Entity.GetEntitiesByType<Character>())
				{
					if (!_lastSpeedByCharacter.ContainsKey(player))
					{
						_lastSpeedByCharacter[player] = 0.0f;
					}
					else
					{
						if (_lastSpeedByCharacter[player] < -_fallDamageData.minSpeedToDamage
						    && Mathf.Abs(player.Rigidbody.velocity.y) < Constants.Epsilon)
						{
							_host.Damage(player.connectionToClient, player.connectionToClient,
								(int) (-(_lastSpeedByCharacter[player] + _fallDamageData.minSpeedToDamage) *
								       _fallDamageData.damagePerMetersPerSecond));
						}

						_lastSpeedByCharacter[player] = player.Rigidbody.velocity.y;
					}
				}

				await UniTask.WaitForFixedUpdate(token).SuppressCancellationThrow();
			}
		}

		private void OnPlayerDied(Character character)
		{
			_lastSpeedByCharacter.Remove(character);
		}
	}
}