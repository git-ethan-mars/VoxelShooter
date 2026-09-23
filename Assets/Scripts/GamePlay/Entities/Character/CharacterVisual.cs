using Mirror;
using TMPro;
using UnityEngine;
namespace GamePlay
{
	public class CharacterVisual : NetworkBehaviour
	{
		[SerializeField] private Character character;
		[SerializeField] private TextMeshProUGUI nickNameText;
		[SerializeField] private SkinnedMeshRenderer skin;
		[SerializeField] private Canvas nickNameCanvas;

		public override void OnStartLocalPlayer()
		{
			skin.enabled = false;
			nickNameCanvas.gameObject.SetActive(false);
			nickNameText.SetText(character.NickName);
		}
	}
}