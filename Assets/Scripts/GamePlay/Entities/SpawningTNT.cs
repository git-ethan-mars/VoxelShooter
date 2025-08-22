using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Reflex.Attributes;
using UnityEngine;
namespace GamePlay
{
	public class SpawningTNT : NetworkBehaviour
	{
		private TNTData _data;

		[Inject]
		private void Construct(IInventoryItemDataLoader dataLoader)
		{
			_data = dataLoader.LoadItemData<TNTData>(ItemType.TNT);
		}

		public async UniTask ExplodeAsync()
		{
			await UniTask.WaitForSeconds(_data.DelayInSeconds, cancellationToken:destroyCancellationToken);
			Debug.Log("TNT EXPLODED");
		}
	}
}