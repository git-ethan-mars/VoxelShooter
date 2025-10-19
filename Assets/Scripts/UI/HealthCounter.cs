using TMPro;
using UnityEngine;
namespace UI
{
	public class HealthCounter : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI healthText;

		public void SetHealthValue(string value)
		{
			healthText.SetText(value);
		}
	}
}