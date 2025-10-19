using Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMap;

namespace Editor
{
	public class MapEditorSceneView : SceneView
	{
		private MapConfigure _mapConfigure;
		private MapPreviewStage _stage;

		public static void Open(MapConfigure mapConfigure)
		{
			var sceneView = GetWindow<MapEditorSceneView>();
			sceneView._mapConfigure = mapConfigure;
			sceneView.Setup();
			sceneView.Repaint();
		}

		private void Setup()
		{
			_stage = CreateInstance<MapPreviewStage>();
			StageUtility.GoToStage(_stage, true);
			_stage.Setup(_mapConfigure);
			
			FrameSelected();
			titleContent = new GUIContent(_mapConfigure.name);
		}
	}
}