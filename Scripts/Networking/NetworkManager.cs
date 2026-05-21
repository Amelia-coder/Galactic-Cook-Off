using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Networking.LANComponents;

//todo: make universal(so that it will be able to work with both lan and outisde-hosted lobbies
namespace Scripts.Networking
{
	public partial class NetworkManager : Node
	{
		[Export] public int MaxPlayers = 8;

		public event Action Connected;
		public event Action Disconnected;
		public event Action ConnectionFailed;
		public event Action<long> PlayerJoined;
		public event Action<long> PlayerLeft;

		private ENetMultiplayerPeer _peer;

		public bool IsServer => Multiplayer.IsServer();
		public long MyId => Multiplayer.GetUniqueId();

		public override void _Ready()
		{
			Multiplayer.PeerConnected += id => PlayerJoined?.Invoke(id);
			Multiplayer.PeerDisconnected += id => PlayerLeft?.Invoke(id);
			Multiplayer.ConnectedToServer += () => Connected?.Invoke();
			Multiplayer.ConnectionFailed += () => ConnectionFailed?.Invoke();
			Multiplayer.ServerDisconnected += () => Disconnected?.Invoke();
		}

		public Error Host(int port)
		{
			Disconnect();
			_peer = new ENetMultiplayerPeer();
			_peer.SetBindIP("0.0.0.0");
			var err = _peer.CreateServer(port, MaxPlayers);
			if (err == Error.Ok)
				Multiplayer.MultiplayerPeer = _peer;
			return err;
		}

		public Error Join(string ip, int port)
		{
			Disconnect();
			_peer = new ENetMultiplayerPeer();
			var err = _peer.CreateClient(ip, port);
			if (err == Error.Ok)
				Multiplayer.MultiplayerPeer = _peer;
			GD.Print("We joined!");
			return err;
		}

		public void Disconnect()
		{
			_peer?.Close();
			_peer = null;
			Multiplayer.MultiplayerPeer = null;
		}
	}
}
