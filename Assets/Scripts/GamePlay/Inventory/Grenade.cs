using System;
using Data;
using GamePlay.Core;
using Mirror;
using Networking.Core;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class Grenade : InventoryItem
	{
		private IInputService _inputService;
		private IEntityFactory _entityFactory;
		private CameraProvider _cameraProvider;

		private float _holdDownStartTime;
		private readonly SyncReactiveProperty<int> _amount = new SyncReactiveProperty<int>();
		public ReactiveProperty<int> Amount => _amount;
		
		[Inject]
		private void Construct(IInputService inputService, IEntityFactory entityFactory, CameraProvider cameraProvider,
			IStaticDataService staticData)
		{
			_inputService = inputService;
			_entityFactory = entityFactory;
			_cameraProvider = cameraProvider;
		}
		
		public override void OnStartServer()
		{
			base.OnStartServer();

			_amount.Value = Configure.Amount;
		}

		public override ItemType Type => ItemType.Grenade;
		private new GrenadeConfigure Configure => base.Configure as GrenadeConfigure;

		private void Update()
		{
			if (!IsLocalItem)
			{
				return;
			}
			
			if (_inputService.IsFirstActionButtonDown())
			{
				PullPin();
			}

			if (_inputService.IsFirstActionButtonUp())
			{
				Throw(_cameraProvider.CentredRay);
			}
		}

		[Command]
		private void PullPin()
		{
			if (_amount.Value > 0)
			{
				_holdDownStartTime = Time.time;
			}
		}

		[Command]
		private void Throw(Ray ray)
		{
			if (_amount.Value <= 0)
			{
				return;
			}
			
			float holdTime = Math.Min(Time.time - _holdDownStartTime, Configure.MaxThrowDuration);
			float throwForce = Math.Max(holdTime * Configure.ThrowForceModifier, Configure.MinThrowForce);
			SpawningGrenade spawningGrenade = _entityFactory.CreateSpawningGrenade(ray.origin);
			spawningGrenade.Throw(ray.direction, throwForce);

			_amount.Value -= 1;
		}
	}
}