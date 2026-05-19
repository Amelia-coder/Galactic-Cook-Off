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
		public event Action Connected;
		public event Action ConnectionFailed;
		private ENetMultiplayerPeer _peer;
		private bool _bound;


		public override void _Ready()
		{
			if (_bound)
				return;

			_bound = true;
			Multiplayer.PeerConnected += OnPeerConnected;
			Multiplayer.PeerDisconnected += OnPeerDisconnected;

			Multiplayer.ConnectedToServer += OnConnectedToServer;
			Multiplayer.ConnectionFailed += OnConnectionFailed;
			Multiplayer.ServerDisconnected += OnServerDisconnected;
		}

		public void Host()
		{
			Multiplayer.MultiplayerPeer = null;
			GD.Print("[NET] Host() called");
			_peer = new ENetMultiplayerPeer();
			_peer.SetBindIP("0.0.0.0");

			var err = _peer.CreateServer(65000, 5);

			if (err != Error.Ok)
			{
				GD.PrintErr(
					$"[NET] Failed to create host: {err}");

				return;
			}
			else
			{
				GD.Print("!!!!!!!!!!!!");
			}

			Multiplayer.MultiplayerPeer = _peer;
		}

		public void Join(string ip, int port)
		{
			Multiplayer.MultiplayerPeer = null;
			_peer = new ENetMultiplayerPeer();
			_peer.CreateClient(ip, port);

			Multiplayer.MultiplayerPeer = _peer;
		
		}

		private void OnPeerConnected(long id)
		{
			GD.Print($"[NET] Peer connected: {id}, MyID: {Multiplayer.GetUniqueId()}");
		}

		private void OnPeerDisconnected(long id)
		{
			GD.Print($"[NET] Peer disconnected: {id}");
		}

		private void OnConnectedToServer()
		{
			GD.Print("[NET] Connected to server");
		}

		private void OnConnectionFailed()
		{
			GD.PrintErr("[NET] Connection failed");
		}

		private void OnServerDisconnected()
		{
			GD.PrintErr("[NET] Disconnected from server");
		}
	}
}
