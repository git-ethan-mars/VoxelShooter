using UnityEngine;

public class MinimapRenderer : MonoBehaviour
{
	private const string DrawTexture = "DrawTexture";
	private const string SaveTopVoxels = "SaveTopVoxels";

	private static readonly int Pixels = Shader.PropertyToID("Pixels");
	private static readonly int Result = Shader.PropertyToID("Result");
	private static readonly int VertexBuffer = Shader.PropertyToID("VertexBuffer");
	private static readonly int MeshPositionOffset = Shader.PropertyToID("MeshPositionOffset");

	[SerializeField]
	private ComputeShader computeShader;

	[SerializeField]
	private RenderTexture renderTexture;

	[ContextMenu("Draw Full Map")]
	private void DrawChunk()
	{
		renderTexture = new RenderTexture(512, 512, 24);
		renderTexture.enableRandomWrite = true;
		renderTexture.Create();
		var saveTopVoxelsKernel = computeShader.FindKernel(SaveTopVoxels);
		const int colorSize = sizeof(float) * 4;
		var meshFilters = FindObjectsOfType<MeshFilter>();
		using var pixels = new ComputeBuffer(renderTexture.width * renderTexture.height, colorSize);
		for (var i = 0; i < meshFilters.Length; i++)
		{
			var mesh = meshFilters[i].sharedMesh;
			if (mesh == null)
			{
				continue;
			}

			var totalCount = mesh.vertexCount;
			if (totalCount == 0)
			{
				continue;
			}

			mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
			using var vertexBuffer = mesh.GetVertexBuffer(0);
			computeShader.SetBuffer(saveTopVoxelsKernel, VertexBuffer, vertexBuffer);
			computeShader.SetBuffer(saveTopVoxelsKernel, Pixels, pixels);
			computeShader.SetVector(MeshPositionOffset,
				meshFilters[i].gameObject.transform.position);
			computeShader.Dispatch(saveTopVoxelsKernel,
				Mathf.CeilToInt((float) totalCount / 32), 1, 1);
		}

		var drawTextureKernel = computeShader.FindKernel(DrawTexture);
		computeShader.SetBuffer(drawTextureKernel, Pixels, pixels);
		computeShader.SetTexture(drawTextureKernel, Result, renderTexture);
		computeShader.Dispatch(drawTextureKernel, renderTexture.width / 8,
			renderTexture.height / 8, 1);
	}
}