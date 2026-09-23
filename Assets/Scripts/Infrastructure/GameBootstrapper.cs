using System.Collections.Generic;
using System.Linq;
using Infrastructure.States;
using Reflex.Attributes;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;

# endif

namespace Infrastructure
{
	public class GameBootstrapper : MonoBehaviour
	{
		private const string LocalBuild = "LOCAL_BUILD";
		[SerializeField]
		private bool isLocalBuild;

		private GameStateMachine _gameStateMachine;

		[Inject]
		private void Construct(GameStateMachine gameStateMachine)
		{
			_gameStateMachine = gameStateMachine;
		}

		private void Awake()
		{
			DontDestroyOnLoad(this);
		}

		private void Start()
		{
			_gameStateMachine.Enter<BootstrapState>();
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone, out string[] initialSymbols);
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
	}
}