using UnityEngine;
namespace UI
{
	public interface IBaseMenu
	{
		CanvasGroup CanvasGroup { get; }

		void Show();
		void Hide();
		Transform transform { get; }
	}
}