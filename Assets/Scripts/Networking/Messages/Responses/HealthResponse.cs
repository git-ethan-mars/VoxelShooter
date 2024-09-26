namespace Networking.Messages.Responses
{
	public struct HealthResponse : IMirrorResponse
	{
		public readonly int Health;

		public HealthResponse(int health)
		{
			Health = health;
		}
	}
}