using VoxelMap;
namespace GamePlay.MapFeatures
{
	public interface IMeshFactory
	{
		void CreateFallingMesh(MeshData meshData, FallingMeshParticlePool fallingMeshParticlePool);
	}
}