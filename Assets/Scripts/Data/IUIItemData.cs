using UnityEngine;
namespace Data
{
	public interface IUIItemData
	{
		Sprite Icon { get; }
		string Description { get; }
	}
}