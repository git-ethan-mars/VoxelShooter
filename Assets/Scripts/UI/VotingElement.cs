using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class VotingElement : MonoBehaviour
	{
		private static readonly Color SelectedColor = new Color(0.72f, 0.95f, 0.62f, 1.0f);

		[SerializeField] private Image frame;
		[SerializeField] private Button previewButton;
		[SerializeField] private Image preview;
		[SerializeField] private TextMeshProUGUI candidateName;
		[SerializeField] private TextMeshProUGUI votes;

		public string Candidate { get; private set; }
		public Observable<Unit> Clicked => previewButton.OnClickAsObservable();

		public void Initialize(string candidate, Sprite previewSprite)
		{
			Candidate = candidate;
			candidateName.SetText(candidate);
			preview.sprite = previewSprite;
			preview.enabled = previewSprite != null;
			SetVotes(0);
			Deselect();
		}

		public void SetVotes(int voteCount)
		{
			votes.SetText(voteCount == 1 ? "1 VOTE" : voteCount + " VOTES");
		}

		public void Select()
		{
			frame.color = SelectedColor;
		}

		public void Deselect()
		{
			frame.color = Color.white;
		}
	}
}
