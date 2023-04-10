using Infrastructure.Services;
using Infrastructure.States;
using UI;

namespace Infrastructure
{
    public class Game
    {
        public readonly GameStateMachine StateMachine;

        public Game(ICoroutineRunner coroutineRunner, LoadingCurtain loadingCurtain, AllServices allServices)
        {
            StateMachine = new GameStateMachine(coroutineRunner,new SceneLoader(coroutineRunner), allServices);
        }
    }
}