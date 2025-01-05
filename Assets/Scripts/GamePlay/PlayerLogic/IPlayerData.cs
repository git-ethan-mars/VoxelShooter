using GamePlay.Data;

namespace GamePlay
{
	public interface IPlayerData
	{
		string NickName { get; }
		GameClass GameClass { get; }
	}
}