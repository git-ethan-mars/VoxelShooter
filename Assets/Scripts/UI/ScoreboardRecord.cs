using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class ScoreboardRecord : MonoBehaviour
	{
		[field: SerializeField] public RawImage Avatar { get; private set; }
		[field: SerializeField] public TextMeshProUGUI NickName { get; private set; }
		[field: SerializeField] public TextMeshProUGUI ClassName { get; private set; }
		[field: SerializeField] public TextMeshProUGUI KillCount { get; private set; }
		[field: SerializeField] public TextMeshProUGUI DeathCount { get; private set; }
	}
}