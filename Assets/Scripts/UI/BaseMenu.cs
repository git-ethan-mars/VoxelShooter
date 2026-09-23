using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class BaseMenu : MonoBehaviour
	{
		private CanvasGroup _canvasGroup;

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

		public abstract void Show();
		public abstract void Hide();
	}
}
