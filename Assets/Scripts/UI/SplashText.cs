using Data;
using TMPro;
using UnityEngine;

namespace UI
{
	// Minecraft-like pulsing splash with a random phrase.
	public class SplashText : MonoBehaviour
	{
		[SerializeField] private TMP_Text text;
		[SerializeField] private SplashTextsConfigure splashTexts;
		[SerializeField] private float pulseSpeed = 8.0f;
		[SerializeField] private float pulseAmplitude = 0.08f;

		private void Start()
		{
			string[] texts = splashTexts.Texts;
			text.text = texts.Length > 0 ? texts[Random.Range(0, texts.Length)] : string.Empty;
		}

		private void Update()
		{
			float scale = 1.0f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * pulseSpeed)) * pulseAmplitude;
			transform.localScale = new Vector3(scale, scale, 1.0f);
		}
	}
}
