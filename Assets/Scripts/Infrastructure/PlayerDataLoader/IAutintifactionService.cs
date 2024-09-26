using Common;

namespace Infrastructure.Services.PlayerDataLoader
{
	public interface IAuthenticationService : IService
	{
		string GetPlayerNickName();
		ulong GetPlayerId();
	}
}