using Infrastructure.Services;
using Infrastructure.States;
using UI;
using UnityEngine;
using UnityEngine.Profiling;

namespace Infrastructure
{
    public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
    {
        [SerializeField] private LoadingCurtain curtainPrefab;
        private Game _game;

        private void Awake()
        {
            _game = new Game(this, Instantiate(curtainPrefab), AllServices.Container);
            ApplyGameSettings();
            _game.StateMachine.Enter<BootstrapState>();
            DontDestroyOnLoad(this);
        }

        private static void ApplyGameSettings()
        {
            Profiler.maxUsedMemory = int.MaxValue;
            Application.targetFrameRate = 60;
        }
    }
}