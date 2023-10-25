using Infrastructure.Services;
using Infrastructure.States;

namespace Infrastructure
{
    public class Game
    {
        public readonly GameStateMachine StateMachine;

        public Game(ICoroutineRunner coroutineRunner, AllServices allServices)
        {
            StateMachine = new GameStateMachine(new SceneLoader(coroutineRunner), coroutineRunner, allServices);
        }
    }
}