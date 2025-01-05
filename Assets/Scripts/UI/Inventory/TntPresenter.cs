using System;
using GamePlay;
using GamePlay.Data;
using UnityEngine;

namespace UI.Inventory
{
	public class TntDataProxy : ITntData
	{
		public int ID => _tntData.ID;

		public Sprite InventoryIcon => _tntData.InventoryIcon;

		public float DelayInSeconds => _tntData.DelayInSeconds;

		public AudioData CountdownSound => _tntData.CountdownSound;

		public int ParticlesSpeed => _tntData.ParticlesSpeed;

		public int ParticlesCount => _tntData.ParticlesCount;

		public AudioData ExplosionSound => _tntData.ExplosionSound;

		public int Amount
		{
			get => _tntData.Amount;
			set
			{
				_tntData.Amount = value;
				AmountChanged?.Invoke(value);
			}
		}

		public event Action<int> AmountChanged;

		private readonly TntData _tntData;

		public TntDataProxy(TntData tntData)
		{
			_tntData = tntData;
		}
	}

	public class TntPresenter : SlotPresenter
	{
		private readonly Tnt _tnt;
		private readonly Hud _hud;
		private readonly TntDataProxy _proxy;

		public TntPresenter(Tnt tnt, SlotView slotView, Hud hud) : base(tnt, slotView)
		{
			_tnt = tnt;
			_hud = hud;
			_proxy = new TntDataProxy(_tnt.Data);
		}

		public override void Initialize()
		{
			base.Initialize();
			_tnt.Selected += OnSelected;
			_tnt.Deselected += OnDeselected;
			_proxy.AmountChanged += OnAmountChanged;
		}

		public override void Dispose()
		{
			base.Dispose();
			_proxy.AmountChanged -= OnAmountChanged;
		}

		private void OnSelected()
		{
			_hud.ShowItemInfo(_tnt.InventoryIcon, _tnt.Data.Amount.ToString());
		}

		private void OnDeselected()
		{
			_hud.HideItemInfo();
		}

		private void OnAmountChanged(int amount)
		{
			_hud.SetItemCount(amount.ToString());
		}
	}
}