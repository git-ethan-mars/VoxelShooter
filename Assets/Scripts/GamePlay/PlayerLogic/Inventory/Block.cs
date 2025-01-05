using System.Collections.Generic;
using GamePlay.Data;
using GamePlay.MapFeatures;
using GamePlay.Services;
using UnityEngine;
using Gradient = GamePlay.Data.Gradient;

namespace GamePlay
{
	public class Block : InventoryItem
	{
		[SerializeField] 
		private List<Gradient> gradients;
		public override Sprite InventoryIcon => Data.InventoryIcon;
		private RayCaster _rayCaster;
		private IInputService _inputService;
		public BlockData Data { get; private set; }

		public void Construct(IInputService inputService, BlockData data, RayCaster rayCaster)
		{
			_inputService = inputService;
			Data = data;
			_rayCaster = rayCaster;
		}

		private void Update()
		{
			if (_inputService.IsFirstActionButtonDown())
			{
				PlaceBlock();
			}
		}

		private void PlaceBlock()
		{
			var rayCastResult = _rayCaster.GetBuildRayCastHit(out var rayCastHit);
			if (rayCastResult)
			{
				if (rayCastHit.collider.TryGetComponent<IBuildVisitor>(out var buildVisitor))
				{
					buildVisitor.Visit(Data, rayCastHit);
				}
			}
		}
	}
}