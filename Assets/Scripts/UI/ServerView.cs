using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class ServerView : MonoBehaviour
	{
		public Observable<Unit> JoinButtonPressed => joinButton.onClick.AsObservable();
		
		[field: SerializeField] public TextMeshProUGUI ServerName { get; private set; }
		[field: SerializeField] public TextMeshProUGUI MapName { get; private set; }
		[field: SerializeField] private Button joinButton;
	}
}