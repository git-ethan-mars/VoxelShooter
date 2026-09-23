using TMPro;
using UnityEngine;

namespace UI
{
	public class VotingElement : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI text;

		private int _votes;

		public string Candidate { get; private set; }

		public void Initialize(string candidate)
		{
			Candidate = candidate;
			_votes = 0;
			text.text = $"{candidate} [{_votes}]";
		}

		public void SetVotes(int votes)
		{
			_votes = votes;
			text.text = $"{Candidate} [{_votes}]";
		}
	}
}
