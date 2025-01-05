using System;
using GamePlay.Data;
using GamePlay.Entities;
using GamePlay.Services;
using UnityEngine;

namespace GamePlay
{
	public class Grenade : InventoryItem
	{
		public override Sprite InventoryIcon => Data.InventoryIcon;
		public GrenadeData Data { get; private set; }

		private IInputService _inputService;
		private IEntityFactory _entityFactory;
		private RayCaster _rayCaster;

		private float _holdDownStartTime;


		public void Construct(IInputService inputService, IEntityFactory entityFactory, GrenadeData grenadeData,
			RayCaster rayCaster)
		{
			_inputService = inputService;
			_entityFactory = entityFactory;
			Data = grenadeData;
			_rayCaster = rayCaster;
		}

		private void Update()
		{
			if (_inputService.IsFirstActionButtonDown())
			{
				PullPin();
			}

			if (_inputService.IsFirstActionButtonUp())
			{
				Throw();
			}
		}

		private void PullPin()
		{
			_holdDownStartTime = Time.time;
		}

		private void Throw()
		{
			var holdTime = Math.Min(Time.time - _holdDownStartTime, Data.MaxThrowDuration);
			var throwForce = Math.Max(holdTime * Data.ThrowForceModifier, Data.MinThrowForce);
			var ray = _rayCaster.CentredRay;
			var spawningGrenade = _entityFactory.CreateSpawningGrenade(ray.origin);
			spawningGrenade.Throw(ray.direction, throwForce);
		}
	}
}