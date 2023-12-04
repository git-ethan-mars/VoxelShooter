using Data;

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
            _playerData.Items = null;
            _playerData.ItemData = null;
            _playerData.CountByItem = null;
        }

        public void Exit()
        {
        }
    }
}