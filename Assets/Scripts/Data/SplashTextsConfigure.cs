using UnityEngine;

namespace Data
{
	[CreateAssetMenu(fileName = "Splash Texts", menuName = "UI/Splash Texts")]
	public class SplashTextsConfigure : ScriptableObject
	{
		[field: SerializeField] public string[] Texts { get; private set; }
	}
}
