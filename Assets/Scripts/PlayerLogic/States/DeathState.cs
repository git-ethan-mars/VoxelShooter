using Data;
using UnityEngine;

namespace PlayerLogic.States
{
    public class DeathState : IPlayerState
    {
        private readonly PlayerData _playerData;

        public DeathState(PlayerData playerData)
        {
            _playerData = playerData;
        }

        public void Enter()
        {
            _playerData.IsAlive = false;
            _playerData.Deaths += 1;
             _playerData.Characteristic = null;
            _playerData.Health = 0;
            _playerData.ItemCountById = null;
            _playerData.ItemsId = null;
            _playerData.RangeWeaponsById  = null;
            _playerData.MeleeWeaponsById  = null;
            _playerData.ItemCountById = null;
        }

        public void Exit()
        {
        }
    }
}