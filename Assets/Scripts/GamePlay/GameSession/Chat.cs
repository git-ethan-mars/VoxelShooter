using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;

namespace GamePlay
{
	public class Chat
	{
		public const int MaxMessageLength = 120;
		private const string CommandPrefix = "/";

		private readonly VSNetworkManager _networkManager;
		private readonly Func<NetworkConnectionToClient, string> _getNickName;
		private readonly Dictionary<string, ChatCommand> _commands = new Dictionary<string, ChatCommand>(StringComparer.OrdinalIgnoreCase);
		private readonly Subject<ChatMessageResponse> _messageReceived = new Subject<ChatMessageResponse>();

		public Observable<ChatMessageResponse> MessageReceived => _messageReceived;

		private Chat(VSNetworkManager networkManager, Func<NetworkConnectionToClient, string> getNickName)
		{
			_networkManager = networkManager;
			_getNickName = getNickName;
		}

		public static Chat Create(VSNetworkManager networkManager, Func<NetworkConnectionToClient, string> getNickName)
		{
			var chat = new Chat(networkManager, getNickName);

			networkManager.MessageReceived.OfMessageType<ChatMessageRequest>()
				.Subscribe(directedMessage => chat.OnMessageRequest(directedMessage.Connection, directedMessage.Message.Text))
				.AddTo(networkManager);
			networkManager.MessageReceived.OfMessageType<ChatMessageResponse>()
				.Subscribe(directedMessage => chat._messageReceived.OnNext(directedMessage.Message))
				.AddTo(networkManager);

			chat.RegisterCommand(new ChatCommand("help", "/help", "Show the available commands", _ => chat.GetHelp()));
			return chat;
		}

		public void Send(string text)
		{
			text = Sanitize(text);

			if (text.Length > 0)
			{
				_networkManager.SendRequest(new ChatMessageRequest(text));
			}
		}

		// Commands are executed on the server and are available only to the host.
		public void RegisterCommand(ChatCommand command)
		{
			_commands[command.Name] = command;
		}

		public void SendSystemMessage(string text)
		{
			_networkManager.SendResponseToAll(new ChatMessageResponse(ChatMessageType.System, null, text));
		}

		private void OnMessageRequest(NetworkConnectionToClient connection, string text)
		{
			string nickName = _getNickName(connection);
			text = Sanitize(text);

			if (nickName == null || text.Length == 0)
			{
				return;
			}

			if (text.StartsWith(CommandPrefix))
			{
				ExecuteCommand(connection, text.Substring(CommandPrefix.Length));
				return;
			}

			_networkManager.SendResponseToAll(new ChatMessageResponse(ChatMessageType.Player, nickName, text));
		}

		private void ExecuteCommand(NetworkConnectionToClient connection, string commandLine)
		{
			if (connection != NetworkServer.localConnection)
			{
				Reply(connection, "Only the host can use commands");
				return;
			}

			string[] parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length == 0 || !_commands.TryGetValue(parts[0], out ChatCommand command))
			{
				Reply(connection, $"Unknown command. Type {CommandPrefix}help");
				return;
			}

			string reply = command.Execute(parts.Skip(1).ToArray());

			if (!string.IsNullOrEmpty(reply))
			{
				Reply(connection, reply);
			}
		}

		private void Reply(NetworkConnectionToClient connection, string text)
		{
			_networkManager.SendResponse(connection, new ChatMessageResponse(ChatMessageType.CommandReply, null, text));
		}

		private string GetHelp()
		{
			return string.Join("\n", _commands.Values.Select(command => $"{command.Usage} - {command.Description}"));
		}

		private static string Sanitize(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}

			text = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
			return text.Length > MaxMessageLength ? text.Substring(0, MaxMessageLength) : text;
		}
	}
}
