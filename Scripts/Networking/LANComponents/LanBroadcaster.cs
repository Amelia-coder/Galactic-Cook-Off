using Godot;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Scripts.Networking.LANComponents
{
	public partial class LanBroadcaster : Node
	{
		[Export] public int BroadcastPort = 65001;

		[Export] public string ServerName = "Cooking Server";

		[Export] public int GamePort = 65000;

		private PacketPeerUdp _udp;

		private Timer _timer;


		public override void _Ready()
		{
			_udp = new PacketPeerUdp();

			// Required for LAN broadcast packets
			_udp.SetBroadcastEnabled(true);

			// Internal periodic broadcaster
			_timer = new Timer();

			_timer.WaitTime = 1.0;
			_timer.Autostart = false;
			_timer.OneShot = false;

			_timer.Timeout += Broadcast;

			AddChild(_timer);
		}

		public void Init(string serverName, int gamePort)
		{
			ServerName = serverName;
			GamePort = gamePort;
		}

		public void StartBroadcasting()
		{
			GD.Print("[LAN] Broadcasting started");

			_timer.Start();
		}

		public void StopBroadcasting()
		{
			GD.Print("[LAN] Broadcasting stopped");

			_timer.Stop();
		}

		private string GetLocalIp()
		{
			string fallback = "unknown";

			foreach (var ip in IP.GetLocalAddresses())
			{
				if (!ip.Contains('.') || ip.StartsWith("127.")) continue;

				// Skip common virtual adapter ranges
				if (ip.StartsWith("169.254.")) continue;  // APIPA (no DHCP)
				if (ip.StartsWith("172.17.")) continue;    // Docker
				if (ip.StartsWith("172.18.")) continue;    // Docker

				// Prefer typical LAN ranges
				if (ip.StartsWith("192.168.") || ip.StartsWith("10."))
					return ip;

				fallback = ip;
			}

			return fallback;
		}


		private void Broadcast()
		{
			string localIp = GetLocalIp(); // same method the lobby uses

			string message =
				$"GAME_SERVER|{ServerName}|{GamePort}|{localIp}";

			byte[] bytes = Encoding.UTF8.GetBytes(message);
			_udp.SetDestAddress("255.255.255.255", BroadcastPort);
			_udp.PutPacket(bytes);
			//// Example:
			//// GAME_SERVER|KitchenWar|65000

			//string message =
			//	$"GAME_SERVER|{ServerName}|{GamePort}";

			//byte[] bytes =
			//	Encoding.UTF8.GetBytes(message);

			//_udp.SetDestAddress(
			//	"255.255.255.255",
			//	BroadcastPort);

			//_udp.PutPacket(bytes);

			//GD.Print($"[LAN] Broadcasted: {message}");
		}

		public string GetBroadcastSourceIp()
		{
			try
			{
				using var socket = new Socket(
					AddressFamily.InterNetwork,
					SocketType.Dgram,
					ProtocolType.Udp);

				socket.Connect("8.8.8.8", 80);
				var localEp = (IPEndPoint)socket.LocalEndPoint;
				return localEp.Address.ToString();
			}
			catch
			{
				return "unknown";
			}
		}
	}
}
