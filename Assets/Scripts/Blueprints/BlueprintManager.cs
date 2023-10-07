using CameraLogic;
using Infrastructure.Services.Input;
using UnityEngine;

namespace Blueprints
{
    public class BlueprintManager : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private MeshCollider meshCollider;

        [SerializeField]
        private Transform plane;

        [SerializeField]
        private FreeCamera freeCamera;

        private bool IsMenuActivated
        {
            get => _isMenuActivated;
            set
            {
                if (value)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    freeCamera.enabled = false;
                }
                else
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    freeCamera.enabled = true;
                }

                _isMenuActivated = value;
            }
        }

        private IInputService _inputService;
        private BlueprintBuilder _builder;
        private BlueprintUI _ui;

        private bool _isMenuActivated;

        private void Awake()
        {
            Debug.Log(Screen.currentResolution);
            _builder = new BlueprintBuilder(meshFilter, meshCollider, plane);
            _ui = new BlueprintUI(_builder);
            _inputService = new StandaloneInputService();
            IsMenuActivated = true;
        }

        private void Update()
        {
            if (_inputService.IsMenuButtonPressed() && _ui.State == BlueprintGUIState.Build)
            {
                IsMenuActivated = !IsMenuActivated;
            }

            if (IsMenuActivated)
            {
                return;
            }

            if (_inputService.IsFirstActionButtonDown())
            {
                _builder.CreateBlock();
            }

            if (_inputService.IsSecondActionButtonDown())
            {
                _builder.RemoveBlock();
            }
        }

        private void OnGUI()
        {
            if (_ui.State == BlueprintGUIState.Create)
            {
                _ui.DrawCreateMenu();
            }

            if (_ui.State == BlueprintGUIState.Choose)
            {
                _ui.DrawChooseMenu();
            }

            if (_ui.State == BlueprintGUIState.Build && IsMenuActivated)
            {
                _ui.DrawBuildMenu();
            }
        }
    }
}