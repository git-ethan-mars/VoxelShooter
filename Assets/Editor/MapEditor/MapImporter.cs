using System.IO;
using Services;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Callbacks;
using UnityEngine;
using VoxelMap;
using VoxelMap.Data;
using Object = UnityEngine.Object;

namespace Editor
{
	[ScriptedImporter(1, Constants.RchExtension)]
	public class MapImporter : ScriptedImporter
	{
		[OnOpenAsset(1)]
		public static bool OnOpenAsset(EntityId entityId, int line)
		{
			Object obj = EditorUtility.EntityIdToObject(entityId);
			string assetPath = AssetDatabase.GetAssetPath(obj);

			if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(entityId)) != typeof(MapConfigure) ||
			    Path.GetExtension(assetPath) != Constants.RchExtension || Application.isPlaying)
			{
				return false;
			}

			MapEditorSceneView.Open((MapConfigure)EditorUtility.EntityIdToObject(entityId));
			return true;
		}

		public override void OnImportAsset(AssetImportContext ctx)
		{
			string mapName = Path.GetFileNameWithoutExtension(ctx.assetPath);
			var mapConfigureLoader = new MapConfigureLoader();
			MapConfigure mapConfigure = mapConfigureLoader.GetMapConfigure(mapName);
			using MapData mapData = MapDataReader.ReadFromFile(mapName);
			Texture2D minimapTexture = GetMinimapTexture(mapData);
			ctx.AddObjectToAsset(mapName, mapConfigure, minimapTexture);
		}

		private Texture2D GetMinimapTexture(MapData mapData)
		{
			var colors = new Color32[mapData.Width * mapData.Depth];

			for (ushort x = 0; x < mapData.Width; x++)
			{
				for (ushort z = 0; z < mapData.Depth; z++)
				{
					colors[z * mapData.Width + x] = GetHighestBlockColor(mapData, x, z);
				}
			}

			var texture = new Texture2D(mapData.Width, mapData.Depth);

			texture.filterMode = FilterMode.Point;
			texture.SetPixels32(colors);
			texture.Apply();

			return texture;
		}

		private Color32 GetHighestBlockColor(MapData mapData, ushort x, ushort z)
		{
			ushort y = (ushort)(mapData.Height - 1);

			if (mapData[x, y, z] != VoxelData.Air)
			{
				return mapData[x, y, z].Color;
			}

			do
			{
				y--;

				if (mapData[x, y, z] != VoxelData.Air)
				{
					return mapData[x, y, z].Color;
				}
			} while (y > 0);

			return VoxelData.Air.Color;
		}
	}
}
