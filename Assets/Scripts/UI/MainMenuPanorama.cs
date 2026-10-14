using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	// Shows the pre-rendered map panorama as the menu skybox and slowly spins it, like the Minecraft title screen.
	public class MainMenuPanorama : MonoBehaviour
	{
		private static readonly int RotationProperty = Shader.PropertyToID("_Rotation");

		[SerializeField] private Material panoramaSkybox;
		[SerializeField] private Image backgroundImage;
		[SerializeField] private float rotationSpeed = 2.0f;
		[SerializeField, Range(-30, 30)] private float pitch = 5.0f;

		// A runtime copy, so spinning does not modify the material asset in the editor.
		private Material _skybox;
		private Camera _camera;
		private float _rotation;

		private void Start()
		{
			_camera = Camera.main;

			if (panoramaSkybox == null || _camera == null)
			{
				enabled = false;
				return;
			}

			_skybox = new Material(panoramaSkybox);
			RenderSettings.skybox = _skybox;
			_camera.clearFlags = CameraClearFlags.Skybox;
			backgroundImage.enabled = false;
			_rotation = Random.Range(0, 360.0f);
		}

		private void Update()
		{
			// Applied every frame so the pitch can be tuned from the inspector in play mode.
			_camera.transform.rotation = Quaternion.Euler(pitch, 0, 0);
			_rotation = Mathf.Repeat(_rotation + rotationSpeed * Time.unscaledDeltaTime, 360);
			_skybox.SetFloat(RotationProperty, _rotation);
		}

		private void OnDestroy()
		{
			if (_skybox != null)
			{
				Destroy(_skybox);
			}
		}
	}
}
