using System.Globalization;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
	public class LocalAuthenticationService : IAuthenticationService
	{
		public string GetPlayerNickName()
		{
			return Random.value.ToString(CultureInfo.InvariantCulture);
		}

		public ulong GetPlayerId()
		{
			return 0;
		}
	}
}