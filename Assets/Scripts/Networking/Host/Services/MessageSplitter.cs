using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using Networking.Messages.Responses;
using VoxelMap;

namespace Networking.Host.Services
{
	public static class MessageSplitter
	{
		public static T[] SplitBlocksIntoMessages<T>(IList<BlockDataWithPosition> blocks,
			int maxPacketSize)
		{
			var messages = new List<T>();
			var blockBuffer = new List<BlockDataWithPosition>();
			for (var i = 0; i < blocks.Count; i++)
			{
				blockBuffer.Add(blocks[i]);
				if ((blockBuffer.Count + 1) * sizeof(int) * 4 >= maxPacketSize)
				{
					messages.Add((T) Activator.CreateInstance(typeof(T), blocks.ToArray()));
					blockBuffer.Clear();
				}
			}

			if (blockBuffer.Count > 0)
			{
				messages.Add((T) Activator.CreateInstance(typeof(T), blocks.ToArray()));
			}

			return messages.ToArray();
		}

		public static DownloadMapResponse[] SplitBytesIntoMessages(byte[] bytes, int maxPacketSize)
		{
			var messages = new List<DownloadMapResponse>();
			for (var i = 0; i < bytes.Length; i += maxPacketSize)
			{
				var lastByte = Math.Max(i + maxPacketSize, bytes.Length);
				messages.Add(new DownloadMapResponse(bytes[i..lastByte], bytes.Length, i));
			}

			return messages.ToArray();
		}

		public static void SendMessages<T>(T[] messages, float delayInSeconds,
			bool onlyReady = false, NetworkConnectionToClient connection = null) where T : struct, NetworkMessage
		{
			if (connection is null)
			{
				if (onlyReady)
				{
					SendAsync(messages, delayInSeconds, message => NetworkServer.SendToReady(message)).Forget();
				}
				else
				{
					SendAsync(messages, delayInSeconds, message => NetworkServer.SendToAll(message)).Forget();
				}
			}
			else
			{
				if (onlyReady && connection.isReady || !onlyReady)
				{
				}
			}
		}

		private static async UniTaskVoid SendAsync<T>(IReadOnlyList<T> messages, float delayInSeconds,
			Action<T> sendDelegate) where T : struct, NetworkMessage
		{
			for (var i = 0; i < messages.Count; i++)
			{
				sendDelegate.Invoke(messages[i]);
				await UniTask.WaitForSeconds(delayInSeconds);
			}
		}
	}
}