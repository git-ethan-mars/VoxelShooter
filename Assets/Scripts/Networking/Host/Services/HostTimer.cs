using System.Collections;
using System.Threading;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using Infrastructure;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.Host.Services
{
	public class HostTimer
	{
		private readonly IHost _host;
		private readonly int _timeInSeconds;
		private ServerTime _timeLeft;

		public HostTimer(IHost host, int timeInMinutes)
		{
			_host = host;
			_timeInSeconds = timeInMinutes * 60;
		}

		public void Start()
		{
			SendTimeAsync(_host.OnHostStopped).Forget();
		}

		private async UniTaskVoid SendTimeAsync(CancellationToken token)
		{
			_timeLeft = new ServerTime(_timeInSeconds);
			for (var i = 0; i < _timeInSeconds; i++)
			{
				NetworkServer.SendToReady(new GameTimeResponse(_timeLeft));
				_timeLeft = _timeLeft.Subtract(new ServerTime(1));
				await UniTask.WaitForSeconds(1, cancellationToken: token).SuppressCancellationThrow();
			}

			_host.Stop();
		}
	}
}