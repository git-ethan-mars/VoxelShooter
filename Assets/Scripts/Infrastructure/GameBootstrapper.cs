using Infrastructure.Services;
using Infrastructure.States;
using UnityEngine;
using UnityEngine.Profiling;

namespace Infrastructure
{
    public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
    {
        [SerializeField] private bool isLocalBuild;
        private Game _game;

        private void Awake()
        {
            _game = new Game(this, AllServices.Container, isLocalBuild);
            ApplyGameSettings();
            _game.StateMachine.Enter<BootstrapState>();
            DontDestroyOnLoad(this);
        }

        private static void ApplyGameSettings()
        {
            Profiler.maxUsedMemory = int.MaxValue;
        }
    }
}