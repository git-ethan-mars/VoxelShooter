using TMPro;
using UnityEngine;
namespace GamePlay
{
	public class CharacterVisual : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI nickNameText;
		[SerializeField] private SkinnedMeshRenderer skin;
		[SerializeField] private Canvas nickNameCanvas;

		public void TurnOffBodyRender()
		{
			skin.enabled = false;
		}

		public void TurnOffNickName()
		{
			nickNameCanvas.gameObject.SetActive(false);
		}

		public void SetNickName(string nickName)
		{
			nickNameText.SetText(nickName);
		}
	}
}