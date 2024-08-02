using System.Collections.Generic;
using CameraLogic;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Inventory;
using MapLogic;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;
using UI;
using UnityEngine;

namespace Blueprints
{
    public class BlueprintsSystem
    { 
        private readonly List<ConstructionState> _states = new();
        private readonly InventoryInput _inventoryInput;
        private int _index = -1;
        private readonly List<Blueprint> _blueprints;
        private readonly Hud _hud;
        private readonly Player _player;
        private readonly IMeshFactory _meshFactory;


        public BlueprintsSystem(IInputService inputService, IStaticDataService staticData, IMeshFactory meshFactory,
            Hud hud, MapProvider mapProvider, Player player)
        {
            var rayCaster = new RayCaster(Camera.main);
            _inventoryInput = new InventoryInput(inputService);
            _blueprints = staticData.GetBlueprints();
            _hud = hud;
            _player = player;
            _meshFactory = meshFactory;
            _states.Add(new ConstructionState(meshFactory, _inventoryInput, rayCaster, player, hud, mapProvider, _blueprints[2]));
            _states.Add(new ConstructionState(meshFactory, _inventoryInput, rayCaster, player, hud, mapProvider, _blueprints[1]));
            _states.Add(new ConstructionState(meshFactory, _inventoryInput, rayCaster, player, hud, mapProvider, _blueprints[0]));

            for (var i = 0; i < _states.Count; i++)
            {
                var previousColor = _hud.BlueprintsSlots[i].color;
                _hud.BlueprintsSlots[i].color = new Color(previousColor.r, previousColor.g, previousColor.b, 255);
                _hud.BlueprintsSlots[i].sprite = _states[i].Blueprint.image;
            }
            
            _inventoryInput.ZActionButtonDown += SetZIndex;
            _inventoryInput.XActionButtonDown += SetXIndex;
            _inventoryInput.CActionButtonDown += SetCIndex;
            _inventoryInput.ZActionButtonDown += ChangeSlotRequest;
            _inventoryInput.XActionButtonDown += ChangeSlotRequest;
            _inventoryInput.CActionButtonDown += ChangeSlotRequest;
            _inventoryInput.SlotButtonPressed += RemoveIndex;
            _inventoryInput.MouseScrolledDown += RemoveIndex;
            _inventoryInput.MouseScrolledUp += RemoveIndex;
        }

        private void ChangeSlotRequest()
        {
            foreach (Transform item in _player.ItemPosition)
            {
                Object.Destroy(item.gameObject);
            }

            _meshFactory.CreateGameModel(_states[_index].Blueprint.prefab,
                _player.ItemPosition);
        }

        private void SetZIndex()
        {
            if (_index != -1)
            {
                _states[_index].Exit();
                _hud.BlueprintsBoarders[_index].SetActive(false);
            }
            _index = 0;
            _states[_index].Enter();
            _hud.BlueprintsBoarders[_index].SetActive(true);
        }

        private void SetXIndex()
        {
            if (_index != -1)
            {
                _states[_index].Exit(); 
                _hud.BlueprintsBoarders[_index].SetActive(false);
            }
            _index = 1;
            _states[_index].Enter();
            _hud.BlueprintsBoarders[_index].SetActive(true);
        }
        
        private void SetCIndex()
        {
            if (_index != -1)
            {
                _states[_index].Exit();
                _hud.BlueprintsBoarders[_index].SetActive(false);
            }
            _index = 2;
            _states[_index].Enter();
            _hud.BlueprintsBoarders[_index].SetActive(true);
        }

        private void RemoveIndex(int index)
        {
            if (_index != -1)
            {
                _states[_index].Exit();
                _hud.BlueprintsBoarders[_index].SetActive(false);
            }
            _index = -1;
        }
        
        private void RemoveIndex()
        {
            if (_index != -1)
            {
                _states[_index].Exit();
                _hud.BlueprintsBoarders[_index].SetActive(false);
            }
            _index = -1;
        }

        public void Update()
        {
            _inventoryInput.Update();
            if (_index != -1)
            {
                _states[_index].Update(); 
            }
        }

        public void OnDestroy()
        {
            foreach (var state in _states)
            {
                state.Dispose();
            }

            if (_index != -1)
            {
                _states[_index].Exit();
            }
        }
    }
}