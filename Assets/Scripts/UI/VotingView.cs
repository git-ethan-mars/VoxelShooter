using System.Collections.Generic;
using System.Collections.Specialized;
using GamePlay;
using ObservableCollections;
using R3;
using Reflex.Attributes;
using Services;
using TMPro;
using UnityEngine;

namespace UI
{
	public class VotingView : ListView<VotingElement>
	{
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private TextMeshProUGUI title;

		private IInputService _inputService;

		private Voting _voting;

		[Inject]
		private void Construct(IInputService inputService)
		{
			_inputService = inputService;
		}

		public void Initialize(Voting voting)
		{
			_voting = voting;

			_voting.IsActivated.Subscribe(value => enabled = value);

			_voting.Title.Subscribe(title.SetText).AddTo(this);
			_voting.VoteByCandidate.CollectionChanged += OnVotingChanged;
		}

		private void OnEnable()
		{
			canvasGroup.alpha = 1;
		}

		private void OnDisable()
		{
			canvasGroup.alpha = 0;
		}

		private void Update()
		{
			var i = 0;

			foreach ((string candidate, int _) in _voting.VoteByCandidate)
			{
				if (_inputService.IsSlotButtonPressed(i))
				{
					_voting.SendVote(candidate);
				}

				i++;
			}
		}

		private void OnVotingChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<string, int>> e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					VotingElement element = SpawnElement();
					element.Initialize(e.NewItem.Key);
					break;
				case NotifyCollectionChangedAction.Replace:
					string candidateName = e.NewItem.Key;
					Items.Find(votingElement => votingElement.Candidate == candidateName).SetVotes(e.NewItem.Value);
					break;
				case NotifyCollectionChangedAction.Reset:
					Clear();
					break;
			}
		}

		private void OnDestroy()
		{
			_voting.VoteByCandidate.CollectionChanged -= OnVotingChanged;
		}
	}
}
