using System;
using CameraLogic;
using Data;
using Mirror;
using Networking.Messages.Requests;
using UnityEngine;

namespace Inventory.Grenade
{
    public class GrenadeModel : IInventoryItemModel, IConsumable
    {
        public event Action ModelChanged;

        public int Amount
        {
            get => _grenadeItemData.Count;
            set
            {
                _grenadeItemData.Count = value;
                ModelChanged?.Invoke();
            }
        }

        private float _holdDownStartTime;
        private readonly RayCaster _rayCaster;
        private readonly float _minThrowForce;
        private readonly float _maxThrowDuration;
        private readonly float _throwForceModifier;
        private readonly GrenadeItemData _grenadeItemData;


        public GrenadeModel(RayCaster rayCaster, GrenadeItem configure, GrenadeItemData data)
        {
            _rayCaster = rayCaster;
            _minThrowForce = configure.minThrowForce;
            _maxThrowDuration = configure.maxThrowDuration;
            _throwForceModifier = configure.throwForceModifier;
            _grenadeItemData = data;
        }

        public void PullPin()
        {
            _holdDownStartTime = Time.time;
        }

        public void Throw()
        {
            var holdTime = Math.Min(Time.time - _holdDownStartTime, _maxThrowDuration);
            var throwForce = Math.Max(holdTime * _throwForceModifier, _minThrowForce);
            NetworkClient.Send(new GrenadeSpawnRequest(_rayCaster.CentredRay, throwForce));
        }
    }
}