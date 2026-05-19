using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Networking
{
	/// <summary>
	/// Class for custom Player spwaning logic that is more that MultiplayerSynchonixer offers
	/// </summary>
	public partial class PlayerManager : Node
	{
		[Export] public PackedScene PlayerScene;

		public override void _Ready()
		{
			if (!Multiplayer.IsServer())
				return;

			SpawnPlayer(Multiplayer.GetUniqueId()); // host player
		}

		public override void _EnterTree()
		{
			if (Multiplayer.IsServer())
				Multiplayer.PeerConnected += OnPeerConnected;
		}

		private void SpawnPlayer(long id)
		{
			var player = PlayerScene.Instantiate<Node3D>();

			player.Name = id.ToString();

			AddChild(player);

			player.SetMultiplayerAuthority((int)id);

			GD.Print($"Spawned player for {id}");
		}

		private void OnPeerConnected(long id)
		{
			SpawnPlayer(id);
		}

	}
}
