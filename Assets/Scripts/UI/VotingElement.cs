using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class VotingElement : MonoBehaviour
	{
		private static readonly Color SelectedColor = new Color(0.72f, 0.95f, 0.62f, 1.0f);

		[SerializeField] private Image background;
		[SerializeField] private TextMeshProUGUI number;
		[SerializeField] private TextMeshProUGUI candidateName;
		[SerializeField] private TextMeshProUGUI votes;

		public string Candidate { get; private set; }

		public void Initialize(int index, string candidate)
		{
			Candidate = candidate;
			number.SetText((index + 1).ToString());
			candidateName.SetText(candidate);
			SetVotes(0);
			Deselect();
		}

		public void SetVotes(int voteCount)
		{
			votes.SetText(voteCount.ToString());
		}

		public void Select()
		{
			background.color = SelectedColor;
		}

		public void Deselect()
		{
			background.color = Color.white;
		}
	}
}
