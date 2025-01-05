using Mirror;
using VoxelMap;

namespace Networking.Messages.Responses
{
    public struct FallBlockResponse : IMirrorResponse
    {
        public readonly Voxel[] Voxels;

        public FallBlockResponse(Voxel[] voxels)
        {
            Voxels = voxels;
        }
    }
}