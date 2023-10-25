using Infrastructure.Services;
using Infrastructure.States;
using UnityEngine;

namespace Infrastructure
{
    public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
    {
        [SerializeField]
        private bool isLocalBuild;

        private Game _game;

        private void Awake()
        {
            typeof(Constants).GetField("isLocalBuild").SetValue(null, isLocalBuild);
            _game = new Game(this, AllServices.Container);
            _game.StateMachine.Enter<BootstrapState>();
            DontDestroyOnLoad(this);
        }
    }
}