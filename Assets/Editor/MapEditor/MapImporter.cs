using System.IO;
using Data;
using Infrastructure;
using Services;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Callbacks;
using UnityEngine;
using VoxelMap;
using Object = UnityEngine.Object;

namespace Editor
{
	[ScriptedImporter(1, Constants.RchExtension)]
	public class MapImporter : ScriptedImporter
	{
		[OnOpenAsset(1)]
		public static bool OpenMapConfigureAsset(int instanceID)
		{
			Object obj = EditorUtility.InstanceIDToObject(instanceID);
			string assetPath = AssetDatabase.GetAssetPath(obj);

			if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(instanceID)) != typeof(MapConfigure) ||
			     Path.GetExtension(assetPath) != Constants.RchExtension || Application.isPlaying)
			{
				return false;
			}

			MapEditorSceneView.Open((MapConfigure)EditorUtility.InstanceIDToObject(instanceID));
			return true;
		}

		public override void OnImportAsset(AssetImportContext ctx)
		{
			string mapName = Path.GetFileNameWithoutExtension(ctx.assetPath);
			MapConfigureLoader mapConfigureLoader = new MapConfigureLoader(new AssetProvider());
			MapConfigure mapConfigure = mapConfigureLoader.GetMapConfigure(mapName);
			using MapData mapData = MapDataReader.ReadFromFile(mapName);
			Texture2D minimapTexture = GetMinimapTexture(mapData);
			ctx.AddObjectToAsset(mapName, mapConfigure, minimapTexture);
		}

		private Texture2D GetMinimapTexture(MapData mapData)
		{
			Color32[] colors = new Color32[mapData.Width * mapData.Depth];

			for (var x = 0; x < mapData.Width; x++)
			{
				for (var z = 0; z < mapData.Depth; z++)
				{
					colors[z * mapData.Width + x] = GetHighestBlockColor(mapData, x, z);
				}
			}

			Texture2D texture = new Texture2D(mapData.Width, mapData.Depth);

			texture.filterMode = FilterMode.Point;
			texture.SetPixels32(colors);
			texture.Apply();

			return texture;

		}

		private Color32 GetHighestBlockColor(MapData mapData, int x, int z)
		{
			for (var y = mapData.Height - 1; y >= 0; y--)
			{
				if (mapData[x, y, z] != VoxelData.Air)
				{
					return mapData[x, y, z].Color;
				}
			}

			return VoxelData.Air.Color;
		}
	}
}