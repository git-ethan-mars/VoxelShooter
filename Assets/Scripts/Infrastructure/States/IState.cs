using Cysharp.Threading.Tasks;

namespace Infrastructure.States
{
	public interface IState : IExitableState
	{
		UniTask EnterAsync();
	}
}
