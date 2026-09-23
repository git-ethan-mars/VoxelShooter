using VoxelMap;
namespace GamePlay
{
	public interface IMeshFactory
	{
		void CreateFallingMesh(MeshData meshData, FallingMeshParticlePool fallingMeshParticlePool);
	}
}