using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;
using ObservableCollections;
using R3;

namespace GamePlay
{
	public class Voting
	{
		private readonly VSNetworkManager _networkManager;
		private readonly ObservableDictionary<string, int> _voteByCandidate = new ObservableDictionary<string, int>();
		private readonly Dictionary<NetworkConnectionToClient, string> _voteByConnection = new Dictionary<NetworkConnectionToClient, string>();
		private readonly ReactiveProperty<string> _title = new ReactiveProperty<string>();
		public IReadOnlyObservableDictionary<string, int> VoteByCandidate => _voteByCandidate;
		public Observable<string> Title => _title;

		private Voting(VSNetworkManager networkManager)
		{
			_networkManager = networkManager;
		}

		public static Voting Create(VSNetworkManager networkManager)
		{
			var voting = new Voting(networkManager);

			networkManager.MessageReceived.OfMessageType<VoteResponse>()
				.Subscribe(directedMessage => voting.OnVoteResponse(directedMessage.Message.Title, directedMessage.Message.Variants))
				.AddTo(networkManager);
			networkManager.MessageReceived.OfMessageType<VoteRequest>()
				.Subscribe(directedMessage => voting.AddVote(directedMessage.Connection, directedMessage.Message.Variant))
				.AddTo(networkManager);
			networkManager.MessageReceived.OfMessageType<VoteCancelRequest>()
				.Subscribe(directedMessage => voting.RemoveVote(directedMessage.Connection))
				.AddTo(networkManager);
			networkManager.MessageReceived.OfMessageType<VoteFinishResponse>()
				.Subscribe(_ => voting.FinishVote())
				.AddTo(networkManager);
			return voting;
		}

		public async UniTask<string> RunVotingAsync(TimeSpan duration, string title,
			string[] variants, CancellationToken cancellationToken = default)
		{
			_title.Value = title;

			var response = new VoteResponse(title, variants);
			_networkManager.SendResponseToAll(response);

			await UniTask.Delay(TimeSpan.FromSeconds(duration.TotalSeconds), cancellationToken: cancellationToken);

			string bestCandidate = null;
			int maxQuantity = int.MinValue;

			foreach ((string candidate, int voteQuantity) in _voteByCandidate)
			{
				if (maxQuantity < voteQuantity)
				{
					bestCandidate = candidate;
					maxQuantity = voteQuantity;
				}
			}

			_networkManager.SendResponseToAll(new VoteFinishResponse());

			return bestCandidate;
		}

		public void SendVote(string variant)
		{
			_networkManager.SendRequest(new VoteRequest(variant));
		}

		public void CancelVote()
		{
			_networkManager.SendRequest(new VoteCancelRequest());
		}

		public void OnAddPlayer(NetworkConnectionToClient connection)
		{
			var response = new VoteResponse(_title.CurrentValue, _voteByCandidate.Select(kvp => kvp.Key).ToArray());
			_networkManager.SendResponse(connection, response);
		}

		private void AddVote(NetworkConnectionToClient connection, string candidate)
		{
			if (string.IsNullOrEmpty(candidate) || !_voteByCandidate.ContainsKey(candidate))
			{
				return;
			}

			RemoveVote(connection);

			_voteByConnection[connection] = candidate;
			_voteByCandidate[candidate]++;
		}

		private void RemoveVote(NetworkConnectionToClient connection)
		{
			if (!_voteByConnection.Remove(connection, out string candidate))
			{
				return;
			}

			_voteByCandidate[candidate]--;
		}

		private void OnVoteResponse(string title, string[] candidates)
		{
			_title.Value = title;

			foreach (string candidate in candidates)
			{
				_voteByCandidate[candidate] = 0;
			}
		}

		private void FinishVote()
		{
			_voteByConnection.Clear();
			_voteByCandidate.Clear();
		}
	}
}
