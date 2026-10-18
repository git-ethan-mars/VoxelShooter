using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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

		[SerializeField] private bool isLocalBuild;

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

		private async UniTaskVoid Start()
		{
			await _gameStateMachine.EnterAsync<BootstrapState>();
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			// Only the prefab asset owns the define, otherwise copies with different values toggle it in a recompile loop
			if (Application.isPlaying || !EditorUtility.IsPersistent(this))
				return;

			EditorApplication.delayCall += UpdateLocalBuildSymbol;
		}

		private void UpdateLocalBuildSymbol()
		{
			if (this == null)
				return;

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
