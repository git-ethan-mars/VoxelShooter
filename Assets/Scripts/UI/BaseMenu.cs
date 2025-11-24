using UnityEngine;
namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class BaseMenu : MonoBehaviour
	{
		public CanvasGroup CanvasGroup
		{
			get
			{
				if (_canvasGroup == null)
				{
					_canvasGroup = GetComponent<CanvasGroup>();
				}
				
				return _canvasGroup;
			}
		}
		private CanvasGroup _canvasGroup;
		public abstract void Show();
		public abstract void Hide();
	}
}