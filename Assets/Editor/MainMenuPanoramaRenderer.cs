using System.IO;
using System.Linq;
using Services;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using VoxelMap;

namespace Editor
{
	// Builds a map in a temporary scene, captures a 360° panorama from a chosen point and makes it the main menu skybox.
	public class MainMenuPanoramaRenderer : EditorWindow
	{
		private const string TexturePath = "Assets/Skybox/MainMenuPanorama.png";
		private const string MaterialPath = "Assets/Skybox/MainMenuPanorama.mat";
		private const string GameMenuPath = "Assets/Resources/Prefabs/UI/GameMenu.prefab";
		private const string PanoramicShaderName = "Skybox/Panoramic";
		private const float FarClipPlane = 1500;

		// Each face is rendered with a 90° camera looking along forward with the given up vector.
		private static readonly (Vector3 Forward, Vector3 Up)[] Faces =
		{
			(Vector3.right, Vector3.up), (Vector3.left, Vector3.up),
			(Vector3.up, Vector3.back), (Vector3.down, Vector3.forward),
			(Vector3.forward, Vector3.up), (Vector3.back, Vector3.up)
		};

		private string[] _mapNames;
		private int _mapIndex;
		private bool _useMapCenter = true;
		private Vector2Int _position;
		private float _heightAboveGround = 3;
		private int _faceSize = 1024;

		[MenuItem("Tools/Render Main Menu Panorama")]
		public static void Open()
		{
			GetWindow<MainMenuPanoramaRenderer>("Menu Panorama");
		}

		private void OnEnable()
		{
			_mapNames = MapDataReader.GetExistedMaps().ToArray();
		}

		private void OnGUI()
		{
			if (_mapNames.Length == 0)
			{
				EditorGUILayout.HelpBox("No maps found in Assets/Maps.", MessageType.Warning);
				return;
			}

			_mapIndex = EditorGUILayout.Popup("Map", _mapIndex, _mapNames);
			_useMapCenter = EditorGUILayout.Toggle("Use map center", _useMapCenter);

			using (new EditorGUI.DisabledScope(_useMapCenter))
			{
				_position = EditorGUILayout.Vector2IntField("Position (X, Z)", _position);
			}

			_heightAboveGround = EditorGUILayout.FloatField("Height above ground", _heightAboveGround);
			_faceSize = EditorGUILayout.IntPopup("Face size", _faceSize, new[] { "512", "1024", "2048" }, new[] { 512, 1024, 2048 });

			if (GUILayout.Button("Render"))
			{
				Render();
			}
		}

		private void Render()
		{
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				return;
			}

			SceneSetup[] sceneSetup = EditorSceneManager.GetSceneManagerSetup();
			string mapName = _mapNames[_mapIndex];
			MapData mapData = default;
			bool isMapDataRead = false;
			try
			{
				EditorUtility.DisplayProgressBar("Menu Panorama", $"Building {mapName}", 0);
				EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

				mapData = MapDataReader.ReadFromFile(mapName);
				isMapDataRead = true;
				Map map = new MapBuilder()
					.FromConfigure(new MapConfigureLoader().GetMapConfigure(mapName))
					.Build(mapData, mapName);

				Vector3 cameraPosition = GetCameraPosition(map);
				EditorUtility.DisplayProgressBar("Menu Panorama", "Rendering faces", 0.3f);
				Color32[][] faces = RenderFaces(cameraPosition);

				EditorUtility.DisplayProgressBar("Menu Panorama", "Building panorama", 0.6f);
				SavePanorama(faces);
				AssignToGameMenu(CreateMaterial());

				Object.DestroyImmediate(map.gameObject);
			}
			finally
			{
				// Edit mode never calls Map.OnDestroy, so the voxel data is released here.
				if (isMapDataRead)
				{
					mapData.Dispose();
				}

				EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
				EditorUtility.ClearProgressBar();
			}

			Debug.Log($"Main menu panorama rendered from {mapName}");
		}

		private Vector3 GetCameraPosition(Map map)
		{
			ushort x = (ushort)Mathf.Clamp(_useMapCenter ? map.Width / 2 : _position.x, 0, map.Width - 1);
			ushort z = (ushort)Mathf.Clamp(_useMapCenter ? map.Depth / 2 : _position.y, 0, map.Depth - 1);
			int groundY = 0;

			for (int y = map.Height - 1; y >= 0; y--)
			{
				if (map.GetVoxelByGlobalPosition(x, (ushort)y, z).IsSolid())
				{
					groundY = y + 1;
					break;
				}
			}

			return new Vector3(x + 0.5f, groundY + _heightAboveGround, z + 0.5f);
		}

		private Color32[][] RenderFaces(Vector3 position)
		{
			var cameraObject = new GameObject("Panorama Camera");
			Camera camera = cameraObject.AddComponent<Camera>();
			camera.fieldOfView = 90;
			camera.aspect = 1;
			camera.farClipPlane = FarClipPlane;
			camera.clearFlags = CameraClearFlags.Skybox;
			cameraObject.transform.position = position;

			var renderTexture = new RenderTexture(_faceSize, _faceSize, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			var readTexture = new Texture2D(_faceSize, _faceSize, TextureFormat.RGBA32, false);
			var faces = new Color32[Faces.Length][];

			try
			{
				for (int i = 0; i < Faces.Length; i++)
				{
					cameraObject.transform.rotation = Quaternion.LookRotation(Faces[i].Forward, Faces[i].Up);
					RenderCamera(camera, renderTexture);

					RenderTexture previous = RenderTexture.active;
					RenderTexture.active = renderTexture;
					readTexture.ReadPixels(new Rect(0, 0, _faceSize, _faceSize), 0, 0);
					readTexture.Apply();
					RenderTexture.active = previous;
					faces[i] = readTexture.GetPixels32();
				}
			}
			finally
			{
				Object.DestroyImmediate(cameraObject);
				Object.DestroyImmediate(readTexture);
				renderTexture.Release();
				Object.DestroyImmediate(renderTexture);
			}

			return faces;
		}

		private static void RenderCamera(Camera camera, RenderTexture target)
		{
			var request = new RenderPipeline.StandardRequest { destination = target };

			if (RenderPipeline.SupportsRenderRequest(camera, request))
			{
				RenderPipeline.SubmitRenderRequest(camera, request);
				return;
			}

			camera.targetTexture = target;
			camera.Render();
			camera.targetTexture = null;
		}

		// Converts the six faces into a latitude-longitude image for the Skybox/Panoramic shader.
		private void SavePanorama(Color32[][] faces)
		{
			int width = _faceSize * 4;
			int height = _faceSize * 2;
			var pixels = new Color32[width * height];

			for (int row = 0; row < height; row++)
			{
				float latitude = (1 - (row + 0.5f) / height) * Mathf.PI;

				for (int column = 0; column < width; column++)
				{
					float longitude = (0.5f - (column + 0.5f) / width) * 2 * Mathf.PI;
					var direction = new Vector3(Mathf.Sin(latitude) * Mathf.Cos(longitude), Mathf.Cos(latitude),
						Mathf.Sin(latitude) * Mathf.Sin(longitude));
					pixels[row * width + column] = SampleFaces(faces, direction);
				}
			}

			var panorama = new Texture2D(width, height, TextureFormat.RGBA32, false);
			panorama.SetPixels32(pixels);
			File.WriteAllBytes(TexturePath, panorama.EncodeToPNG());
			Object.DestroyImmediate(panorama);

			AssetDatabase.ImportAsset(TexturePath);
			var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
			importer.mipmapEnabled = false;
			importer.wrapModeU = TextureWrapMode.Repeat;
			importer.wrapModeV = TextureWrapMode.Clamp;
			importer.maxTextureSize = Mathf.Max(width, 2048);
			importer.SaveAndReimport();
		}

		private Color32 SampleFaces(Color32[][] faces, Vector3 direction)
		{
			float absX = Mathf.Abs(direction.x);
			float absY = Mathf.Abs(direction.y);
			float absZ = Mathf.Abs(direction.z);
			int face = absX >= absY && absX >= absZ ? direction.x > 0 ? 0 : 1
				: absY >= absZ ? direction.y > 0 ? 2 : 3
				: direction.z > 0 ? 4 : 5;

			Vector3 forward = Faces[face].Forward;
			Vector3 up = Faces[face].Up;
			Vector3 right = Vector3.Cross(up, forward);
			float depth = Vector3.Dot(direction, forward);
			float u = (Vector3.Dot(direction, right) / depth + 1) * 0.5f;
			float v = (Vector3.Dot(direction, up) / depth + 1) * 0.5f;

			return SampleBilinear(faces[face], u, v);
		}

		private Color32 SampleBilinear(Color32[] pixels, float u, float v)
		{
			float x = Mathf.Clamp(u * _faceSize - 0.5f, 0, _faceSize - 1);
			float y = Mathf.Clamp(v * _faceSize - 0.5f, 0, _faceSize - 1);
			int x0 = (int)x;
			int y0 = (int)y;
			int x1 = Mathf.Min(x0 + 1, _faceSize - 1);
			int y1 = Mathf.Min(y0 + 1, _faceSize - 1);
			float tx = x - x0;
			float ty = y - y0;

			Color32 bottom = Color32.Lerp(pixels[y0 * _faceSize + x0], pixels[y0 * _faceSize + x1], tx);
			Color32 top = Color32.Lerp(pixels[y1 * _faceSize + x0], pixels[y1 * _faceSize + x1], tx);
			return Color32.Lerp(bottom, top, ty);
		}

		private static Material CreateMaterial()
		{
			var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

			if (material == null)
			{
				material = new Material(Shader.Find(PanoramicShaderName));
				AssetDatabase.CreateAsset(material, MaterialPath);
			}

			material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath));
			EditorUtility.SetDirty(material);
			AssetDatabase.SaveAssets();
			return material;
		}

		private static void AssignToGameMenu(Material material)
		{
			GameObject root = PrefabUtility.LoadPrefabContents(GameMenuPath);
			try
			{
				MainMenuPanorama panorama = root.GetComponent<MainMenuPanorama>();

				if (panorama == null)
				{
					panorama = root.AddComponent<MainMenuPanorama>();
				}

				var serializedPanorama = new SerializedObject(panorama);
				serializedPanorama.FindProperty("panoramaSkybox").objectReferenceValue = material;
				serializedPanorama.ApplyModifiedPropertiesWithoutUndo();
				PrefabUtility.SaveAsPrefabAsset(root, GameMenuPath);
			}
			finally
			{
				PrefabUtility.UnloadPrefabContents(root);
			}
		}
	}
}
