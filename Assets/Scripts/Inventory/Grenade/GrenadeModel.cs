using System;
using Data;
using Infrastructure;
using Mirror;
using Networking.Messages.Requests;
using UnityEngine;

namespace Inventory
{
    public class GrenadeModel : IInventoryItemModel, IConsumable
    {
        public ObservableVariable<int> Count { get; set; }
        private float _holdDownStartTime;
        private readonly RayCaster _rayCaster;
        private readonly float _minThrowForce;
        private readonly float _maxThrowDuration;
        private readonly float _throwForceModifier;


        public GrenadeModel(RayCaster rayCaster, GrenadeItem configure)
        {
            _rayCaster = rayCaster;
            _minThrowForce = configure.minThrowForce;
            _maxThrowDuration = configure.maxThrowDuration;
            _throwForceModifier = configure.throwForceModifier;
            Count = new ObservableVariable<int>(configure.count);
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