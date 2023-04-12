using Data;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Networking.Synchronization;
using TMPro;
using UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Inventory
{
    public class GunSystem : IInventoryItemView, ILeftMouseButtonDownHandler, ILeftMouseButtonHoldHandler, IUpdated
    {
        public Sprite Icon { get; }
        private GameObject Model { get; }
        private readonly Weapon _weapon;
        private readonly TextMeshProUGUI _ammoInfo;
        private readonly Transform _attackPoint;
        private readonly Camera _fpsCam;
        private readonly IGameFactory _gameFactory;
        private readonly WeaponSynchronization _bulletSynchronization;
        private readonly IInputService _inputService;


        public GunSystem(IGameFactory gameFactory, IInputService inputService, Camera camera,
            Transform itemPosition, GameObject player, GameObject hud, Weapon configuration)
        {
            _gameFactory = gameFactory;
            _inputService = inputService;
            _weapon = configuration;
            Model = Object.Instantiate(_weapon.Prefab,
                itemPosition);
            Model.SetActive(false);
            Icon = _weapon.Icon;
            _attackPoint = Model.GetComponentInChildren<Transform>();
            _fpsCam = camera;
            _ammoInfo = hud.GetComponent<Hud>().ammoCount.GetComponent<TextMeshProUGUI>();
            _bulletSynchronization = player.GetComponent<WeaponSynchronization>();
            }

        public void Select()
        {
            _ammoInfo.gameObject.SetActive(true);
            Model.SetActive(true);
        }


        public void Unselect()
        {
            _ammoInfo.gameObject.SetActive(false);
            Model.SetActive(false);
        }

        public void InnerUpdate()
        {
            if (_inputService.IsReloadingButtonDown())
                _bulletSynchronization.CmdReload(_weapon.ID);
            _ammoInfo.SetText($"{_weapon.BulletsInMagazine} / {_weapon.TotalBullets}");
        }

        public void OnLeftMouseButtonDown()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f,0.5f));
            _bulletSynchronization.CmdShootSingle(ray, _weapon.ID);
        }

        public void OnLeftMouseButtonHold()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _bulletSynchronization.CmdShootAutomatic(ray,_weapon.ID);
        }
    }
}