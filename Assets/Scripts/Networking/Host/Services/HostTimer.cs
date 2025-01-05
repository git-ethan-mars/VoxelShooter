using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay.Data;
using Mirror;
using Networking.Messages.Responses;

namespace Networking.Host.Services
{
    public class HostTimer
    {
        private readonly IHost _host;
        private readonly CancellationToken _token;
        private readonly int _timeInSeconds;
        private ServerTime _timeLeft;

        public HostTimer(IHost host, int timeInMinutes, CancellationToken token)
        {
            _host = host;
            _token = token;
            _timeInSeconds = timeInMinutes * 60;
        }

        public async void Start()
        {
            await SendTimeAsync();
        }

        private async UniTask SendTimeAsync()
        {
            _timeLeft = new ServerTime(_timeInSeconds);
            for (var i = 0; i < _timeInSeconds; i++)
            {
                NetworkServer.SendToReady(new GameTimeResponse(_timeLeft));
                _timeLeft = _timeLeft.Subtract(new ServerTime(1));
                if (await UniTask.WaitForSeconds(1, cancellationToken: _token).SuppressCancellationThrow())
                {
                    return;
                }
            }

            _host.Stop();
        }
    }
}
