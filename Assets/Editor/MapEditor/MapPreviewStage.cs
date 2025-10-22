using Data;
using Services;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMap;
namespace Editor
{
	public class MapPreviewStage : PreviewSceneStage
	{
		private Map _map;
		private MapEditorWindow _editorWindow;

		public void Setup(MapConfigure mapConfigure)
		{
			var mapFactory = new MapFactory(new AssetProvider());
			MapData mapData = MapDataReader.ReadFromFile(mapConfigure.name);
			MapBuilder mapBuilder = new MapBuilder(mapFactory, mapData)
				.FromConfigure(mapConfigure)
				.WithSpawnPoints(mapConfigure.SpawnPoints);
			_map = mapBuilder.Build();

			StageUtility.PlaceGameObjectInCurrentStage(_map.gameObject);
			Selection.activeObject = _map.gameObject;

			_editorWindow = EditorWindow.GetWindow<MapEditorWindow>();
			_editorWindow.Setup(mapConfigure);

			_editorWindow.Repaint();
		}

		protected override GUIContent CreateHeaderContent()
		{
			return new GUIContent(nameof(MapPreviewStage));
		}

		protected override void OnCloseStage()
		{
			base.OnCloseStage();

			if (_map != null)
			{
				DestroyImmediate(_map.gameObject);
			}
			
			_editorWindow?.Close();
		}
	}
}