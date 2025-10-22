using Data;
namespace Services
{
	public interface IPlayerService
	{
		void AddPlayer(int playerId, PlayerData playerData);
		void RemovePlayer(int playerId);
		bool TryGetPlayerData(int playerId, out PlayerData playerData);
		void ResetData();
	}
}