using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Services;
using Infrastructure.Services;
using Infrastructure.States;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;

# endif

namespace Infrastructure
{
	public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
	{
		[SerializeField]
		private bool isLocalBuild;

		private const string LocalBuild = "LOCAL_BUILD";

		private Game _game;

#if UNITY_EDITOR
		private void OnValidate()
		{
			PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone, out var initialSymbols);
			var symbols = new List<string>(initialSymbols);
			if (isLocalBuild && !initialSymbols.Contains(LocalBuild))
			{
				symbols.Add(LocalBuild);
				PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, string.Join(';', symbols));
			}
			if (!isLocalBuild && initialSymbols.Contains(LocalBuild))
			{
				symbols.Remove(LocalBuild);
				PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, string.Join(';', symbols));
			}
		}
#endif

		private void Awake()
		{
			_game = new Game(this, AllServices.Container);
			_game.StateMachine.Enter<BootstrapState>();
			DontDestroyOnLoad(this);
		}
	}
}