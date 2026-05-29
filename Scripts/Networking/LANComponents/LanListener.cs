using Godot;
using System;
using System.Text;


namespace Scripts.Networking.LANComponents
{
	public partial class LanListener : Node
	{
		[Export] public int ListenPort = 65001;

		private PacketPeerUdp _udp;

		public event Action<ServerInfo> ServerDiscovered;

		public override void _Ready()
		{
			_udp = new PacketPeerUdp();
		}

		public void StartListening()
		{
			var err = _udp.Bind(ListenPort);

			if (err != Error.Ok)
			{
				GD.PrintErr(
					$"[LAN] Failed to bind port {ListenPort}");
				return;
			}

			GD.Print($"[LAN] Listening on {ListenPort}");
		}

		public void StopListening()
		{
			_udp.Close();

			GD.Print("[LAN] Listener stopped");
		}

		public override void _Process(double delta)
		{
			while (_udp.GetAvailablePacketCount() > 0)
			{
				var packet = _udp.GetPacket();

				string message =
					Encoding.UTF8.GetString(packet);

				string ip = _udp.GetPacketIP();

				ParsePacket(message, ip);
			}
		}

		private void ParsePacket(string message, string sourceIp)
		{
			// Example:
			// GAME_SERVER|KitchenWar|65000

			var split = message.Split('|');

			if (split.Length < 3)
				return;

			if (split[0] != "GAME_SERVER")
				return;

			string serverName = split[1];

			if (!int.TryParse(split[2], out int port))
				return;

			string reportedIp = split[3];

			// Prefer the source IP (what the network actually sees),
			// fall back to self-reported if they differ
			var info = new ServerInfo(serverName, sourceIp, port);

			GD.Print($"[LAN] Found server {info.Name} at {info.Ip}:{info.Port}" +
					 (sourceIp != reportedIp ? $" (host reports {reportedIp})" : ""));

			ServerDiscovered?.Invoke(info);
		}
	}
}
