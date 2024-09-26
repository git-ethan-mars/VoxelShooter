using System.Collections;
using Common;
using Common.Factory;
using Common.StaticData;
using Explosions;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using UnityEngine;

namespace Networking.Host.Services
{
	public class MeleeWeaponValidator
	{
		private readonly IHost _host;
		private readonly ICoroutineRunner _coroutineRunner;
		private readonly IParticleFactory _particleFactory;
		private readonly MapUpdater _mapUpdater;
		private readonly SingleBlockArea _weakHitArea;
		private readonly LineBlockArea _strongHitArea;

		public MeleeWeaponValidator(IHost host, ICoroutineRunner coroutineRunner, IParticleFactory particleFactory,
			MapUpdater mapUpdater)
		{
			_host = host;
			_coroutineRunner = coroutineRunner;
			_particleFactory = particleFactory;
			_mapUpdater = mapUpdater;
			_weakHitArea = new SingleBlockArea(_mapUpdater);
			_strongHitArea = new LineBlockArea(_mapUpdater);
		}

		public void Hit(NetworkConnectionToClient connection, Ray ray, bool isStrongHit)
		{
			var playerData = _host.GetPlayerData(connection);
			var meleeWeaponData = (MeleeWeaponData) playerData.SelectedItemData;

			if (!CanHit(meleeWeaponData))
			{
				return;
			}

			var isSurface = ApplyRaycast(connection, ray, meleeWeaponData, isStrongHit && meleeWeaponData.HasStrongHit);
			_coroutineRunner.StartCoroutine(ResetHit(connection, meleeWeaponData));
			_host.SendAudio(isSurface ? meleeWeaponData.DiggingAudio : meleeWeaponData.HittingAudio,
				connection.identity);
		}

		private IEnumerator ResetHit(NetworkConnectionToClient connection, MeleeWeaponData data)
		{
			data.IsReady = false;
			var waitForHitReset = new WaitWithoutSlotChange(_host, connection, data.TimeBetweenHit);
			while (true)
			{
				yield return waitForHitReset;
				if (waitForHitReset.CompletedSuccessfully || waitForHitReset.IsAborted)
				{
					break;
				}

				waitForHitReset = new WaitWithoutSlotChange(_host, connection, data.TimeBetweenHit);
			}

			if (waitForHitReset.CompletedSuccessfully)
			{
				data.IsReady = true;
			}
		}

		private bool ApplyRaycast(NetworkConnectionToClient source, Ray ray, MeleeWeaponData meleeWeaponData,
			bool isStrongHit)
		{
			var raycastResult = Physics.Raycast(ray, out var rayHit, meleeWeaponData.Range, Constants.attackMask);
			if (!raycastResult) return false;
			if (rayHit.collider.CompareTag("Head"))
			{
				HitImpact(source, rayHit, (int) (meleeWeaponData.HeadMultiplier * meleeWeaponData.DamageToPlayer));
			}

			if (rayHit.collider.CompareTag("Leg"))
			{
				HitImpact(source, rayHit, (int) (meleeWeaponData.LegMultiplier * meleeWeaponData.DamageToPlayer));
			}

			if (rayHit.collider.CompareTag("Chest"))
			{
				HitImpact(source, rayHit, (int) (meleeWeaponData.ChestMultiplier * meleeWeaponData.DamageToPlayer));
			}

			if (rayHit.collider.CompareTag("Arm"))
			{
				HitImpact(source, rayHit, (int) (meleeWeaponData.ArmMultiplier * meleeWeaponData.DamageToPlayer));
			}

			if (rayHit.collider.CompareTag("Chunk"))
			{
				var targetBlock = Vector3Int.FloorToInt(rayHit.point - rayHit.normal / 2);
				_particleFactory.CreateBlockDestructionParticle(targetBlock
				                                                + new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity,
					_mapUpdater.GetBlockByGlobalPosition(targetBlock).Color);
				if (isStrongHit)
				{
					_mapUpdater.DamageBlocks(_strongHitArea, targetBlock, meleeWeaponData.DamageToBlock);
				}
				else
				{
					_mapUpdater.DamageBlocks(_weakHitArea, targetBlock, meleeWeaponData.DamageToBlock);
				}
				return true;
			}

			return false;
		}

		private void HitImpact(NetworkConnectionToClient source, RaycastHit rayHit, int damage)
		{
			var receiver = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
			if (source != receiver)
			{
				_host.Damage(source, receiver, damage);
				var blood = _particleFactory.CreateBlood(rayHit.point, Quaternion.LookRotation(rayHit.normal));
				_host.SpawnParticles(blood);
			}
		}

		private bool CanHit(MeleeWeaponData meleeWeapon)
		{
			return meleeWeapon.IsReady;
		}
	}
}