using Common;

namespace Infrastructure.PlayerDataLoader
{
	public interface IAuthenticationService : IService
	{
		string GetPlayerNickName();
		ulong GetPlayerId();
	}
}