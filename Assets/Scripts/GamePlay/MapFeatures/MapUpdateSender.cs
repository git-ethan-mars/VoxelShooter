using System.Collections.Generic;
using Networking;
using Networking.Messages;
using R3;
using Reflex.Attributes;
using UnityEngine;
using VoxelMap;
namespace GamePlay.MapFeatures
{
	public class MapUpdateSender : MapFeature
	{
		private MapProvider _mapProvider;
		private VSNetworkManager _networkManager;
		
		private readonly List<Voxel> _addedVoxels = new List<Voxel>();
		private readonly List<Vector3Ushort> _removedPositions = new List<Vector3Ushort>();

		[Inject]
		private void Construct(MapProvider mapProvider, VSNetworkManager networkManager)
		{
			_mapProvider = mapProvider;
			_networkManager = networkManager;
		}

		private void Start()
		{
			_mapProvider.Map.VoxelsAdded.Subscribe(OnVoxelsAdded).AddTo(this);
			_mapProvider.Map.VoxelsRemoved.Subscribe(OnVoxelsRemoved).AddTo(this);
		}

		private void Update()
		{
			if (_addedVoxels.Count > 0)
			{
				_networkManager.SendResponseToAll(new AddedVoxelResponse(_addedVoxels));
				_addedVoxels.Clear();
			}
			if (_removedPositions.Count > 0)
			{
				_networkManager.SendResponseToAll(new RemovedPositionResponse(_removedPositions));
				_removedPositions.Clear();
			}
		}

		private void OnVoxelsAdded(IReadOnlyList<Voxel> addedVoxels)
		{
			_addedVoxels.AddRange(addedVoxels);
		}

		private void OnVoxelsRemoved(IReadOnlyList<Vector3Ushort> removedVoxels)
		{
			_removedPositions.AddRange(removedVoxels);
		}
	}

}