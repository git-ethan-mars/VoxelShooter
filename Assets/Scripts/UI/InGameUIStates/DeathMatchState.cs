using System;
using GamePlay;
using UI.Inventory;
namespace UI.InGameUIStates
{
	public class DeathMatchState : DefaultState
	{
		private readonly TimeInfo _timeInfo;

		private IDisposable _disposable;

		public DeathMatchState(CharacterProvider characterProvider, TimeInfo timeInfo, Hud hud, InventoryView inventoryView) : base
			(characterProvider, hud, inventoryView)
		{
			_timeInfo = timeInfo;
		}

		public override void Enter()
		{
			base.Enter();
			_timeInfo.CanvasGroup.alpha = 1.0f;
		}

		public override void Exit()
		{
			base.Exit();
			_timeInfo.CanvasGroup.alpha = 0.0f;
		}
	}
}