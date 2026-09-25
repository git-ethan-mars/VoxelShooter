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
		private VotingElement _selectedElement;
		private Voting _voting;

		[Inject]
		private void Construct(IInputService inputService)
		{
			_inputService = inputService;
		}

		public void Initialize(Voting voting)
		{
			_voting = voting;

			_voting.Title.Subscribe(title.SetText).AddTo(this);
			_voting.VoteByCandidate.CollectionChanged += OnVotingChanged;
		}

		private void OnEnable()
		{
			canvasGroup.alpha = 1;
		}

		private void Update()
		{
			for (int i = 0; i < Items.Count; i++)
			{
				if (!_inputService.IsSlotButtonPressed(i))
				{
					continue;
				}

				if (!_selectedElement)
				{
					SelectElement(Items[i]);
				}
				else
				{
					_selectedElement.Deselect();

					if (_selectedElement != Items[i])
					{
						SelectElement(Items[i]);
					}
					else
					{
						_voting.CancelVote();
						_selectedElement = null;
					}
				}
			}
		}

		private void SelectElement(VotingElement element)
		{
			_voting.SendVote(element.Candidate);
			_selectedElement = element;
			_selectedElement.Select();
		}

		private void OnDisable()
		{
			canvasGroup.alpha = 0;
		}

		private void OnDestroy()
		{
			if (_voting != null)
			{
				_voting.VoteByCandidate.CollectionChanged -= OnVotingChanged;
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
					_selectedElement = null;
					Clear();
					break;
			}
		}
	}
}
