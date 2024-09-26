using Networking.Messages.Responses;

namespace Networking.Client
{
	public interface IResponseHandler<TResponse> where TResponse : IMirrorResponse
	{
		void OnResponseReceived(TResponse response);
	}
}