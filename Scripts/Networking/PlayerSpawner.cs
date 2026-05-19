using Godot;

namespace Scripts.Networking
{
	public partial class PlayerSpawner : MultiplayerSpawner
	{
		private Vector3 SpawnPoint = new Vector3(0, 3.657f, 0);  
		[Export]
		public PackedScene PlayerScene;

		[Export]
		public Node PlayersContainer;

		public override void _Ready()
		{
			SpawnPath =
				PlayersContainer.GetPath();

			AddSpawnableScene(
				PlayerScene.ResourcePath);

			if (!Multiplayer.IsServer())
				return;

			Multiplayer.PeerConnected +=
				OnPeerConnected;

			Multiplayer.PeerDisconnected +=
				OnPeerDisconnected;

			SpawnPlayer(
				Multiplayer.GetUniqueId());
		}

		private void OnPeerConnected(long id)
		{
			SpawnPlayer(id);
		}

		private void OnPeerDisconnected(long id)
		{
			var player =
				PlayersContainer
					.GetNodeOrNull(id.ToString());

			player?.QueueFree();
		}

		private void SpawnPlayer(long id)
		{
			var player =
				PlayerScene.Instantiate<Node3D>();

			player.Name = id.ToString();

			player.GlobalPosition = SpawnPoint;
			player.SetMultiplayerAuthority(
				(int)id);

			PlayersContainer.AddChild(player);

			GD.Print(
				$"Spawned player {id}");
		}
	}
}
