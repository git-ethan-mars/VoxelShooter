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

		private readonly HashSet<VotingElement> _subscribedElements = new HashSet<VotingElement>();

		private IMapConfigureLoader _mapConfigureLoader;
		private VotingElement _selectedElement;
		private Voting _voting;

		[Inject]
		private void Construct(IMapConfigureLoader mapConfigureLoader)
		{
			_mapConfigureLoader = mapConfigureLoader;
		}

		public void Initialize(Voting voting)
		{
			_voting = voting;

			_voting.Title.Subscribe(title.SetText).AddTo(this);
			_voting.VoteByCandidate.CollectionChanged += OnVotingChanged;
			UpdateVisibility();
		}

		private void OnDestroy()
		{
			if (_voting != null)
			{
				_voting.VoteByCandidate.CollectionChanged -= OnVotingChanged;
			}
		}

		// Clicking the voted map again takes the vote back.
		private void OnElementClicked(VotingElement element)
		{
			if (_selectedElement != null)
			{
				_selectedElement.Deselect();
			}

			if (_selectedElement == element)
			{
				_voting.CancelVote();
				_selectedElement = null;
				return;
			}

			_voting.SendVote(element.Candidate);
			_selectedElement = element;
			_selectedElement.Select();
		}

		private void OnVotingChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<string, int>> e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddElement(e.NewItem.Key);
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

			UpdateVisibility();
		}

		private void AddElement(string candidate)
		{
			VotingElement element = SpawnElement();
			element.Initialize(candidate, _mapConfigureLoader.GetMapConfigure(candidate).Image);

			// Elements are pooled by the list, so each one is subscribed only once.
			if (_subscribedElements.Add(element))
			{
				element.Clicked.Subscribe(_ => OnElementClicked(element)).AddTo(element);
			}
		}

		private void UpdateVisibility()
		{
			bool isVisible = Items.Count > 0;
			canvasGroup.alpha = isVisible ? 1.0f : 0.0f;
			canvasGroup.blocksRaycasts = isVisible;
			canvasGroup.interactable = isVisible;
		}
	}
}
