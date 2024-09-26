using System;
using Common;
using Common.AssetManagement;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using Entities;
using Inventory;
using UnityEngine;

namespace GamePlay.PlayerLogic.Inventory.Tnt
{
	public class Tnt : MonoBehaviour, IInventoryItem
	{
		[SerializeField]
		private GameObject transparentTntPrefab;

		private IEntityFactory _entityFactory;
		private RayCaster _rayCaster;
		private GameObject _transparentTnt;
		public TntData Data { get; private set; }

		public void Construct(IEntityFactory entityFactory, IAssetProvider assets, TntData data, RayCaster rayCaster)
		{
			_entityFactory = entityFactory;
			_transparentTnt = assets.Instantiate(transparentTntPrefab, transform);
			_rayCaster = rayCaster;
			Data = data;
		}

		public void CreateTnt()
		{
			if (Data.Amount <= 0)
			{
				return;
			}
			
			var raycastResult = _rayCaster.GetBuildRayCastHit(out var raycastHit);
			if (!raycastResult)
			{
				var tntPosition = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
				                  GetTntOffsetPosition(raycastHit.normal);
				var tntRotation = GetTntRotation(raycastHit.normal);
				var linkedPosition = Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2);
				var tnt = _entityFactory.CreateTnt(tntPosition, tntRotation, Data);
				tnt.ExplodeAsync().Forget();
			}

			Data.Amount -= 1;
		}

		private void Update()
		{
			var rayCastResult = _rayCaster.GetBuildRayCastHit(out var rayCastHit);
			if (rayCastResult)
			{
				_transparentTnt.SetActive(true);
				transparentTntPrefab.transform.position = Vector3Int.FloorToInt(rayCastHit.point + rayCastHit.normal / 2) +
				                                          GetTntOffsetPosition(rayCastHit.normal);
				transparentTntPrefab.transform.rotation = GetTntRotation(rayCastHit.normal);
			}
			else
			{
				transparentTntPrefab.SetActive(false);
			}
		}

		public void Enable()
		{
			_transparentTnt.SetActive(true);
		}

		public void Disable()
		{
			_transparentTnt.SetActive(false);
		}

		private static Vector3 GetTntOffsetPosition(Vector3 normal)
		{
			if (normal == Vector3.up)
			{
				return new Vector3(0.45f, 0, 0.43f);
			}

			if (normal == Vector3.down)
			{
				return new Vector3(0.45f, 1, 0.57f);
			}

			if (normal == Vector3.right)
			{
				return new Vector3(0, 0.57f, 0.57f);
			}

			if (normal == Vector3.left)
			{
				return new Vector3(1, 0.57f, 0.43f);
			}

			if (normal == Vector3.forward)
			{
				return new Vector3(0.43f, 0.57f, 0);
			}

			if (normal == Vector3.back)
			{
				return new Vector3(0.57f, 0.57f, 1);
			}

			throw new ArgumentException("Can't attach tnt to wrong face of block");
		}

		private static Quaternion GetTntRotation(Vector3 normal)
		{
			if (normal == Vector3.up)
			{
				return Quaternion.Euler(0, 0, 0);
			}

			if (normal == Vector3.down)
			{
				return Quaternion.Euler(180, 0, 0);
			}

			if (normal == Vector3.right)
			{
				return Quaternion.Euler(90, 0, -90);
			}

			if (normal == Vector3.left)
			{
				return Quaternion.Euler(90, 0, 90);
			}

			if (normal == Vector3.forward)
			{
				return Quaternion.Euler(90, 0, 0);
			}

			if (normal == Vector3.back)
			{
				return Quaternion.Euler(90, 0, 180);
			}

			throw new ArgumentException("Can't attach tnt to wrong face of block");
		}
	}
}